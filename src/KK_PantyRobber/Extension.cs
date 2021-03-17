using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Extension
{
    public static class Extension
    {
        private static readonly Dictionary<FieldKey, FieldInfo> _fieldCache = new Dictionary<FieldKey, FieldInfo>();

        public static object GetField(this object self, string name, Type type = null)
        {
            if (type == null) type = self.GetType();
            if (!self.SearchForFields(name))
            {
                Console.WriteLine("[KK_Extension] Field Not Found: " + name);
                return false;
            }

            var key = new FieldKey(type, name);
            if (!_fieldCache.TryGetValue(key, out var value))
            {
                value = key.type.GetField(key.name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.FlattenHierarchy);
                _fieldCache.Add(key, value);
            }

            return value.GetValue(self);
        }

        public static bool SetField(this object self, string name, object value, Type type = null)
        {
            if (type == null) type = self.GetType();
            if (!self.SearchForFields(name))
            {
                Console.WriteLine("[KK_Extension] Field Not Found: " + name);
                return false;
            }

            var key = new FieldKey(type, name);
            if (!_fieldCache.TryGetValue(key, out var value2))
            {
                value2 = key.type.GetField(key.name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.FlattenHierarchy);
                if (value2 != null)
                {
                    _fieldCache.Add(key, value2);
                    value2.SetValue(self, value);
                    return true;
                }

                Console.WriteLine("[KK_Extension] Set Field Not Found: " + name);
            }

            return false;
        }

        public static bool SetProperty(this object self, string name, object value)
        {
            if (!self.SearchForProperties(name))
            {
                Console.WriteLine("[KK_Extension] Field Not Found: " + name);
                return false;
            }

            var property = self.GetType().GetProperty(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
            if (property != null)
            {
                property.SetValue(self, value, null);
                return true;
            }

            Console.WriteLine("[KK_Extension] Set Property Not Found: " + name);
            return false;
        }

        public static object GetProperty(this object self, string name)
        {
            if (!self.SearchForProperties(name))
            {
                Console.WriteLine("[KK_Extension] Property Not Found: " + name);
                return false;
            }

            return self.GetType().GetProperty(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
                .GetValue(self, null);
        }

        public static object Invoke(this object self, string name, object[] p = null)
        {
            try
            {
                return self?.GetType().InvokeMember(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod, null, self, p);
            }
            catch (MissingMethodException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                var array = self?.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public |
                                                       BindingFlags.NonPublic | BindingFlags.FlattenHierarchy |
                                                       BindingFlags.InvokeMethod);
                var list = new List<string>();
                var array2 = array;
                foreach (var memberInfo in array2)
                {
                    if (memberInfo.Name == name) return true;
                    list.Add("[KK_Extension] Member Name/Type: " + memberInfo.Name + " / " + memberInfo.MemberType);
                }

                foreach (var item in list) Console.WriteLine(item);
                Console.WriteLine("[KK_Extension] Get " + array.Length + " Members.");
                return false;
            }
        }

        public static bool SearchForFields(this object self, string name)
        {
            var fields = self.GetType().GetFields(AccessTools.all);
            var list = new List<string>();
            var array = fields;
            foreach (var fieldInfo in array)
            {
                if (fieldInfo.Name == name) return true;
                list.Add("[KK_Extension] Field Name/Type: " + fieldInfo.Name + " / " + fieldInfo.FieldType);
            }

            Console.WriteLine("[KK_Extension] Get " + fields.Length + " Fields.");
            foreach (var item in list) Console.WriteLine(item);
            return false;
        }

        public static bool SearchForProperties(this object self, string name)
        {
            var properties = self.GetType().GetProperties(AccessTools.all);
            var list = new List<string>();
            var array = properties;
            foreach (var propertyInfo in array)
            {
                if (propertyInfo.Name == name) return true;
                list.Add("[KK_Extension] Property Name/Type: " + propertyInfo.Name + " / " + propertyInfo.PropertyType);
            }

            Console.WriteLine("[KK_Extension] Get " + properties.Length + " Properties.");
            foreach (var item in list) Console.WriteLine(item);
            return false;
        }

        public static object GetNonPublicField(this object obj, string name)
        {
            return obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(obj);
        }

        public static T GetNonPublicField<T>(this object obj, string name)
        {
            return (T) obj.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
        }

        public static void SetNonPublicField<T>(this object obj, string name, object value)
        {
            obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SetValue(obj, value);
        }

        private struct FieldKey
        {
            public readonly Type type;

            public readonly string name;

            private readonly int _hashCode;

            public FieldKey(Type inType, string inName)
            {
                type = inType;
                name = inName;
                _hashCode = type.GetHashCode() ^ name.GetHashCode();
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
    }
}