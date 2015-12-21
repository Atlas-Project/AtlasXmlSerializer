using System;

namespace Atlas.Xml.Attributes
{
    /// <summary>
    /// Defines base class for XmlSerialization related attributes
    /// </summary>
    public abstract class XmlSerializationAttributeBase : Attribute
    {

        #region Constants

        /// <summary>
        /// Defines default child element name for IEnumerable serialization
        /// </summary>
        public const string DefaultChildElementName = "item";

        /// <summary>
        /// Defines default child value node name for IDictionary serialization
        /// </summary>
        public const string DefaultChildValueNodeName = "v";

        /// <summary>
        /// Defines default child key node name for IDictionary serialization
        /// </summary>
        public const string DefaultChildKeyNodeName = "k";

        #endregion

        #region Enumerable

        /// <summary>
        /// Specifies child element name for IEnumerable serialization.
        /// </summary>
        public string ChildElementName { get; set; }

        #endregion

        #region Dictionary

        /// <summary>
        /// Gets child key node name or default if key node name is null
        /// </summary>
        public string ChildKeyNodeName { get; set; }

        /// <summary>
        /// Child value's node name. Used on IEnumerable serialization.
        /// </summary>
        /// <remarks>If type is IDictionary, KeyValuePair.Value will be serialized with this name.</remarks>
        public string ChildValueNodeName { get; set; }

        #endregion

        #region Merge & Conversion Methods

        /// <summary>
        /// Gets SerializationOtions instance matching this attribute
        /// </summary>
        /// <param name="other">Other attribute to merge from</param>
        public SerializationOptions ToOptions()
        {
            // If default, return default instance
            if (ChildElementName == null && ChildKeyNodeName == null && ChildValueNodeName == null)
                return SerializationOptions.Default;

            // Convert to options
            return new SerializationOptions
            {
                ChildElementName = ChildElementName ?? DefaultChildElementName,
                KeyNodeName = ChildKeyNodeName ?? DefaultChildKeyNodeName,
                ValueNodeName = ChildValueNodeName ?? DefaultChildValueNodeName,
            };
        }

        protected void MergeBase(XmlSerializationAttributeBase other)
        {
            if (string.IsNullOrEmpty(ChildElementName))
                ChildElementName = other.ChildElementName;

            if (string.IsNullOrEmpty(ChildValueNodeName))
                ChildValueNodeName = other.ChildValueNodeName;

            if (string.IsNullOrEmpty(ChildKeyNodeName))
                ChildKeyNodeName = other.ChildKeyNodeName;
        }

        #endregion

    }
}