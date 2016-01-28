namespace Atlas.Xml
{
    /// <summary>
    /// Enumerates serialization methods to be used during xml serialization
    /// </summary>
    public enum ByteArraySerializationType
    {
        /// <summary>
        /// Member should be serialized with Base64 encoding
        /// </summary>
        Base64,

        /// <summary>
        /// Member should be serialized with Hex encoding
        /// </summary>
        BinHex,

        /// <summary>
        /// Each byte should be serialized into new element
        /// </summary>
        Elements,

    }
}