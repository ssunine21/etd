using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ETD.Scripts.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public static class BackEndJsonUtility
{
    public static readonly JsonConverter[] BackEndJsonConverters =
    {
        new ObjectToIntConverter(),
        new ObjectToFloatConverter(),
        new ObjectToDoubleConverter(),
        new ObjectToStringConverter(),
        new ObjectToEnumConverter(),
        new ObjectToArrayConverter(),
        new ObjectToBooleanConverter(),
    };

    private class ObjectToIntConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = serializer.Deserialize<TypeValue>(reader);
            var sValue = o.S?.ToString();

            if (!string.IsNullOrEmpty(sValue) && int.TryParse(sValue, out var value))
            {
                return value;
            }

            return 0;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    private class ObjectToFloatConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(float);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = serializer.Deserialize<TypeValue>(reader);
            var sValue = o.S?.ToString();

            if (!string.IsNullOrEmpty(sValue) && float.TryParse(sValue, out var value))
            {
                return value;
            }
            return 0f;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    private class ObjectToDoubleConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = serializer.Deserialize<TypeValue>(reader);
            var sValue = o.S?.ToString();

            if (!string.IsNullOrEmpty(sValue) && double.TryParse(sValue, out var value))
            {
                return value;
            }

            return 0d;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    
    private class ObjectToBooleanConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = serializer.Deserialize<TypeValue>(reader);
            var sValue = o.S?.ToString();

            if (!string.IsNullOrEmpty(sValue) && bool.TryParse(sValue, out var value))
            {
                return value;
            }

            return false;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    private class ObjectToStringConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = serializer.Deserialize<TypeValue>(reader);
            var sValue = o.S?.ToString();

            return !string.IsNullOrEmpty(sValue) ? o.S : null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    
    private class ObjectToEnumConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType.BaseType == typeof(Enum);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = serializer.Deserialize<TypeValue>(reader);
            var sValue = o.S?.ToString();

            if (string.IsNullOrEmpty(sValue)) return null;
            return Enum.TryParse(objectType, sValue, out var value) ? value : null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    private class ObjectToArrayConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            if (objectType.BaseType != typeof(Array)) return false;
            var type = objectType.GetElementType();
            return
                type != null
                && (type == typeof(int)
                    || type == typeof(double)
                    || type == typeof(float)
                    || type == typeof(string)
                    || type.BaseType == typeof(Enum));

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var o = serializer.Deserialize<TypeValue>(reader);
            var sValue = o.S?.ToString();

            var type = objectType.GetElementType();
            if (string.IsNullOrEmpty(sValue)) return null;

            
            var tempStr = Regex.Replace(sValue, @"(?<!\\)\[", "");
                tempStr =  Regex.Replace(tempStr, @"(?<!\\)\]", "");
            
            
            
            var strList = Regex.Split(tempStr, @"(?<!\\),")
                .Select(s => s
                    .Replace("\\[", "[")
                    .Replace("\\]", "]")
                    .Replace("\\,", ","))
                .ToArray();
                
            if (type == null) return null;
            
            var typeArray = Array.CreateInstance(type, strList.Length);

            for (var i = 0; i < strList.Length; ++i)
            {
                if (type == typeof(int))
                {
                    if (int.TryParse(strList[i], out var result))
                    {
                        typeArray.SetValue(result, i);
                    }
                }
                else if (type == typeof(double))
                {
                    if (double.TryParse(strList[i], out var result))
                    {
                        typeArray.SetValue(result, i);
                    }
                }
                else if (type == typeof(float))
                {
                    if (float.TryParse(strList[i], out var result))
                    {
                        typeArray.SetValue(result, i);
                    }
                }
                else if (type == typeof(string))
                {
                    var str = strList[i].Trim();
                    typeArray.SetValue(str, i);
                }
                else if (type.BaseType == typeof(Enum))
                {
                    if (Enum.TryParse(type, strList[i], out var result))
                    {
                        typeArray.SetValue(result, i);
                    }
                }
            }
            return typeArray;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    private struct TypeValue
    {
        public object S;
        public object NULL;
    }
}

[System.Serializable]
public class Rows<T>
{
    public List<T> rows;
}