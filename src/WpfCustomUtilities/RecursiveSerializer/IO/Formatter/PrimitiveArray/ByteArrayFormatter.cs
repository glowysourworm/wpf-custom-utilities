using System.IO;

using RecursiveSerializer.Formatter;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Formatter.PrimitiveArray
{
    internal class ByteArrayFormatter : BaseFormatter<byte[]>
    {
        readonly IntegerFormatter _integerFormatter;

        internal ByteArrayFormatter()
        {
            _integerFormatter = new IntegerFormatter();
        }

        protected override byte[] ReadImpl(Stream stream)
        {
            // READ BUFFER LENGTH
            var bufferLength = _integerFormatter.Read(stream);
            var buffer = new byte[bufferLength];

            // READ BUFFER
            stream.Read(buffer, 0, bufferLength);

            return buffer;
        }

        protected override void WriteImpl(Stream stream, byte[] theObject)
        {
            var buffer = new byte[theObject.Length];

            // WRITE BUFFER LENGTH
            _integerFormatter.Write(stream, buffer.Length);

            // WRITE BUFFER 
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
