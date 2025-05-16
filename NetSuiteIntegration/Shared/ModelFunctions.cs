using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace NetSuiteIntegration.Shared
{
    public static class ModelFunctions
    {
        public static Type GetBaseType(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var propertyType = property.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);

            return underlyingType ?? propertyType;
        }

        public static Type? GetPropertyType(string modelTypeName, string propertyName)
        {
            // Get the type of the model
            var modelType = Type.GetType(modelTypeName);
            if (modelType == null)
            {
                throw new ArgumentException($"Model type '{modelTypeName}' not found.");
            }

            // Get the property info
            var propertyInfo = modelType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found in model type '{modelTypeName}'.");
            }

            // Return the property type
            return propertyInfo.PropertyType;
        }

        public static string? GetDisplayName(this PropertyInfo? property)
        {
            string? displayName = null;

            if (property != null)
            {
                string? displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                string? displayAttribute = property.GetCustomAttribute<DisplayAttribute>()?.Name;
                if (displayNameAttribute != null)
                    displayName = displayNameAttribute;
                else if (displayAttribute != null)
                    displayName = displayAttribute;
                else
                    displayName = GetFieldNameWithSpaces(property.Name, true);
            }

            return displayName;
        }

        public static string? GetDisplayGroup(this PropertyInfo? property)
        {
            //Used to define a bool property where true and false values format this property differently
            return property?.GetCustomAttribute<DisplayAttribute>()?.GroupName;
        }

        public static string? GetEnumDisplayName(this Enum? enumValue)
        {
            return enumValue?
                .GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .Name ?? enumValue?.ToString(); //Default option
        }

        public static string? GetEnumDescription(this Enum? enumValue)
        {
            return enumValue?
                .GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description;
        }

        public static string GetFieldNameWithSpaces(this string? text, bool? preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        ((bool)(preserveAcronyms ?? false) && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static bool IsNullableEnum(this Type? t)
        {
            Type? u = Nullable.GetUnderlyingType(t);
            return u != null && u.IsEnum;
        }

        public static bool? GetIsTrue(this object? item, string? property)
        {
            return (bool?)item?.GetType().GetProperty(property ?? "")?.GetValue(item) ?? false;
        }

        public static T? Clone<T>(this T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json);
        }

        public static void CopyProperties<T>(this T source, T target)
        {
            if (source == null || target == null)
                throw new ArgumentNullException("Source or/and Target is null");

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    var value = property.GetValue(source);
                    property.SetValue(target, value);
                }
            }
        }

        public static string Format(this object value, string format, IFormatProvider formatProvider = null)
        {
            if (value == null)
            {
                return string.Empty;
            }

            IFormattable formattable = value as IFormattable;

            if (formattable != null)
            {
                return formattable.ToString(format, formatProvider);
            }

            //throw new ArgumentException("value");
            return string.Empty;
        }
    }
}
