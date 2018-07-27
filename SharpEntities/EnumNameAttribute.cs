using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SharpEntities
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumNameAttribute : Attribute
    {
        public string Name;

        public EnumNameAttribute(string name)
        {
            Name = name;
        }
    }

    public static class EnumExtensions
    {
        public static string GetName(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            FieldInfo field = value.GetType().GetField(name);
            EnumNameAttribute attribute = field.GetCustomAttribute(typeof(EnumNameAttribute)) as EnumNameAttribute;
            if (attribute == null) return name;
            return attribute.Name;
        }
    }
}
