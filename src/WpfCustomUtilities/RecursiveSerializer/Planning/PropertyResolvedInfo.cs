using System.Reflection;

using WpfCustomUtilities.Extensions;

using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Planning
{
    internal class PropertyResolvedInfo
    {
        public string PropertyName { get; set; }
        public object ResolvedObject { get; set; }
        public ResolvedHashedType ResolvedType { get; set; }

        readonly PropertyInfo _reflectedInfo;

        internal PropertyResolvedInfo(PropertyInfo reflectedInfo)
        {
            _reflectedInfo = reflectedInfo;
        }

        internal PropertyInfo GetReflectedInfo()
        {
            if (_reflectedInfo == null)
                throw new System.Exception("Internal error - Reflected property info is null: PropertyResolvedInfo.cs");

            return _reflectedInfo;
        }

        internal bool AreEqualProperties(PropertyResolvedInfo info)
        {
            return this.PropertyName.Equals(info.PropertyName) &&
                   this.ResolvedType.Equals(info.ResolvedType);
        }

        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
