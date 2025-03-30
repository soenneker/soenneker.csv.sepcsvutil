using Microsoft.Extensions.Logging;
using Soenneker.Csv.SepCsvUtil.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using nietras.SeparatedValues;
using Soenneker.Extensions.String;

namespace Soenneker.Csv.SepCsvUtil;

/// <inheritdoc cref="ISepCsvUtil"/>
public class SepCsvUtil : ISepCsvUtil
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

                if (!string.IsNullOrWhiteSpace(propertyValue))
                {
                    object? convertedValue = ConvertPropertyValue(property.PropertyType, propertyValue);
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

    private static object? ConvertPropertyValue(Type targetType, string value)
    {
        if (targetType == typeof(string))
            return value;

        if (Nullable.GetUnderlyingType(targetType) is Type underlying)
        {
            if (value.IsNullOrWhiteSpace())
                return null;

            targetType = underlying;
        }

        if (targetType == typeof(int) && int.TryParse(value, out int intVal))
            return intVal;

        if (targetType == typeof(long) && long.TryParse(value, out long longVal))
            return longVal;

        if (targetType == typeof(short) && short.TryParse(value, out short shortVal))
            return shortVal;

        if (targetType == typeof(ushort) && ushort.TryParse(value, out ushort ushortVal))
            return ushortVal;

        if (targetType == typeof(uint) && uint.TryParse(value, out uint uintVal))
            return uintVal;

        if (targetType == typeof(ulong) && ulong.TryParse(value, out ulong ulongVal))
            return ulongVal;

        if (targetType == typeof(byte) && byte.TryParse(value, out byte byteVal))
            return byteVal;

        if (targetType == typeof(sbyte) && sbyte.TryParse(value, out sbyte sbyteVal))
            return sbyteVal;

        if (targetType == typeof(float) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatVal))
            return floatVal;

        if (targetType == typeof(double) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleVal))
            return doubleVal;

        if (targetType == typeof(decimal) && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalVal))
            return decimalVal;

        if (targetType == typeof(bool) && bool.TryParse(value, out bool boolVal))
            return boolVal;

        if (targetType == typeof(DateTime) && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTimeVal))
            return dateTimeVal;

        if (targetType == typeof(TimeSpan) && TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan timeSpanVal))
            return timeSpanVal;

        if (targetType == typeof(Guid) && Guid.TryParse(value, out Guid guidVal))
            return guidVal;

        if (targetType == typeof(Uri) && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri? uriVal))
            return uriVal;

        if (targetType.IsEnum && Enum.TryParse(targetType, value, ignoreCase: true, out object? enumVal))
            return enumVal;

        return null;
    }
}
