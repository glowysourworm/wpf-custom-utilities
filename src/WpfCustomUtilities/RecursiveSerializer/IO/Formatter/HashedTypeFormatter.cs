using System.IO;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;

namespace RecursiveSerializer.Formatter
{
    /// <summary>
    /// RECURSIVE FORMATTER! NEEDED TO STORE TYPE DATA FOR OBJECTS THAT DON'T MATCH
    /// THEIR DECLARED TYPES
    /// </summary>
    internal class HashedTypeFormatter : BaseFormatter<HashedType>
    {
        readonly StringFormatter _stringFormatter;
        readonly IntegerFormatter _integerFormatter;
        readonly BooleanFormatter _booleanFormatter;

        internal HashedTypeFormatter()
        {
            _stringFormatter = new StringFormatter();
            _integerFormatter = new IntegerFormatter();
            _booleanFormatter = new BooleanFormatter();
        }

        protected override HashedType ReadImpl(Stream stream)
        {
            var hasTypeDiscrepancy = _booleanFormatter.Read(stream);

            var declaringData = ReadHashData(stream);

            if (hasTypeDiscrepancy)
            {
                var implementingData = ReadHashData(stream);

                return new HashedType(declaringData, implementingData);
            }

            return new HashedType(declaringData, declaringData);
        }

        protected override void WriteImpl(Stream stream, HashedType theObject)
        {
            var hasTypeDiscrepancy = !theObject.Declaring.Equals(theObject.Implementing);

            // WRITE EXTRA BOOLEAN -> "HAS TYPE DISCREPANCY"
            _booleanFormatter.Write(stream, hasTypeDiscrepancy);

            WriteHashData(stream, theObject.Declaring);

            if (hasTypeDiscrepancy)
            {
                WriteHashData(stream, theObject.Implementing);
            }
        }

        private HashedTypeData ReadHashData(Stream stream)
        {
            var assembly = _stringFormatter.Read(stream);
            var type = _stringFormatter.Read(stream);
            var isGeneric = _booleanFormatter.Read(stream);
            var isEnum = _booleanFormatter.Read(stream);

            // RECURSE USING LOOP -> EXPECTED TO SELF TERMINATE!
            var argumentCount = _integerFormatter.Read(stream);
            var arguments = new HashedTypeData[argumentCount];

            for (int index = 0; index < argumentCount; index++)
            {
                arguments[index] = ReadHashData(stream);
            }

            var enumUnderlyingType = HashedTypeData.Empty;

            if (isEnum)
                enumUnderlyingType = ReadHashData(stream);

            return new HashedTypeData(assembly, type, isGeneric, isEnum, arguments, enumUnderlyingType);
        }

        private void WriteHashData(Stream stream, HashedTypeData data)
        {
            _stringFormatter.Write(stream, data.Assembly);
            _stringFormatter.Write(stream, data.Type);
            _booleanFormatter.Write(stream, data.IsGeneric);
            _booleanFormatter.Write(stream, data.IsEnum);

            // RECURSE USING LOOP -> EXPECTED TO SELF TERMINATE!
            _integerFormatter.Write(stream, data.GenericArguments.Length);

            foreach (var argument in data.GenericArguments)
                WriteHashData(stream, argument);

            if (data.IsEnum)
                WriteHashData(stream, data.EnumUnderlyingType);
        }
    }
}
