using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfCustomUtilities.Extensions
{
    public static class EnumExtension
    {
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var member = value.GetType()
                              .GetMember(value.ToString())
                              .FirstOrDefault();

            if (member == null)
                throw new Exception("No Member Defined for Enum Type");

            var attributes = member.GetCustomAttributes(typeof(T), true);

            return attributes.Any() ? (T)attributes.First() : default(T);
        }

        public static bool Has<T>(this Enum value, T flag) where T : Enum
        {
            return value.HasFlag(flag);
        }

        public static IEnumerable<T> Enumerate<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
