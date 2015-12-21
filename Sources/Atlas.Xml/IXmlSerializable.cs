using System.Xml;

namespace Atlas.Xml
{
    /// <summary>
    /// Defines interface for general xml serialization
    /// </summary>
    /// <remarks>Classes implementing this interface, will be serialized by their own serialization implementations</remarks>
    public interface IXmlSerializable
    {

        /// <summary>
        /// Gets default serialization options
        /// </summary>
        SerializationOptions DefaultSerializationOptions { get; }

        /// <summary>
        /// Writes object instance to xml
        /// </summary>
        /// <param name="writer">Xml writer to write object instance</param>
        /// <param name="objectInstance">Object instance to be written</param>
        /// <param name="options">Serialization options to be used. Use SerializationOptions.Default for default options</param>
        /// <remarks>Start tah will be created before calling this method, thus only content of element should be written. Also the end tag will be written by parent.</remarks>
        void WriteXml(XmlWriter writer, object objectInstance, SerializationOptions options);

        /// <summary>
        /// Creates instance of type and reads contents from <c>XmlReader</c>
        /// </summary>
        /// <param name="reader"><c>XmlReader</c> to read from</param>
        /// <param name="options">Serialization options to be used. Use SerializationOptions.Default for default options</param>
        /// <returns>The instance of object instantiated from xml</returns>
        /// <remarks>When this method is called, readers position will be on start tag of element. After finished reading, the end tag should be consumed.</remarks>
        object ReadXml(XmlReader reader, SerializationOptions options);

        /// <summary>
        /// Reads contents from <c>XmlReader</c> into objectInstance
        /// </summary>
        /// <param name="reader"><c>XmlReader</c> to read from</param>
        /// <param name="objectInstance">Object instance to be read into</param>
        /// <param name="options">Serialization options to be used. Use SerializationOptions.Default for default options</param>
        /// <remarks>When this method is called, readers position will be on first attribute of element, or on start tag if there are no attributes. 
        /// After finished reading, the end tag should be consumed.</remarks>
        void ReadXml(XmlReader reader, object objectInstance, SerializationOptions options);

    }
}