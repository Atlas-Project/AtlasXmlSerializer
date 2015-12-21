using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Xml;

namespace Atlas.Xml.SerializationCompiler
{
    internal class EnumerableCompiler
    {

        #region Constructor & Constants & Initial Fields

        public const string ChildInstanceName = "item";

        readonly Type _type;

        public EnumerableCompiler(Type type, StringBuilder classLevelCodePart)
        {
            _type = type;

            Initialize(classLevelCodePart);
        }

        #endregion

        #region Fields

        private string _nullSetterFunction;

        private SerializationNodeType _valueSerializationMethod;
        private CompilerTypeInfo _valueTypeInfo;

        private string _valueGetterFunction;
        private string _valueGetterFunctionEnd;
        private string _valueSerializerFunctionStart;
        private string _valueSerializerFunctionEnd;

        private string _valueSetterFunctionStart;
        private string _valueSetterFunctionEnd;

        #endregion

        #region Properties

        public Type ChildType
        {
            get { return _valueTypeInfo.RealValueType; }
        }

        #endregion

        #region Initialization

        private void Initialize(StringBuilder classLevelCodePart)
        {
            ConfigureMemberType();
            ConfigureGetter(classLevelCodePart);
            ConfigureSetter(classLevelCodePart);
            ConfigureTypeConverter(classLevelCodePart);
            ConfigureSerializer(classLevelCodePart);
        }

        private void ConfigureMemberType()
        {
            #region Get Value Type

            var valueType = typeof(object);

            if (_type.IsArray)
            {
                valueType = _type.GetElementType();
            }
            else if (_type.GetInterface("IEnumerable") != null)
            {
                Type enumerableType = null;
                if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    enumerableType = _type;
                }
                else
                {
                    enumerableType = _type.GetInterfaces()
                        .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .FirstOrDefault();
                }

                if (enumerableType != null)
                    valueType = enumerableType.GetGenericArguments()[0];
            }
            else
            {
                throw new InvalidOperationException("Type is not IEnumerable!");
            }

            _valueTypeInfo = new CompilerTypeInfo(valueType);

            #endregion

            #region Configure by attributes

            _valueSerializationMethod = XmlSerializationTypeAttribute.FromType(_type).ChildValueNodeType;
            if (_valueSerializationMethod == SerializationNodeType.Auto)
            {
                _valueSerializationMethod = _valueTypeInfo.SerializationAttribute.NodeType;

                if (_valueSerializationMethod == SerializationNodeType.Auto)
                    _valueSerializationMethod = SerializationNodeType.Text;
            }

            #endregion

        }

        private void ConfigureGetter(StringBuilder classLevelCodePart)
        {
            _valueGetterFunction = ChildInstanceName;

            // If nullable, use .Value suffix for getter
            if (_valueTypeInfo.IsNullableType)
                _valueGetterFunctionEnd = ".Value";
            else
                _valueSetterFunctionEnd = "";
        }

        private void ConfigureSetter(StringBuilder classLevelCodePart)
        {
            _valueSetterFunctionStart = Compiler.ObjectInstanceName + ".Add(";
            _valueSetterFunctionEnd = ");";

            _nullSetterFunction = _valueSetterFunctionStart + "null" + _valueSetterFunctionEnd;
        }

        private void ConfigureTypeConverter(StringBuilder classLevelCodePart)
        {
            // Converter declaration
            if (_valueTypeInfo.ShouldUseTypeConverterOnSerialization || _valueTypeInfo.ShouldUseTypeConverterOnDeserialization)
            {
                // var tc"MemberName" = System.ComponentModel.TypeDescriptor.GetConverter(typeof("ValueType"));
                classLevelCodePart.Append("var tc");
                classLevelCodePart.Append(" = System.ComponentModel.TypeDescriptor.GetConverter(typeof(");
                classLevelCodePart.Append(_valueTypeInfo.TypeName);
                classLevelCodePart.AppendLine("));");
            }
        }

