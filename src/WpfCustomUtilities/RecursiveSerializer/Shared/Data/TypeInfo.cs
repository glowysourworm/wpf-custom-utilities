using System;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;

namespace WpfCustomUtilities.RecursiveSerializer.Shared.Data
{
    public struct TypeInfo
    {
        public string Assembly { get; set; }
        public string Name { get; set; }

        public TypeInfo(Type type)
        {
            this.Assembly = type.Assembly.FullName;
            this.Name = type.Name;
        }

        public TypeInfo(string assembly, string name)
        {
            this.Assembly = assembly;
            this.Name = name;
        }

        internal TypeInfo(HashedTypeData hashedType)
        {
            this.Assembly = hashedType.Assembly;
            this.Name = hashedType.Type;
        }
    }
}
