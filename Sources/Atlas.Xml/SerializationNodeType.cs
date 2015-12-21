namespace Atlas.Xml
{
    /// <summary>
    /// Enumerates serialization methods to be used during xml serialization
    /// </summary>
    public enum SerializationNodeType
    {
        /// <summary>
        /// Member should not be serialized
        /// </summary>
        None,

        /// <summary>
        /// Members serialization type should be determined by compiler.
        /// </summary>
        Auto,

        /// <summary>
        /// Member should be serialized to xml elements attribute
        /// </summary>
        Attribute,

        /// <summary>
        /// Member should be serialized to xml element
        /// </summary>
        Element,

        /// <summary>
        /// Member should be serialized to xml element with CData value
        /// </summary>
        ElementAsCData,

        /// <summary>
        /// Member should be serialized to xml text
        /// </summary>
        Text,

        /// <summary>
        /// Member should be serialized to xml text as CData
        /// </summary>
        TextAsCData,

    }
}