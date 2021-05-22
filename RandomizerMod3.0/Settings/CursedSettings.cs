using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RandomizerMod.Extensions;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class CursedSettings : ICloneable
    {
        public bool RandomCurses;
        public bool RandomizeFocus;
        public bool RandomizeNail;
        public bool LongerProgressionChains;
        public bool ReplaceJunkWithOneGeo;
        public bool RemoveSpellUpgrades;
        public bool SplitClaw;
        public bool SplitCloak;


        private static Dictionary<string, FieldInfo> fields = typeof(CursedSettings)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(f => f.Name, f => f);
        public static string[] FieldNames => fields.Keys.ToArray();

        public void SetFieldByName(string fieldName, object value)
        {
            if (fields.TryGetValue(fieldName, out FieldInfo field))
            {
                field.SetValue(this, value);
            }
        }

        public string ToMultiline()
        {
            StringBuilder sb = new StringBuilder("Curses");
            foreach (var kvp in fields)
            {
                sb.AppendLine($"{kvp.Key.FromCamelCase()}: {kvp.Value.GetValue(this)}");
            }

            return sb.ToString();
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
