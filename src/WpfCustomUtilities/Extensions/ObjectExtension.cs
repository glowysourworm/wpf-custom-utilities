using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WpfCustomUtilities.Extensions
{
    public static class ObjectExtension
    {
        /// <summary>
        /// Returns the First or Default attribute of the supplied type for the supplied object
        /// </summary>
        public static T GetAttribute<T>(this object value) where T : System.Attribute
        {
            var attributes = value.GetType().GetCustomAttributes(typeof(T), true);

            return attributes.Any() ? (T)attributes.First() : default(T);
        }

        public static bool ImplementsInterface<T>(this object value)
        {
            return value.GetType()
                        .GetInterfaces()
                        .Any(type => type.Equals(typeof(T)));
        }

        public static PropertyInfo GetPropertyInfo<T, V>(this T theObject, Expression<Func<T, V>> propertySelector)
        {
            var unaryExpression = propertySelector.Body as UnaryExpression;

            if (unaryExpression == null)
                throw new Exception("Invalid use of property selector ObjectExtension.GetPropertyInfo<T, V>");

            var memberInfo = unaryExpression.Operand as MemberExpression;

            if (memberInfo == null)
                throw new Exception("Invalid use of property selector ObjectExtension.GetPropertyInfo<T, V>");

            var propertyInfo = memberInfo.Member as PropertyInfo;

            if (propertyInfo == null)
                throw new Exception("Invalid use of property selector ObjectExtension.GetPropertyInfo<T, V>");

            return propertyInfo;
        }

        /// <summary>
        /// Creates formatted string of object data using property reflection
        /// </summary>
        public static string FormatToString<T>(this T theObject)
        {
            var properties = typeof(T).GetProperties();

            var result = string.Join(", ", properties.Select(property => property.Name + "=" + property.GetValue(theObject)?.ToString()));

            return "{" + result + " }";
        }
    }
}
