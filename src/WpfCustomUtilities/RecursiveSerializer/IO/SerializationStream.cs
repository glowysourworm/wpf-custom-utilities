using RecursiveSerializer.Formatter;

using System;
using System.Collections.Generic;
using System.IO;

using WpfCustomUtilities.RecursiveSerializer.Interface;
using WpfCustomUtilities.RecursiveSerializer.IO.Interface;
using WpfCustomUtilities.RecursiveSerializer.Manifest;

namespace WpfCustomUtilities.RecursiveSerializer.IO
{
    /// <summary>
    /// Wrapper for USER STREAM to perform serialization
    /// </summary>
    internal class SerializationStream : ISerializationStreamReader, ISerializationStreamWriter
    {
        // Collection of formatters for serialization
        Dictionary<Type, IBaseFormatter> _formatters;

        readonly Stream _stream;
        readonly List<SerializedStreamData> _streamData;

        public SerializationStream(Stream stream)
        {
            _stream = stream;
            _streamData = new List<SerializedStreamData>();
            _formatters = new Dictionary<Type, IBaseFormatter>();
        }

        public IEnumerable<SerializedStreamData> GetStreamData()
        {
            return _streamData;
        }

        public bool SupportsType(Type type)
        {
            return FormatterFactory.CanCreateFormatter(type);
        }

        public void Write<T>(T theObject)
        {
            Write(theObject, typeof(T));
        }

        public void Write(object theObject, Type theObjectType)
        {
            var formatter = SelectFormatter(theObjectType);

            Write(formatter, theObject);
        }

        public T Read<T>()
        {
            return (T)Read(typeof(T));
        }

        public object Read(Type type)
        {
            var formatter = SelectFormatter(type);

            object result = null;

            Read(formatter, out result);

            return result;
        }

        private void Read(IBaseFormatter formatter, out object theObject)
        {
            theObject = formatter.Read(_stream);
        }

        private void Write(IBaseFormatter formatter, object theObject)
        {
            formatter.Write(_stream, theObject);
        }

        private IBaseFormatter SelectFormatter(Type type)
        {
            if (_formatters.ContainsKey(type))
                return _formatters[type];

            IBaseFormatter formatter = null;

            if (FormatterFactory.IsPrimitiveSupported(type))
            {
                formatter = FormatterFactory.CreatePrimitiveFormatter(type);
            }
            else
            {
                formatter = FormatterFactory.CreateFormatter(type);
            }

            _formatters.Add(type, formatter);

            return formatter;
        }
    }
}
