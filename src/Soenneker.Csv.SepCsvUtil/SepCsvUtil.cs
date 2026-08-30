using Microsoft.Extensions.Logging;
using Soenneker.Csv.SepCsvUtil.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using nietras.SeparatedValues;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Type;

namespace Soenneker.Csv.SepCsvUtil;

public sealed class SepCsvUtil : ISepCsvUtil
{
    private readonly ILogger<SepCsvUtil> _logger;

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

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(obj, propertyValue);
                    continue;
                }

                if (propertyValue.IsNullOrWhiteSpace())
                    continue;

                object? convertedValue = property.PropertyType.ConvertPropertyValue(propertyValue);
                if (convertedValue is null)
                    throw new InvalidOperationException($"CSV column '{propertyName}' could not be converted to {property.PropertyType.FullName}.");

                property.SetValue(obj, convertedValue);
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
                    row[property.Name].Set(FormatValue(value));
                else
                    row[property.Name].Set("");
            }
        }
    }

    private static PropertyInfo[] GetCachedProperties(Type type)
    {
        return _propertyCache.GetOrAdd(type, static t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                           .Where(static property => property.CanRead && property.CanWrite &&
                                                               property.GetIndexParameters().Length == 0)
                                                           .ToArray());
    }

    private static string FormatValue(object value) => value switch
    {
        DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
        DateOnly dateOnly => dateOnly.ToString("O", CultureInfo.InvariantCulture),
        TimeOnly timeOnly => timeOnly.ToString("O", CultureInfo.InvariantCulture),
        TimeSpan timeSpan => timeSpan.ToString("c", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
        _ => value.ToString() ?? string.Empty
    };

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
