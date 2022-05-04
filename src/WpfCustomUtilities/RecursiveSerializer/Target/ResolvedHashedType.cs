using System;

using WpfCustomUtilities.RecursiveSerializer.Shared;

namespace WpfCustomUtilities.RecursiveSerializer.Target
{
    internal class ResolvedHashedType
    {
        readonly Type _declaringType;
        readonly Type _implementingType;

        // CACHE THE HASH CODE FOR PERFORMANCE!
        int _calculatedHashCode;

        internal ResolvedHashedType(Type declaringType)
        {
            _declaringType = declaringType;
            _implementingType = declaringType;

            _calculatedHashCode = default(int);
        }
        internal ResolvedHashedType(Type declaringType, Type implementingType)
        {
            _declaringType = declaringType;
            _implementingType = implementingType;

            _calculatedHashCode = default(int);
        }

        internal Type GetDeclaringType()
        {
            return _declaringType;
        }

        internal Type GetImplementingType()
        {
            return _implementingType;
        }

        public override bool Equals(object obj)
        {
            var type = obj as ResolvedHashedType;

            return this.GetHashCode() == type.GetHashCode();
        }

        public override int GetHashCode()
        {
            if (_calculatedHashCode == default(int))
                _calculatedHashCode = RecursiveSerializerTypeFactory.CalculateHashCode(_declaringType, _implementingType);

            return _calculatedHashCode;
        }

        public override string ToString()
        {
            return string.Format("Declaring={0}, Implementing={1}", _declaringType.Name, _implementingType.Name);
        }
    }
}