        private void ConfigureSerializer(StringBuilder classLevelCodePart)
        {
            var serializerInstance = "SerializerFactory<" + _valueTypeInfo.TypeName + ">.Instance";
            string serializerOptionsInstance = PropertyCompiler.ConfigureSerializerOptions(classLevelCodePart, serializerInstance + ".DefaultSerializationOptions", _valueTypeInfo.SerializationOptions, null, "childSerializerOptions");

            if (!_valueTypeInfo.DeserializerConfigured)
            {
                _valueSetterFunctionStart += serializerInstance + ".Deserialize(reader, ";
                _valueSetterFunctionStart += serializerOptionsInstance + ")";
            }

            if (!_valueTypeInfo.SerializerConfigured)
            {
                _valueSerializerFunctionStart = serializerInstance + ".Serialize(writer, ";
                _valueSerializerFunctionEnd = ", " + serializerOptionsInstance + ");";
            }
        }

        #endregion

        #region Writer Methods

        public void AppendWriteChildToXml(StringBuilder code)
        {
            if (_valueTypeInfo.CanBeNull)
            {
                // if("Value" != null) {
                code.Append("if(");
                code.Append(_valueGetterFunction);
                code.AppendLine(" != null) {");
            }

            bool isText = _valueSerializationMethod == SerializationNodeType.Text || _valueSerializationMethod == SerializationNodeType.TextAsCData;

            if (_valueSerializationMethod == SerializationNodeType.Attribute)
            {
                code.Append("writer.WriteAttributeString(options.ValueNodeName, ");
                Compiler.AppendToString(code, _valueTypeInfo, _valueGetterFunction + _valueGetterFunctionEnd, null);
                code.AppendLine(");");
            }
            else
            {
                if (!isText)
                    code.AppendLine("writer.WriteStartElement(options.ValueNodeName);");

                if (_valueSerializationMethod == SerializationNodeType.ElementAsCData || _valueSerializationMethod == SerializationNodeType.TextAsCData)
                    Compiler.AppendWriteCData(code, _valueTypeInfo, _valueGetterFunction + _valueGetterFunctionEnd, null);
                else
                    AppendWriteValueToXml(code, _valueGetterFunction + _valueGetterFunctionEnd);

                if (!isText)
                    code.AppendLine("writer.WriteEndElement();");
            }

            // Write null as type so it can be converted to null on deserialization.
            if (_valueTypeInfo.CanBeNull)
            {
                code.AppendLine("}");
                if (isText)
                    code.AppendLine("else { writer.WriteAttributeString(Serializer.TypeAttributeName, \"null\"); }");
            }
        }

        private void AppendWriteValueToXml(StringBuilder code, string getterFunction)
        {
            if (!_valueTypeInfo.SerializerConfigured)
            {
                // "serializer".Serialize(writer, "ValueAsString", options);
                code.Append(_valueSerializerFunctionStart);
                code.Append(getterFunction);
                code.AppendLine(_valueSerializerFunctionEnd);
            }
            else
            {
                // writer.WriteString("ValueAsString");
                code.Append("writer.WriteString(");
                Compiler.AppendToString(code, _valueTypeInfo, getterFunction, null);
                code.AppendLine(");");
            }
        }

        #endregion

        #region Reader Methods

        public void AppendReadChildFromXml(StringBuilder code)
        {
            bool isText = _valueSerializationMethod == SerializationNodeType.Text || _valueSerializationMethod == SerializationNodeType.TextAsCData;

            if (isText)
                AppendReadChildFromXmlText(code);
            else if (_valueSerializationMethod == SerializationNodeType.Attribute)
                AppendReadChildFromXmlAttribute(code);
            else
                AppendReadChildFromXmlElement(code);
        }

