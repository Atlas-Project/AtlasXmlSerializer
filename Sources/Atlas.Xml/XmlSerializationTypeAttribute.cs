using System;
using Atlas.Xml.Attributes;

namespace Atlas.Xml
{
    /// <summary>
    /// Defines attribute for serialization of types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class XmlSerializationTypeAttribute : XmlSerializationAttributeBase
    {

        #region Properties

        /// <summary>
        /// Specifies default node type of member.
        /// </summary>
        public SerializationNodeType NodeType { get; set; } = SerializationNodeType.Auto;

        #endregion

        #region Dictionary

        /// <summary>
        /// Used on IDictionary serialization, determines serialization type of KeyValuePair.Key
        /// </summary>
        public SerializationNodeType ChildKeyNodeType { get; set; } = SerializationNodeType.Auto;

        /// <summary>
        /// Child value's serialization type. Used on IEnumerable serialization.
        /// </summary>
        /// <remarks>If type is IDictionary, KeyValuePair.Value will be serialized with this option.</remarks>
        public SerializationNodeType ChildValueNodeType { get; set; } = SerializationNodeType.Auto;

        #endregion

        #region Convert / Merge

        /// <summary>
        /// Merges all attributes by member and member's type with inheritance order. Top attributes will override lower ones.
        /// </summary>
        /// <param name="other">Member info to check attributes</param>
        public static XmlSerializationTypeAttribute FromType(Type type)
        {
            var result = new XmlSerializationTypeAttribute();

            // Merge with type serialization attributes
            var memberHierarchy = type.GetTypeHierarchy();

            var options = memberHierarchy.GetAttributes<XmlSerializationTypeAttribute>();
            foreach (var option in options)
                result.Merge(option);

            // Check defaults
            var @default = SerializationAttributeOverrides.GetDefault(type);
            if (@default != null)
                result.Merge(@default);

            // Check overrides
            var @override = SerializationAttributeOverrides.GetOverride(type);
            if (@override != null)
            {
                var newOptions = @override.Clone();
                newOptions.Merge(result);
                result = newOptions;
            }

            return result;
        }

        /// <summary>
        /// Merges attribute with other
        /// </summary>
        /// <param name="other">Other attribute to merge from</param>
        public void Merge(XmlSerializationTypeAttribute other)
        {
            MergeBase(other);

            if (NodeType == SerializationNodeType.Auto)
                NodeType = other.NodeType;

            if (ChildValueNodeType == SerializationNodeType.Auto)
                ChildValueNodeType = other.ChildValueNodeType;

            if (ChildKeyNodeType == SerializationNodeType.Auto)
                ChildKeyNodeType = other.ChildKeyNodeType;
        }

        /// <summary>
        /// Returns new instance and copies values to it.
        /// </summary>
        /// <returns>Newly created attribute with same values</returns>
        public XmlSerializationTypeAttribute Clone()
        {
            var result = new XmlSerializationTypeAttribute();
            result.Merge(this);
            return result;
        }

        #endregion

    }
}