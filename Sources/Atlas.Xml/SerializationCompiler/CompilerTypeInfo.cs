using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace Atlas.Xml.SerializationCompiler
{
    internal class CompilerTypeInfo
    {

        #region Constructors & Constants

        protected const BindingFlags InstanceMethodBindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        protected const BindingFlags StaticMethodBindings = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        protected CompilerTypeInfo()
        {
        }

        public CompilerTypeInfo(Type valueType)
        {
            InitializeValueType(valueType);
            FinalizeValueType();
        }

        #endregion

        #region Initialization

        protected void InitializeValueType(Type valueType)
        {
            // Describe property
            RealValueType = valueType;
            ValueType = valueType;

            // Check for IEnumerable
            if (ValueType.GetInterface("IEnumerable") != null)
            {
                IsEnumerable = true;

                IsCollection = ValueType.IsGenericType && ValueType.GetGenericTypeDefinition() == typeof(ICollection<>);
                IsCollection |= ValueType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>));
                IsCollection |= ValueType == typeof(System.Collections.IList);
                IsCollection |= ValueType.GetInterfaces().Any(t => t == typeof(System.Collections.IList));
            }

            FinalizeValueType();
        }

        protected void FinalizeValueType()
        {
            StripNullable();

            // Check if Xml Convertible
            IsXmlConvertible = null != typeof(XmlConvert).GetMethod("ToString", new[] { ValueType });

            if (!SerializerConfigured || !DeserializerConfigured)
            {
                // Use type convertor if type is convertible from string
                var converter = TypeDescriptor.GetConverter(ValueType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    if (!SerializerConfigured && converter.CanConvertTo(typeof(string)))
                        ShouldUseTypeConverterOnSerialization = true;

                    if (!DeserializerConfigured && converter.CanConvertFrom(typeof(string)))
                        ShouldUseTypeConverterOnDeserialization = true;
                }
            }

            // Finally check for ToString / Parse methods
            if (!SerializerConfigured)
            {
                // Check if there's a ToString with culture parameter like; string ToString(IFormatProvider)
                //                                                       or string ToString(CultureInfo)
                if (null != ValueType.GetMethod("ToString", InstanceMethodBindings, null, new[] { typeof(IFormatProvider) }, null)
                    || null != ValueType.GetMethod("ToString", InstanceMethodBindings, null, new[] { typeof(CultureInfo) }, null))
                {
                    ShouldUseToStringWithCultureOnSerialization = true;
                }
            }

            if (!DeserializerConfigured)
            {
                if (ShouldUseToStringWithCultureOnSerialization)
                {
                    // has method like; static "Type" Parse(string, IFormatProvider)
                    //               or static "Type" Parse(string, CultureInfo)
                    if (null != ValueType.GetMethod("Parse", StaticMethodBindings, null, new[] { typeof(string), typeof(IFormatProvider) }, null)
                    || null != ValueType.GetMethod("Parse", StaticMethodBindings, null, new[] { typeof(string), typeof(CultureInfo) }, null))
                    {
                        ShouldUseParseOnDeserialization = true;
                        ShouldUseParseOnDeserializationWithCulture = true;
                    }
                }

                if (!DeserializerConfigured)
                {
                    ShouldUseToStringWithCultureOnSerialization = false;

                    // else: has method like; static "Type" Parse(string)
                    if (null != ValueType.GetMethod("Parse", StaticMethodBindings, null, new[] { typeof(string) }, null))
                        ShouldUseParseOnDeserialization = true;
                }
            }
        }

        #endregion

        #region Properties

        public Type ValueType { get; private set; }
        public Type RealValueType { get; private set; }

        public bool CanBeNull { get; protected set; }
        public bool IsNullableType { get; private set; }

        public bool ShouldUseCustomSerializationMethod { get; protected set; }
        public bool ShouldUseCustomDeserializationMethod { get; protected set; }

        public bool IsXmlConvertible { get; private set; }

        public bool ShouldUseTypeConverterOnSerialization { get; private set; }
        public bool ShouldUseTypeConverterOnDeserialization { get; private set; }

        public bool ShouldUseToStringWithCultureOnSerialization { get; private set; }
        public bool ShouldUseParseOnDeserialization { get; private set; }
        public bool ShouldUseParseOnDeserializationWithCulture { get; private set; }

        public bool IsEnumerable { get; private set; }
        public bool IsCollection { get; private set; }
        public bool IsDictionary { get; private set; }

        public bool ShouldUseXmlConvertOnGet
        {
            get
            {
                if (ShouldUseCustomSerializationMethod || !IsXmlConvertible)
                    return false;
                return true;
            }
        }

        public bool ShouldUseXmlConvertOnSet
        {
            get
            {
                if (ShouldUseCustomDeserializationMethod || !IsXmlConvertible)
                    return false;
                return true;
            }
        }

        public virtual bool SerializerConfigured
        {
            get { return ValueType == typeof(string) || IsXmlConvertible || ShouldUseTypeConverterOnSerialization || ShouldUseToStringWithCultureOnSerialization; }
        }

        public virtual bool DeserializerConfigured
        {
            get { return ValueType == typeof(string) || IsXmlConvertible || ShouldUseTypeConverterOnDeserialization || ShouldUseParseOnDeserialization; }
        }

        private string _typeName;

        public string TypeName
        {
            get
            {
                if (_typeName == null)
                    _typeName = CachedTypeResolver.GetFriendlyNameForCode(RealValueType);

                return _typeName;
            }
        }

        private XmlSerializationTypeAttribute _serializationAttribute;

        public XmlSerializationTypeAttribute SerializationAttribute
        {
            get
            {
                if (_serializationAttribute == null)
                    _serializationAttribute = XmlSerializationTypeAttribute.FromType(RealValueType);

                return _serializationAttribute;
            }
        }


        private SerializationOptions _serializationOptions;

        public SerializationOptions SerializationOptions
        {
            get
            {
                if (_serializationOptions == null)
                    _serializationOptions = SerializationAttribute.ToOptions();

                return _serializationOptions;
            }
        }

        #endregion

        #region Methods

        public virtual void StripNullable()
        {
            // Strip nullable. Nullable<Type> => Type
            CanBeNull = !ValueType.IsValueType;
            if (!CanBeNull)
            {
                var nullableBaseType = Nullable.GetUnderlyingType(ValueType);
                if (nullableBaseType != null)
                {
                    IsNullableType = true;
                    CanBeNull = true;
                    ValueType = nullableBaseType;
                }
            }
        }

        #endregion

    }
}