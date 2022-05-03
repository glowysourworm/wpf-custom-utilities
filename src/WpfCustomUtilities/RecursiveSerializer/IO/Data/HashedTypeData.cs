using System;

using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Data
{
    internal class HashedTypeData
    {
        internal static readonly HashedTypeData Empty = new HashedTypeData()
        {
            Assembly = default(string),
            Type = default(string),
            IsGeneric = default(bool),
            IsEnum = default(bool),
            GenericArguments = new HashedTypeData[] { },
            EnumUnderlyingType = HashedTypeData.Empty
        };

        internal string Assembly { get; private set; }
        internal string Type { get; private set; }
        internal bool IsGeneric { get; private set; }
        internal bool IsEnum { get; private set; }

        /// <summary>
        /// DECLARING Generic arguments
        /// </summary>
        internal HashedTypeData[] GenericArguments { get; private set; }
        internal HashedTypeData EnumUnderlyingType { get; private set; }

        // CACHE FOR PERFORMANCE!
        int _calculatedHashCode;

        private HashedTypeData()
        {
            _calculatedHashCode = default(int);
        }

        internal HashedTypeData(string assembly, string type, bool isGeneric, bool isEnum, HashedTypeData[] genericArguments, HashedTypeData enumUnderlyingType)
        {
            if (isEnum && ReferenceEquals(enumUnderlyingType, HashedTypeData.Empty))
                throw new ArgumentException("Invalid enum type sent to HashedTypeData");

            this.Assembly = assembly;
            this.Type = type;
            this.IsGeneric = isGeneric;
            this.IsEnum = isEnum;

            this.GenericArguments = genericArguments;
            this.EnumUnderlyingType = isEnum ? enumUnderlyingType : HashedTypeData.Empty;

            _calculatedHashCode = default(int);
        }

        public override bool Equals(object obj)
        {
            var hashedTypeData = obj as HashedTypeData;

            return this.GetHashCode() == hashedTypeData.GetHashCode();
        }

        public override int GetHashCode()
        {
            if (_calculatedHashCode == default(int))
            {
                var baseHash = RecursiveSerializerHashGenerator.CreateSimpleHash(this.Assembly,
                                                                             this.Type,
                                                                             this.IsGeneric,
                                                                             this.IsEnum,
                                                                             this.GenericArguments);

                if (this.IsEnum)
                    baseHash = RecursiveSerializerHashGenerator.CreateSimpleHash(baseHash, this.EnumUnderlyingType);

                // CACHE RESULT!
                _calculatedHashCode = baseHash;
            }

            return _calculatedHashCode;
        }

        public override string ToString()
        {
            return string.Format("Hash={0}, Type={1}", this.GetHashCode(), this.Type);
        }
    }
}
