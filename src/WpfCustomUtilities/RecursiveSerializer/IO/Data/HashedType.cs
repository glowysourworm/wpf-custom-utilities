using WpfCustomUtilities.RecursiveSerializer.Shared;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Data
{
    /// <summary>
    /// Type for serializing type information to file
    /// </summary>
    internal class HashedType
    {
        internal HashedTypeData Declaring { get; private set; }
        internal HashedTypeData Implementing { get; private set; }

        // CACHE THE HASH CODE FOR PERFORMANCE!
        int _calculatedHashCode;

        internal HashedType(HashedTypeData declaringType, HashedTypeData implementingType)
        {
            this.Declaring = declaringType;
            this.Implementing = implementingType;
        }

        public override bool Equals(object obj)
        {
            var type = obj as HashedType;

            return this.GetHashCode() == type.GetHashCode();
        }

        public override int GetHashCode()
        {
            if (_calculatedHashCode == default(int))
                _calculatedHashCode = RecursiveSerializerTypeFactory.CalculateHashCode(this);

            return _calculatedHashCode;
        }

        public override string ToString()
        {
            return string.Format("Declaring={0}, Implementing={1}", this.Declaring, this.Implementing);
        }
    }
}
