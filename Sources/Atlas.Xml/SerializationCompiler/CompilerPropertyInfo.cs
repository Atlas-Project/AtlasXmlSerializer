using System;
using System.Reflection;
using System.Linq;

namespace Atlas.Xml.SerializationCompiler
{
    internal class CompilerPropertyInfo : CompilerTypeInfo
    {

        #region Constants

        public const string CustomSerializationMethodSuffix = "_CustomXmlSerialize";
        public const string CustomDeserializationMethodSuffix = "_CustomXmlDeserialize";

        #endregion

        #region Constructor

        public CompilerPropertyInfo(MemberInfo member)
        {
            // Check custom serialization method exists (e.g.: string Class.Property_CustomXmlSerialize())
            if (null != member.ReflectedType.GetMethod(member.Name + CustomSerializationMethodSuffix, InstanceMethodBindings, null, Type.EmptyTypes, null))
            {
                ShouldUseCustomSerializationMethod = true;
                CanBeNull = true;
                CanGet = true;
            }

            // Check custom deserialization method exists (e.g.: void Class.Property_CustomXmlDeserialize(string))
            if (null != member.ReflectedType.GetMethod(member.Name + CustomDeserializationMethodSuffix, InstanceMethodBindings, null, new[] { typeof(string) }, null))
            {
                ShouldUseCustomDeserializationMethod = true;
                CanSet = true;
            }

            // Describe property
            var property = member as PropertyInfo;
            if (property != null)
            {
                InitializeValueType(property.PropertyType);

                if (property.GetIndexParameters().Any())
                {
                    CanGet = false;
                    CanSet = false;
                }
                else
                {
                    if (property.CanRead)
                    {
                        CanGet = true;
                        IsPublicGet = property.GetMethod.IsPublic;
                        IsStatic = property.GetMethod.IsStatic;
                    }
                    if (property.CanWrite)
                    {
                        CanSet = true;
                        IsPublicSet = property.SetMethod.IsPublic;
                        IsStatic = property.SetMethod.IsStatic;
                    }
                }
            }
            else
            {
                // Describe Field
                var field = (FieldInfo)member;

                InitializeValueType(field.FieldType);
                IsStatic = field.IsStatic;

                CanGet = true;
                IsPublicGet = field.IsPublic;

                // If not readonly or const, mark it as writable
                if (!field.IsInitOnly && !field.IsLiteral)
                {
                    CanSet = true;
                    IsPublicSet = field.IsPublic;
                }
            }
        }

        #endregion

        #region Properties

        public bool IsStatic { get; private set; }
        public bool IsPublicGet { get; private set; }
        public bool IsPublicSet { get; private set; }
        public bool CanGet { get; private set; }
        public bool CanSet { get; private set; }

        private bool SeemsLikeToBeSerialized
        {
            get
            {
                // Auto serialized members:
                // - Public Get + Set properties are auto serialized. 
                // - Collections with public get are auto serialized
                // - Custom serialized or deserialized properties.
                return ((IsPublicGet && IsPublicSet && !IsStatic) || ShouldUseCustomDeserializationMethod || ShouldUseCustomSerializationMethod || (IsCollection && IsPublicGet));
            }
        }

        public SerializationNodeType PrefferedSerializationType
        {
            get
            {
                if (SeemsLikeToBeSerialized)
                {
                    // If value is string, xml convertible, Custom serialized or has type converter, then prefer attribute
                    if (SerializerConfigured || DeserializerConfigured)
                        return SerializationNodeType.Attribute;
                    else // Otherwise (XmlSerializer handled) element
                        return SerializationNodeType.Element;
                }

                return SerializationNodeType.None;
            }
        }

        #endregion

        #region Overrides

        public override bool SerializerConfigured
        {
            get { return ShouldUseCustomSerializationMethod || base.SerializerConfigured; }
        }

        public override bool DeserializerConfigured
        {
            get { return ShouldUseCustomDeserializationMethod || base.DeserializerConfigured; }
        }

        public override void StripNullable()
        {
            if (!ShouldUseCustomSerializationMethod)
                base.StripNullable();
        }

        #endregion

    }
}