using System;
using System.Xml;

namespace Atlas.Xml.SerializationCompiler
{
    internal class ByteArraySerializer : IXmlSerializer<byte[]>
    {

        private IXmlSerializer<byte[]> _elementSerializer;

        public ByteArraySerializer()
        {
            _elementSerializer = (IXmlSerializer<byte[]>)Compiler.Compile(typeof(byte[]));
        }

        public void Serialize(XmlWriter writer, byte[] objectInstance, SerializationOptions options)
        {
            if (options.ByteArraySerializationType == ByteArraySerializationType.Base64)
                writer.WriteBase64(objectInstance, 0, objectInstance.Length);
            else if (options.ByteArraySerializationType == ByteArraySerializationType.BinHex)
                writer.WriteBinHex(objectInstance);
            else
                _elementSerializer.Serialize(writer, objectInstance, options);
        }

        public byte[] Deserialize(XmlReader reader, SerializationOptions options)
        {
            if (!reader.IsEmptyElement)
            {
                if (options.ByteArraySerializationType == ByteArraySerializationType.Base64)
                {
                    reader.Read();
                    return reader.ReadBase64();
                }
                if (options.ByteArraySerializationType == ByteArraySerializationType.BinHex)
                {
                    reader.Read();
                    return reader.ReadBinHex();
                }
                else
                {
                    return _elementSerializer.Deserialize(reader, options);
                }
            }

            return new byte[] { };
        }

        public void Deserialize(XmlReader reader, byte[] objectInstance, SerializationOptions options)
        {
            throw new NotSupportedException("Array deserialization cannot be done into existing array! Use Deserialize(reader, options) instead.");
        }

        public SerializationOptions DefaultSerializationOptions
        {
            get { return SerializationOptions.Default; }
        }

        public void WriteXml(XmlWriter writer, object objectInstance, SerializationOptions options)
        {
            Serialize(writer, (byte[])objectInstance, options);
        }

        public object ReadXml(XmlReader reader, SerializationOptions options)
        {
            return Deserialize(reader, options);
        }

        public void ReadXml(XmlReader reader, object objectInstance, SerializationOptions options)
        {
            Deserialize(reader, (byte[])objectInstance, options);
        }

    }
}