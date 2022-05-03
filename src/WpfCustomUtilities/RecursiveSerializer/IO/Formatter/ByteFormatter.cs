using System.IO;

namespace RecursiveSerializer.Formatter
{
    public class ByteFormatter : BaseFormatter<byte>
    {
        protected override byte ReadImpl(Stream stream)
        {
            return System.Convert.ToByte(stream.ReadByte());
        }

        protected override void WriteImpl(Stream stream, byte theObject)
        {
            stream.WriteByte(theObject);
        }
    }
}
