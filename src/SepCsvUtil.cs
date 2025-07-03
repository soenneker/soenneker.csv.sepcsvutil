using Microsoft.Extensions.Logging;
using Soenneker.Csv.SepCsvUtil.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using nietras.SeparatedValues;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Type;

namespace Soenneker.Csv.SepCsvUtil;

/// <inheritdoc cref="ISepCsvUtil"/>
public sealed class SepCsvUtil : ISepCsvUtil
{
    private readonly ILogger<SepCsvUtil> _logger;

    private readonly bool _log = true;

    // TODO: ReflectionCache
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object>> _constructorCache = new();

    public SepCsvUtil(ILogger<SepCsvUtil> logger)
    {
        _logger = logger;
    }

    public List<T> Read<T>(string path)
    {
        _logger.LogDebug("%% CSVUTIL: -- Reading CSV from {path} ...", path);

        using SepReader reader = Sep.Reader().FromFile(path);

        var objects = new List<T>();
        Type type = typeof(T);
        PropertyInfo[] properties = GetCachedProperties(type);

        foreach (SepReader.Row row in reader)
        {
            var obj = CreateInstance<T>();

            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                var propertyValue = row[propertyName].ToString();

                if (!propertyValue.IsNullOrWhiteSpace())
                {
                    object? convertedValue = property.PropertyType.ConvertPropertyValue(propertyValue);
                    if (convertedValue != null)
                        property.SetValue(obj, convertedValue);
                }
            }

            objects.Add(obj);
        }

        _logger.LogDebug("%% CSVUTIL: -- Finished reading CSV");

        return objects;
    }

    public void Write<T>(List<T> objects, string filePath)
    {
        using SepWriter writer = Sep.New(',').Writer(o => o).ToFile(filePath);

        PropertyInfo[] properties = GetCachedProperties(typeof(T));

        foreach (T data in objects)
        {
            using SepWriter.Row row = writer.NewRow();
            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(data);

                if (value != null)
                    row[property.Name].Set(value.ToString());
                else
                    row[property.Name].Set("");
            }
        }
    }

    private static PropertyInfo[] GetCachedProperties(Type type)
    {
        return _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public));
    }

    private static T CreateInstance<T>()
    {
        Type type = typeof(T);

        // For value types (structs), return default without needing a constructor
        if (type.IsValueType)
            return default!;

        Func<object> ctor = _constructorCache.GetOrAdd(type, static t =>
        {
            ConstructorInfo ctorInfo = t.GetConstructor(Type.EmptyTypes)
                                       ?? throw new InvalidOperationException($"Type {t.FullName} does not have a parameterless constructor.");

            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(Expression.New(ctorInfo));
            return lambda.Compile();
        });

        return (T)ctor();
    }
}