        private void AppendReadChildFromXmlText(StringBuilder code)
        {
            if (_valueTypeInfo.CanBeNull)
            {
                code.AppendLine("if(reader.MoveToFirstAttribute()) {");
                code.AppendLine("  if(reader.LocalName == Serializer.TypeAttributeName && reader.Value == \"null\") {");
                code.AppendLine(_nullSetterFunction);
                code.AppendLine("    reader.Skip();");
                code.AppendLine("    continue;");
                code.AppendLine("  }");
                code.AppendLine("  reader.MoveToElement();");
                code.AppendLine("}");
            }

            if (!_valueTypeInfo.DeserializerConfigured)
            {
                code.Append(_valueSetterFunctionStart);
                code.AppendLine(_valueSetterFunctionEnd);
            }
            else
            {
                code.AppendLine("if(!reader.IsEmptyElement) {");
                code.AppendLine("reader.Read();"); // Move to value

                code.Append(_valueSetterFunctionStart);
                Compiler.AppendParseString(code, _valueTypeInfo, null, _valueTypeInfo.TypeName);
                code.AppendLine(_valueSetterFunctionEnd);

                code.AppendLine("reader.MoveUpToNextElement();"); // Consume element
                code.AppendLine("} else {");

                if (_valueTypeInfo.CanBeNull)
                {
                    code.AppendLine(_nullSetterFunction);
                }
                else
                {
                    code.Append(_valueSetterFunctionStart);
                    Compiler.AppendParseString(code, _valueTypeInfo, null, _valueTypeInfo.TypeName, "\"\"");
                    code.AppendLine(_valueSetterFunctionEnd);
                }

                code.AppendLine("reader.Skip();"); // Skip tp next element
                code.AppendLine("}");
            }
        }

        private void AppendReadChildFromXmlElement(StringBuilder code)
        {
            code.AppendLine("if(reader.MoveToFirstChild()) {");

            if (!_valueTypeInfo.DeserializerConfigured)
            {
                code.Append(_valueSetterFunctionStart);
                code.AppendLine(_valueSetterFunctionEnd);
            }
            else
            {
                code.AppendLine("  if(!reader.IsEmptyElement) {");
                code.AppendLine("    reader.Read();"); // Move to value

                code.Append(_valueSetterFunctionStart);
                Compiler.AppendParseString(code, _valueTypeInfo, null, _valueTypeInfo.TypeName);
                code.AppendLine(_valueSetterFunctionEnd);

                code.AppendLine("  reader.MoveUpToNextElement();"); // Consume value
                code.AppendLine("  } else {");

                code.Append(_valueSetterFunctionStart);
                Compiler.AppendParseString(code, _valueTypeInfo, null, _valueTypeInfo.TypeName, "\"\"");
                code.AppendLine(_valueSetterFunctionEnd);

                code.AppendLine("  }");
            }

            code.AppendLine("  reader.MoveUpToNextElement();"); // Consume child
            code.AppendLine("} else {");

            if (_valueTypeInfo.CanBeNull)
            {
                code.AppendLine(_nullSetterFunction);
            }
            else
            {
                code.Append("throw new XmlSerializationException(\"");
                code.Append(string.Format(Resources.XmlMissingValueElementExceptionMessage, _valueTypeInfo.TypeName));
                code.AppendLine("\");");
            }

            code.AppendLine("}");
        }

        private void AppendReadChildFromXmlAttribute(StringBuilder code)
        {
            code.AppendLine("if(reader.MoveToFirstAttribute())");

            code.Append(_valueSetterFunctionStart);
            Compiler.AppendParseString(code, _valueTypeInfo, null, _valueTypeInfo.TypeName);
            code.AppendLine(_valueSetterFunctionEnd);

            code.AppendLine("else");

            if (_valueTypeInfo.CanBeNull)
            {
                code.AppendLine(_nullSetterFunction);
            }
            else
            {
                code.Append("throw new XmlSerializationException(\"");
                code.Append(string.Format(Resources.XmlMissingValueAttributeExceptionMessage, _valueTypeInfo.TypeName));
                code.AppendLine("\");");
            }

            code.AppendLine("reader.Skip();");
        }

        #endregion

    }
}