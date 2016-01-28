using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Atlas.Xml.Attributes;

namespace Atlas.Xml
{
    /// <summary>
    /// Defines attribute for serialization of properties and fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class XmlSerializationMemberAttribute : XmlSerializationAttributeBase
    {

        #region Properties

        /// <summary>
        /// Specifies name of node. If null, member's name will be used on serialization.
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Order of member in xml. If null, members appereance order in class will be used.
        /// </summary>
        public int Order { get; set; } = -1;

        /// <summary>
        /// Specifies node type of member.
        /// </summary>
        public SerializationNodeType NodeType { get; set; } = SerializationNodeType.Auto;

        #endregion

        #region Convert / Merge

        /// <summary>
        /// Merges all attributes by member and member's type with inheritance order. Top attributes will override lower ones.
        /// </summary>
        /// <param name="other">Member info to check attributes</param>
        public static XmlSerializationMemberAttribute FromMember(MemberInfo member)
        {
            var result = new XmlSerializationMemberAttribute();

            result.MergeSystemAttributes(member);

            // TODO: Check inheritance order etc
            //       En üstteki attribute ilk gelmeli.
            var attributes = member.GetCustomAttributes<XmlSerializationMemberAttribute>(true);
            foreach (var attribute in attributes)
                result.Merge(attribute);

            // Check defaults
            var @default = SerializationAttributeOverrides.GetDefault(member.DeclaringType, member.Name);
            if (@default != null)
                result.Merge(@default);

            // Check overrides
            var @override = SerializationAttributeOverrides.GetOverride(member.DeclaringType, member.Name);
            if (@override != null)
            {
                var newOptions = @override.Clone();
                newOptions.Merge(result);
                result = newOptions;
            }

            // Merge with type attributes
            XmlSerializationTypeAttribute typeAttribute = null;
            if (member.MemberType == MemberTypes.Property)
                typeAttribute = XmlSerializationTypeAttribute.FromType((member as PropertyInfo).PropertyType);
            else if (member.MemberType == MemberTypes.Field)
                typeAttribute = XmlSerializationTypeAttribute.FromType((member as FieldInfo).FieldType);

            if (typeAttribute != null)
                result.Merge(typeAttribute);

            return result;
        }

        private void MergeSystemAttributes(MemberInfo member)
        {
            // Ignore / NonSerialized
            if (NodeType == SerializationNodeType.Auto)
            {
                if (member.GetCustomAttributes<XmlIgnoreAttribute>().Any() || member.GetCustomAttributes<NonSerializedAttribute>().Any())
                    NodeType = SerializationNodeType.None;
            }

            // XmlText 
            if (NodeType == SerializationNodeType.Auto && member.GetCustomAttributes<XmlTextAttribute>().Any())
                NodeType = SerializationNodeType.Text;

            // XmlElement
            var element = member.GetCustomAttributes<XmlElementAttribute>().FirstOrDefault();
            if (element != null)
            {
                if (NodeType == SerializationNodeType.Auto)
                    NodeType = SerializationNodeType.Element;

                if (string.IsNullOrEmpty(NodeName))
                    NodeName = element.ElementName;

                if (Order < 0 && element.Order > 0)
                    Order = element.Order;
            }

            // XmlArray
            var array = member.GetCustomAttributes<XmlArrayAttribute>().FirstOrDefault();
            if (array != null)
            {
                if (NodeType == SerializationNodeType.Auto)
                    NodeType = SerializationNodeType.Element;

                if (string.IsNullOrEmpty(NodeName))
                    NodeName = array.ElementName;

                if (Order < 0 && array.Order > 0)
                    Order = array.Order;
            }

            // XmlArrayItem
            var arrayItem = member.GetCustomAttributes<XmlArrayItemAttribute>().FirstOrDefault();
            if (arrayItem != null)
            {
                if (string.IsNullOrEmpty(ChildElementName))
                    ChildElementName = arrayItem.ElementName;

                // TODO: NestingLevel
            }

            // XmlAttribute
            var attribute = member.GetCustomAttributes<XmlAttributeAttribute>().FirstOrDefault();
            if (attribute != null)
            {
                if (NodeType == SerializationNodeType.Auto)
                    NodeType = SerializationNodeType.Attribute;

                if (string.IsNullOrEmpty(NodeName))
                    NodeName = attribute.AttributeName;
            }
        }

        /// <summary>
        /// Merges attribute with other
        /// </summary>
        /// <param name="other">Other attribute to merge from</param>
        public void Merge(XmlSerializationMemberAttribute other)
        {
            MergeBase(other);

            if (string.IsNullOrEmpty(NodeName))
                NodeName = other.NodeName;

            if (Order < 0)
                Order = other.Order;

            if (NodeType == SerializationNodeType.Auto)
                NodeType = other.NodeType;
        }

        /// <summary>
        /// Merges attribute with type attribute
        /// </summary>
        /// <param name="other">Type attribute to merge from</param>
        public void Merge(XmlSerializationTypeAttribute other)
        {
            MergeBase(other);

            if (NodeType == SerializationNodeType.Auto)
                NodeType = other.NodeType;
        }

        /// <summary>
        /// Returns new instance and copies values to it.
        /// </summary>
        /// <returns>Newly created attribute with same values</returns>
        public XmlSerializationMemberAttribute Clone()
        {
            var result = new XmlSerializationMemberAttribute();
            result.Merge(this);
            return result;
        }

        #endregion

        #region Static Instances

        /// <summary>
        /// Gets instance of <see cref="XmlSerializationMemberAttribute"/> with NodeType = SerializationNodeType.None.
        /// </summary>
        public static XmlSerializationMemberAttribute Ignore { get; } = new XmlSerializationMemberAttribute { NodeType = SerializationNodeType.None };

        #endregion

    }
}