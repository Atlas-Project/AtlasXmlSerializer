using System.IO;
using System.Text;
using System.Xml;

namespace Atlas.Xml
{
    /// <summary>
    /// Provides methods for xml serialization
    /// </summary>
    /// <remarks>
    /// Xml serialization will be done by dynamic code generation. First use of any type on xml serialization will be slower than expected.
    /// </remarks>
    public static class Serializer
    {

        #region Constants

        /// <summary>
        /// Defines type attribute name for serialization. Serialization uses this name on first attribute of elements to make sure deserialized type is instantiated from right type.
        /// </summary>
        public const string TypeAttributeName = "_type";

        #endregion

        #region Serialize Methods

        private static XmlWriterSettings DefaultWriterSettings = new XmlWriterSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment,
        };

        /// <summary>
        /// Serializes an instance to xml
        /// </summary>
        /// <typeparam name="T">Type of instance to be serialized</typeparam>
        /// <param name="objectInstance">Instance of type to be serialized</param>
        /// <param name="indent">If true, generated xml will be indented</param>
        /// <param name="includeXmlDeclaration">If ture, generated xml will have xml declaration</param>
        /// <param name="rootElementName">Root element name. If null, type's name will be used</param>
        /// <param name="writeRootElementType">Writes type of root element so that xml will be deserialized without knowing its type. Has same effect with serializing object boxed instance, but faster.</param>
        /// <returns>Xml string of serialized instance</returns>
        public static string Serialize<T>(T objectInstance, bool indent, bool includeXmlDeclaration = false, string rootElementName = null, bool writeRootElementType = false, SerializationOptions options = null)
        {
            var settings = new XmlWriterSettings
            {
                ConformanceLevel = includeXmlDeclaration ? ConformanceLevel.Document : ConformanceLevel.Fragment,
                Indent = indent,
            };

            return Serialize(objectInstance, rootElementName, writeRootElementType, settings, options);
        }

        /// <summary>
        /// Serializes an instance to xml
        /// </summary>
        /// <typeparam name="T">Type of instance to be serialized</typeparam>
        /// <param name="objectInstance">Instance of type to be serialized</param>
        /// <param name="rootElementName">Root element name. If null, type's name will be used</param>
        /// <param name="writeRootElementType">Writes type of root element so that xml will be deserialized without knowing its type. Has same effect with serializing object boxed instance, but faster.</param>
        /// <param name="settings"><c>XmlWriterSettings</c> to be used on serialization. If null, default settings will be used, where ComformanceLevel is Fragment.</param>
        /// <returns>Xml string of serialized instance</returns>
        public static string Serialize<T>(T objectInstance, string rootElementName = null, bool writeRootElementType = false, XmlWriterSettings settings = null, SerializationOptions options = null)
        {
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder, settings ?? DefaultWriterSettings))
            {
                writer.WriteStartElement(rootElementName ?? typeof(T).GetNonGenericName());

                if (writeRootElementType)
                    writer.WriteAttributeString(TypeAttributeName, typeof(T).GetNameForGetType());

                SerializerFactory<T>.Instance.Serialize(writer, objectInstance, options ?? SerializerFactory<T>.Instance.DefaultSerializationOptions);
                writer.WriteEndElement();
            }

            return builder.ToString();
        }

        #endregion

        #region Deserialize Methods

        static XmlReaderSettings DefaultReaderSettings = new XmlReaderSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment,
        };

        /// <summary>
        /// Deserializes a type from xml
        /// </summary>
        /// <typeparam name="T">Type to be deserialized</typeparam>
        /// <param name="xml">Xml data of type</param>
        /// <returns>New instance of deserialized type</returns>
        public static T Deserialize<T>(string xml)
        {
            ArgumentValidation.NotNull(xml, nameof(xml));

            using (var reader = XmlReader.Create(new StringReader(xml), DefaultReaderSettings))
            {
                while (reader.NodeType != XmlNodeType.Element && reader.Read()) ;
                return SerializerFactory<T>.Instance.Deserialize(reader, SerializerFactory<T>.Instance.DefaultSerializationOptions);
            }
        }

        /// <summary>
        /// Deserializes a type from xml
        /// </summary>
        /// <typeparam name="T">Type to be deserialized</typeparam>
        /// <param name="xml">Xml data of type</param>
        /// <param name="options">Runtime options for serialization</param>
        /// <returns>New instance of deserialized type</returns>
        public static T Deserialize<T>(string xml, SerializationOptions options)
        {
            ArgumentValidation.NotNull(xml, nameof(xml));
            ArgumentValidation.NotNull(options, nameof(options));

            using (var reader = XmlReader.Create(new StringReader(xml), DefaultReaderSettings))
            {
                while (reader.NodeType != XmlNodeType.Element && reader.Read()) ;
                return SerializerFactory<T>.Instance.Deserialize(reader, SerializerFactory<T>.Instance.DefaultSerializationOptions);
            }
        }

        #endregion

    }
}