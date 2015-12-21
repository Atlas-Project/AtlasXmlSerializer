using System.Xml;

namespace Atlas.Xml
{
    /// <summary>
    /// Defines strongly typed serialization methods for xml serialization
    /// </summary>
    /// <typeparam name="T">Type of instance to be serialized</typeparam>
    public interface IXmlSerializer<T> : IXmlSerializable
    {
        /// <summary>
        /// Writes object instance to xml
        /// </summary>
        /// <param name="writer">Xml writer to write object instance</param>
        /// <param name="objectInstance">Object instance to be written</param>
        /// <param name="options">Serialization options to be used. Use SerializationOptions.Default for default options</param>
        /// <remarks>Start tah will be created before calling this method, thus only content of element should be written. Also the end tag will be written by parent.</remarks>
        void Serialize(XmlWriter writer, T objectInstance, SerializationOptions options);

        /// <summary>
        /// Creates instance of type and reads contents from <c>XmlReader</c>
        /// </summary>
        /// <param name="reader"><c>XmlReader</c> to read from</param>
        /// <param name="options">Serialization options to be used. Use SerializationOptions.Default for default options</param>
        /// <returns>The instance of object instantiated from xml</returns>
        /// <remarks>When this method is called, readers position will be on first attribute of of element, or on start tag if there are no attributes. 
        /// After finished reading, the end tag should be consumed.</remarks>
        T Deserialize(XmlReader reader, SerializationOptions options);

        /// <summary>
        /// Reads contents from <c>XmlReader</c> into objectInstance
        /// </summary>
        /// <param name="reader"><c>XmlReader</c> to read from</param>
        /// <param name="objectInstance">Object instance to be read into</param>
        /// <param name="options">Serialization options to be used. Use SerializationOptions.Default for default options</param>
        /// <remarks>When this method is called, readers position will be on first attribute of element, or on start tag if there are no attributes. 
        /// After finished reading, the end tag should be consumed.</remarks>
        void Deserialize(XmlReader reader, T objectInstance, SerializationOptions options);

    }
}