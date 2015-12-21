using Atlas.Xml.Attributes;

namespace Atlas.Xml
{
    /// <summary>
    /// Provides runtime serialization options.
    /// </summary>
    public class SerializationOptions
    {

        public static readonly SerializationOptions Default = new SerializationOptions
        {
            IsDefault = true,
            ChildElementName = XmlSerializationAttributeBase.DefaultChildElementName,
            ValueNodeName = XmlSerializationAttributeBase.DefaultChildValueNodeName,
            KeyNodeName = XmlSerializationAttributeBase.DefaultChildKeyNodeName
        };

        /// <summary>
        /// Gets whether this instance is the default instance (SerializationOptions.Default)
        /// </summary>
        public bool IsDefault { get; private set; } = false;

        /// <summary>
        /// Gets or sets child element name for IEnumerable serialization.
        /// </summary>
        public string ChildElementName { get; set; }

        /// <summary>
        /// Gets or sets child value node name for IEnumerable serialization.
        /// </summary>
        public string ValueNodeName { get; set; }

        /// <summary>
        /// Gets or sets child key node name for IDictionary serialization.
        /// </summary>
        public string KeyNodeName { get; set; }

    }
}