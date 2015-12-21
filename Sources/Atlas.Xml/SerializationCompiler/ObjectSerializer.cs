using System.Xml;

namespace Atlas.Xml.SerializationCompiler
{
    /// <summary>
    /// Xml Serializer for boxed types
    /// </summary>
    internal class ObjectSerializer : IXmlSerializer<object>
    {

        /// <summary>
        /// Serializes object instance into writer. First attribute will be written as type attribute to element. (e.g.: _type="System.Int32")
        /// </summary>
        /// <param name="writer">Writer to write into</param>
        /// <param name="objectInstance">Instance of object</param>
        /// <param name="options">Writer settings</param>
        public void Serialize(XmlWriter writer, object objectInstance, SerializationOptions options)
        {
            var realType = objectInstance.GetType();
            writer.WriteAttributeString(Serializer.TypeAttributeName, realType.GetNameForGetType());
            if (realType != typeof(object))
                SerializerFactory.GetSerializer(realType).WriteXml(writer, objectInstance, options);
        }

        /// <summary>
        /// Deserializes boxed object from reader. First attribute of element read must be type attribute. (e.g.: _type="System.Int32")
        /// </summary>
        /// <param name="reader">Reader to read from</param>
        /// <param name="options">Not used by this class</param>
        /// <returns>Deserialized object</returns>
        /// <exception cref="XmlSerializationException">If XmlElement does not have type attribute (e.g.: _type="System.Int32"), this exception will be thrown.</exception>
        public object Deserialize(XmlReader reader, SerializationOptions options)
        {
            if (reader.MoveToFirstAttribute() && reader.LocalName == Serializer.TypeAttributeName)
            {
                var type = CachedTypeResolver.GetType(reader.Value);
                if (type != typeof(object))
                    return SerializerFactory.GetSerializer(type).ReadXml(reader, options);
            }
            else
            {
                throw new XmlSerializationException(string.Format(Resources.XmlTypeAttributeMissingExceptionMessage, typeof(object), Serializer.TypeAttributeName));
            }

            var objectInstance = new object();

            Deserialize(reader, objectInstance, options);

            return objectInstance;
        }

        /// <summary>
        /// Since object itself does not have any properties / fields, this method skips current element.
        /// </summary>
        /// <param name="reader">Reader to skip current element</param>
        /// <param name="objectInstance">Not used by this method</param>
        /// <param name="options">Not used by this method</param>
        public void Deserialize(XmlReader reader, object objectInstance, SerializationOptions options)
        {
            reader.Skip();
        }

        /// <summary>
        /// Gets default serialization options
        /// </summary>
        public SerializationOptions DefaultSerializationOptions
        {
            get
            {
                return SerializationOptions.Default;
            }
        }

        /// <summary>
        /// Serializes a boxed an object into writer
        /// </summary>
        /// <param name="writer">XmlWriter to be written into</param>
        /// <param name="objectInstance">Instance of object</param>
        /// <param name="options">Not used by this method</param>
        public void WriteXml(XmlWriter writer, object objectInstance, SerializationOptions options)
        {
            Serialize(writer, objectInstance, options);
        }

        /// <summary>
        /// Deserializes an object from reader
        /// </summary>
        /// <param name="reader">Reader to be read</param>
        /// <param name="options">Not used by this method</param>
        /// <returns>Deserialized boxed object</returns>
        public object ReadXml(XmlReader reader, SerializationOptions options)
        {
            return Deserialize(reader, options);
        }

        /// <summary>
        /// Since object itself does not have any properties / fields, this method skips current element.
        /// </summary>
        /// <param name="reader">Reader to skip current element</param>
        /// <param name="objectInstance">Not used by this method</param>
        /// <param name="options">Not used by this method</param>
        public void ReadXml(XmlReader reader, object objectInstance, SerializationOptions options)
        {
            reader.Skip();
        }

    }
}