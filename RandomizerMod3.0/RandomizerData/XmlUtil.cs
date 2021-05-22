using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.IO;

namespace RandomizerMod.RandomizerData
{
    public static class XmlUtil
    {
        private static Dictionary<Type, Dictionary<string, FieldInfo>> reflectionCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static XmlDocument LoadEmbeddedXml(string embeddedResourcePath)
        {
            Stream stream = typeof(XmlUtil).Assembly.GetManifestResourceStream(embeddedResourcePath);
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            stream.Dispose();
            return doc;
        }

        public static string GetNameAttribute(this XmlNode node)
        {
            return node.Attributes?["name"]?.InnerText;
        }

        public static (string name, T item) DeserializeByReflectionWithName<T>(this XmlNode node) where T : new()
        {
            return (GetNameAttribute(node), DeserializeByReflection<T>(node));
        } 

        public static T DeserializeByReflection<T>(this XmlNode node) where T : new()
        {
            if (!reflectionCache.TryGetValue(typeof(T), out var fieldDict))
            {
                fieldDict = reflectionCache[typeof(T)] = typeof(T).GetFields().ToDictionary(f => f.Name, f => f);
            }

            object def = new T();
            string name = GetNameAttribute(node) ?? typeof(T).Name;
            if (fieldDict.TryGetValue("name", out FieldInfo nameField)) nameField.SetValue(def, name);

            foreach (XmlNode fieldNode in node.ChildNodes)
            {
                if (!fieldDict.TryGetValue(fieldNode.Name, out FieldInfo field)) continue;
                Type type = field.FieldType;
                string stringValue = fieldNode.InnerText;

                if (type == typeof(string))
                {
                    field.SetValue(def, stringValue);
                }
                else if (type == typeof(bool))
                {
                    field.SetValue(def, bool.Parse(stringValue));
                }
                else if (type == typeof(int))
                {
                    field.SetValue(def, int.Parse(stringValue));
                }
                else if (type == typeof(float))
                {
                    field.SetValue(def, float.Parse(stringValue));
                }
                else if (type.IsEnum)
                {
                    field.SetValue(def, Enum.Parse(type, stringValue));
                }

            }

            return (T)def;
        }
    }
}
