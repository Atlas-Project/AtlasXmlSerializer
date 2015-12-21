using System;
using System.Collections.Generic;

namespace Atlas.Xml
{
    /// <summary>
    /// Provides static methods for serialization attribute overrides / defaults. Those overriden attributes only applies to future serializater compilations, thus they won't change serializaer already compiled.
    /// </summary>
    public static class SerializationAttributeOverrides
    {

        #region Lock Sync Object

        static object _locker = new object();

        #endregion

        #region Member Overrides

        static Dictionary<string, Dictionary<string, XmlSerializationMemberAttribute>> _defaults = new Dictionary<string, Dictionary<string, XmlSerializationMemberAttribute>>();
        static Dictionary<string, Dictionary<string, XmlSerializationMemberAttribute>> _overrides = new Dictionary<string, Dictionary<string, XmlSerializationMemberAttribute>>()
        {
            { "System.Collections.Generic.List", new Dictionary<string, XmlSerializationMemberAttribute> {
                { "Capacity", XmlSerializationMemberAttribute.Ignore }
            }},

            { "System.Collections.ArrayList", new Dictionary<string, XmlSerializationMemberAttribute> {
                { "Capacity", XmlSerializationMemberAttribute.Ignore }
            }},

            { "System.Collections.Generic.Dictionary", new Dictionary<string, XmlSerializationMemberAttribute> {
                { "Keys", XmlSerializationMemberAttribute.Ignore },
                { "Values", XmlSerializationMemberAttribute.Ignore },
            }},
        };

        /// <summary>
        /// Adds serialization attribute to a member of type. This attribute will be merged with existing one's and override already specified properties.
        /// </summary>
        /// <param name="type">Type to be overriden</param>
        /// <param name="memberName">Member to be overriden</param>
        /// <param name="attribute">Attribute to be added. Use null to remove attribute. </param>
        public static void Override(Type type, string memberName, XmlSerializationMemberAttribute attribute)
        {
            ArgumentValidation.NotNull(type, nameof(type));
            ArgumentValidation.NotEmpty(memberName, nameof(memberName));

            AddAttributeToMemberDictionary(_overrides, type, memberName, attribute);
        }

        private static void AddAttributeToMemberDictionary(Dictionary<string, Dictionary<string, XmlSerializationMemberAttribute>> dictionary, Type type, string memberName, XmlSerializationMemberAttribute attribute)
        {
            string typeName = type.GetNonGenericNameWithNamespace();

            lock (_locker)
            {
                Dictionary<string, XmlSerializationMemberAttribute> attributes;
                if (!dictionary.TryGetValue(typeName, out attributes))
                {
                    attributes = new Dictionary<string, XmlSerializationMemberAttribute>();
                    dictionary.Add(typeName, attributes);
                }

                if (attribute != null)
                    attributes[memberName] = attribute;
                else if (attributes.ContainsKey(memberName))
                    attributes.Remove(memberName);
            }
        }

        /// <summary>
        /// Adds serialization attribute to a member of type. This attribute will be merged with existing one's but won't override already specified properties.
        /// </summary>
        /// <param name="type">Type to be overriden</param>
        /// <param name="memberName">Member to be overriden</param>
        /// <param name="attribute">Attribute to be added. Use null to remove attribute.</param>
        public static void SetDefault(Type type, string memberName, XmlSerializationMemberAttribute attribute)
        {
            ArgumentValidation.NotNull(type, nameof(type));
            ArgumentValidation.NotEmpty(memberName, nameof(memberName));

            AddAttributeToMemberDictionary(_defaults, type, memberName, attribute);
        }

        internal static XmlSerializationMemberAttribute GetOverride(Type type, string memberName)
        {
            ArgumentValidation.NotNull(type, nameof(type));
            ArgumentValidation.NotEmpty(memberName, nameof(memberName));

            string typeName = type.GetNonGenericNameWithNamespace();

            Dictionary<string, XmlSerializationMemberAttribute> typeOverrides;
            XmlSerializationMemberAttribute @override;
            if (_overrides.TryGetValue(typeName, out typeOverrides) && typeOverrides.TryGetValue(memberName, out @override))
                return @override;

            return null;
        }

        internal static XmlSerializationMemberAttribute GetDefault(Type type, string memberName)
        {
            ArgumentValidation.NotNull(type, nameof(type));
            ArgumentValidation.NotEmpty(memberName, nameof(memberName));

            string typeName = type.GetNonGenericNameWithNamespace();

            Dictionary<string, XmlSerializationMemberAttribute> attributes;
            XmlSerializationMemberAttribute attribute;
            if (_defaults.TryGetValue(typeName, out attributes) && attributes.TryGetValue(memberName, out attribute))
                return attribute;

            return null;
        }

        #endregion

        #region Type Overrides

        static Dictionary<string, XmlSerializationTypeAttribute> _defaultsForTypes = new Dictionary<string, XmlSerializationTypeAttribute>();
        static Dictionary<string, XmlSerializationTypeAttribute> _overridesForTypes = new Dictionary<string, XmlSerializationTypeAttribute>();

        /// <summary>
        /// Adds serialization attribute to a type. This attribute will be merged with existing one's and override already specified properties.
        /// </summary>
        /// <param name="type">Type to be overriden</param>
        /// <param name="attribute">Attribute to be added. Use null to remove attribute.</param>
        public static void Override(Type type, XmlSerializationTypeAttribute attribute)
        {
            ArgumentValidation.NotNull(type, nameof(type));

            string typeName = type.GetNonGenericNameWithNamespace();

            lock (_locker)
            {
                if (attribute != null)
                    _overridesForTypes[typeName] = attribute;
                else if (_overridesForTypes.ContainsKey(typeName))
                    _overridesForTypes.Remove(typeName);
            }
        }

        /// <summary>
        /// Adds serialization attribute to a type. This attribute will be merged with existing one's but won't override already specified properties.
        /// </summary>
        /// <param name="type">Type to be overriden</param>
        /// <param name="attribute">Attribute to be added. Use null to remove attribute.</param>
        public static void SetDefault(Type type, XmlSerializationTypeAttribute attribute)
        {
            ArgumentValidation.NotNull(type, nameof(type));

            string typeName = type.GetNonGenericNameWithNamespace();

            lock (_locker)
            {
                if (attribute != null)
                    _overridesForTypes[typeName] = attribute;
                else if (_overridesForTypes.ContainsKey(typeName))
                    _overridesForTypes.Remove(typeName);
            }
        }

        internal static XmlSerializationTypeAttribute GetOverride(Type type)
        {
            ArgumentValidation.NotNull(type, nameof(type));

            string typeName = type.GetNonGenericNameWithNamespace();

            XmlSerializationTypeAttribute @override;
            _overridesForTypes.TryGetValue(typeName, out @override);
            return @override;
        }

        internal static XmlSerializationTypeAttribute GetDefault(Type type)
        {
            ArgumentValidation.NotNull(type, nameof(type));

            string typeName = type.GetNonGenericNameWithNamespace();

            XmlSerializationTypeAttribute attribute;
            _defaultsForTypes.TryGetValue(typeName, out attribute);
            return attribute;
        }

        #endregion

    }
}