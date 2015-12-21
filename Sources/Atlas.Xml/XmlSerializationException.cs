using System;

namespace Atlas.Xml
{
    /// <summary>
    /// Defines general exception for serializer operations.
    /// </summary>
    [Serializable]
    public class XmlSerializationException : Exception
    {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        public XmlSerializationException()
        {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message">Message of exception.</param>
        public XmlSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="innerException">Inner exception occoured during serialization operation.</param>
        /// <param name="message">Message of exception.</param>
        public XmlSerializationException(Exception innerException, string message)
            : base(message, innerException)
        {
        }

    }
}