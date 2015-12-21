using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Atlas.Xml.SerializationCompiler
{
    internal class DictionaryCompiler
    {

        #region Constructor & Initial Fields

        readonly Type _type;

        public DictionaryCompiler(Type type, StringBuilder classLevelCodePart)
        {
            _type = type;

            Initialize(classLevelCodePart);
        }

        #endregion

        #region Value Fields

        private SerializationNodeType _valueSerializationMethod;
        private CompilerTypeInfo _valueTypeInfo;

        private string _valueGetterFunction;
        private string _valueGetterFunctionEnd;
        private string _valueSerializerFunctionStart;
        private string _valueSerializerFunctionEnd;

        private string _valueSetterFunctionStart;
        private string _valueSetterFunctionEnd;

        #endregion

        #region Key Fields

        private SerializationNodeType _keySerializationMethod;
        private CompilerTypeInfo _keyTypeInfo;

        private string _keyGetterFunction;
        private string _keyGetterFunctionEnd;
        private string _keySerializerFunctionStart;
        private string _keySerializerFunctionEnd;

        private string _keySetterFunctionStart;
        private string _keySetterFunctionEnd;

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

            #region Get Key/Value Type

            Type keyType = typeof(object);
            Type valueType = typeof(object);

            if (_type.GetInterface("IDictionary") != null)
            {
                Type dictionaryType = null;
                if (_type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    dictionaryType = _type;
                }
                else
                {
                    dictionaryType = _type.GetInterfaces()
                        .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                        .FirstOrDefault();
                }

                if (dictionaryType != null)
                {
                    keyType = dictionaryType.GetGenericArguments()[0];
                    valueType = dictionaryType.GetGenericArguments()[1];
                }
            }
            else
            {
                throw new InvalidOperationException("Type is not IDictionary!");
            }

            #endregion

            #region Key

            _keyTypeInfo = new CompilerTypeInfo(keyType);

            // Determine node type for key (Can not be Text or TextCData)
            _keySerializationMethod = _keyTypeInfo.SerializationAttribute.ChildKeyNodeType;
            if (_keySerializationMethod != SerializationNodeType.Element && _keySerializationMethod != SerializationNodeType.Attribute && _keySerializationMethod != SerializationNodeType.ElementAsCData)
            {
                if (_keyTypeInfo.SerializerConfigured || _keyTypeInfo.DeserializerConfigured)
                    _keySerializationMethod = SerializationNodeType.Attribute;
                else
                    _keySerializationMethod = SerializationNodeType.Element;
            }

            #endregion

            #region Value

            _valueTypeInfo = new CompilerTypeInfo(valueType);

            // Determine node type for value
            _valueSerializationMethod = _valueTypeInfo.SerializationAttribute.ChildValueNodeType;
            if (_valueSerializationMethod == SerializationNodeType.Auto)
            {
                if (_valueTypeInfo.SerializerConfigured || _valueTypeInfo.DeserializerConfigured)
                    _valueSerializationMethod = SerializationNodeType.Text;
                else
                    _valueSerializationMethod = SerializationNodeType.Element;
            }

            // Text types with custom serializers are not supported. Convert them to element / cdata element
            if (_valueSerializationMethod == SerializationNodeType.Text || _valueSerializationMethod == SerializationNodeType.TextAsCData)
            {
                if (!_valueTypeInfo.SerializerConfigured && !_valueTypeInfo.DeserializerConfigured)
                {
                    _valueSerializationMethod = _valueSerializationMethod == SerializationNodeType.TextAsCData
                        ? SerializationNodeType.ElementAsCData
                        : SerializationNodeType.Element;
                }
            }


            // Fix value node type by key node type
            if (_keySerializationMethod == SerializationNodeType.Element || _keySerializationMethod == SerializationNodeType.ElementAsCData)
            {
                if (_valueSerializationMethod != SerializationNodeType.ElementAsCData)
                    _valueSerializationMethod = SerializationNodeType.Element;
            }

            #endregion

        }

        private void ConfigureGetter(StringBuilder classLevelCodePart)
        {
            _valueGetterFunction = EnumerableCompiler.ChildInstanceName + ".Value";
            _keyGetterFunction = EnumerableCompiler.ChildInstanceName + ".Key";

            // If nullable, use .Value suffix for value getter
            if (_valueTypeInfo.IsNullableType)
                _valueGetterFunctionEnd = ".Value";
            else
                _valueGetterFunctionEnd = "";

            // If nullable, use .Value suffix for key getter
            if (_keyTypeInfo.IsNullableType)
                _keyGetterFunctionEnd = ".Value";
            else
                _keyGetterFunctionEnd = "";
        }

        private void ConfigureSetter(StringBuilder classLevelCodePart)
        {
            _valueSetterFunctionStart = "value = ";
            _valueSetterFunctionEnd = ";";

            _keySetterFunctionStart = "key = ";
            _keySetterFunctionEnd = ";";
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

            if ((_keyTypeInfo.ShouldUseTypeConverterOnSerialization || _keyTypeInfo.ShouldUseTypeConverterOnDeserialization))
            {
                // var tck"MemberName" = System.ComponentModel.TypeDescriptor.GetConverter(typeof("KeyType"));
                classLevelCodePart.Append("var tck");
                classLevelCodePart.Append(" = System.ComponentModel.TypeDescriptor.GetConverter(typeof(");
                classLevelCodePart.Append(_keyTypeInfo.TypeName);
                classLevelCodePart.AppendLine("));");
            }
        }

        private void ConfigureSerializer(StringBuilder classLevelCodePart)
        {
            var valueSerializerInstance = "SerializerFactory<" + _valueTypeInfo.TypeName + ">.Instance";
            string valueOptionsInstance = PropertyCompiler.ConfigureSerializerOptions(classLevelCodePart, valueSerializerInstance + ".DefaultSerializationOptions", _valueTypeInfo.SerializationOptions, null, "valueSerializerOptions");

            if (!_valueTypeInfo.DeserializerConfigured)
            {
                _valueSetterFunctionStart += valueSerializerInstance + ".Deserialize(reader, ";
                _valueSetterFunctionStart += valueOptionsInstance + ")";
            }

            if (!_valueTypeInfo.SerializerConfigured)
            {
                _valueSerializerFunctionStart = valueSerializerInstance + ".Serialize(writer, ";
                _valueSerializerFunctionEnd = ", " + valueOptionsInstance + ");";
            }

            var keySerializerInstance = "SerializerFactory<" + _keyTypeInfo.TypeName + ">.Instance";
            string keyOptionsInstance = PropertyCompiler.ConfigureSerializerOptions(classLevelCodePart, keySerializerInstance + ".DefaultSerializationOptions", _keyTypeInfo.SerializationOptions, null, "keySerializerOptions");

            if (!_keyTypeInfo.DeserializerConfigured)
            {
                _keySetterFunctionStart += keySerializerInstance + ".Deserialize(reader, ";
                _keySetterFunctionStart += keyOptionsInstance + ")";
            }

            if (!_keyTypeInfo.SerializerConfigured)
            {
                _keySerializerFunctionStart = keySerializerInstance + ".Serialize(writer, ";
                _keySerializerFunctionEnd = ", " + keyOptionsInstance + ");";
            }

        }

        #endregion

        #region Writer Methods

        public void AppendWriteChildToXml(StringBuilder code)
        {
            bool isText = _valueSerializationMethod == SerializationNodeType.Text || _valueSerializationMethod == SerializationNodeType.TextAsCData;

            if (!isText)
                AppendWriteKeyToXml(code);

            if (_valueTypeInfo.CanBeNull)
            {
                // if("Value" != null) {
                code.Append("if(");
                code.Append(_valueGetterFunction);
                code.AppendLine(" != null) {");
                if (isText)
                    AppendWriteKeyToXml(code);
            }

            if (_valueSerializationMethod == SerializationNodeType.Attribute)
            {
                code.Append("writer.WriteAttributeString(options.ValueNodeName, ");
                Compiler.AppendToString(code, _valueTypeInfo, _valueGetterFunction + _valueGetterFunctionEnd, null);
                code.AppendLine(");");
            }
            else
            {
                // writer.WriteStartElement("Name");
                if (!isText)
                    code.Append("writer.WriteStartElement(options.ValueNodeName);");

                if (_valueSerializationMethod == SerializationNodeType.ElementAsCData || _valueSerializationMethod == SerializationNodeType.TextAsCData)
                    Compiler.AppendWriteCData(code, _valueTypeInfo, _valueGetterFunction + _valueGetterFunctionEnd, null);
                else
                    AppendWriteValueToXml(code, _valueGetterFunction + _valueGetterFunctionEnd);

                // writer.WriteEndElement();
                if (!isText)
                    code.AppendLine("writer.WriteEndElement();");
            }

            // Write null as type, then key.
            if (_valueTypeInfo.CanBeNull)
            {
                code.AppendLine("}");
                if (isText)
                {
                    code.AppendLine("else {");
                    code.AppendLine("  writer.WriteAttributeString(Serializer.TypeAttributeName, \"null\");");
                    AppendWriteKeyToXml(code);
                    code.AppendLine("}");
                }
            }
        }

        private void AppendWriteKeyToXml(StringBuilder code)
        {
            // Write key
            if (_keySerializationMethod == SerializationNodeType.Attribute)
            {
                code.Append("writer.WriteAttributeString(options.KeyNodeName, ");
                Compiler.AppendToString(code, _keyTypeInfo, _keyGetterFunction + _keyGetterFunctionEnd, null);
                code.AppendLine(");");
            }
            else
            {
                code.AppendLine("writer.WriteStartElement(options.KeyNodeName);");

                if (_keySerializationMethod == SerializationNodeType.ElementAsCData)
                    Compiler.AppendWriteCData(code, _keyTypeInfo, _keyGetterFunction + _keyGetterFunctionEnd, null);
                else
                    AppendWriteKeyValueToXml(code, _keyGetterFunction + _keyGetterFunctionEnd);

                code.AppendLine("writer.WriteEndElement();");
            }
        }

        private void AppendWriteKeyValueToXml(StringBuilder code, string getterFunction)
        {
            if (!_keyTypeInfo.SerializerConfigured)
            {
                // "serializer".Serialize(writer, "Key", options);
                code.Append(_keySerializerFunctionStart);
                code.Append(getterFunction);
                code.AppendLine(_keySerializerFunctionEnd);
            }
            else
            {
                // writer.WriteString("KeyAsString");
                code.Append("writer.WriteString(");
                Compiler.AppendToString(code, _keyTypeInfo, getterFunction, null);
                code.AppendLine(");");
            }
        }

        private void AppendWriteValueToXml(StringBuilder code, string getterFunction)
        {
            if (!_valueTypeInfo.SerializerConfigured)
            {
                // "serializer".Serialize(writer, "Value", options);
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

        public void AppendKeyVariableDeclarations(StringBuilder code)
        {
            code.AppendLine("var keyFound = false;");
            code.Append("var key = default(");
            code.Append(_keyTypeInfo.TypeName);
            code.AppendLine(");");

            code.AppendLine("var valueFound = false;");
            code.Append("var value = default(");
            code.Append(_valueTypeInfo.TypeName);
            code.AppendLine(");");
        }

        public void AppendReadChildFromXml(StringBuilder code)
        {
            code.AppendLine("keyFound = false;");
            code.AppendLine("valueFound = false;");

            AppendReadXmlAttributes(code);

            var isText = _valueSerializationMethod == SerializationNodeType.Text || _valueSerializationMethod == SerializationNodeType.TextAsCData;

            if (isText)
                AppendReadValueFromXmlText(code);

            AppendReadXmlElements(code);

            // If key could not be read, throw exception
            code.AppendLine();
            code.AppendLine("if(!keyFound)");

            code.Append("  throw new XmlSerializationException(\"");
            if (_keySerializationMethod == SerializationNodeType.Attribute)
                code.Append(string.Format(Resources.XmlMissingDictionaryKeyAttributeExceptionMessage, _keyTypeInfo.TypeName));
            else
                code.Append(string.Format(Resources.XmlMissingDictionaryKeyElementExceptionMessage, _keyTypeInfo.TypeName));
            code.AppendLine("\");");

            // If value could not be read, throw exception or assign null
            code.AppendLine();
            code.AppendLine("if(!valueFound)");

            // If value could not be read , throw exception
            if (!_valueTypeInfo.CanBeNull)
            {
                code.Append("  throw new XmlSerializationException(\"");
                if (isText)
                    code.Append(string.Format(Resources.XmlMissingDictionaryValueTextExceptionMessage, _keyTypeInfo.TypeName));
                if (_keySerializationMethod == SerializationNodeType.Attribute)
                    code.Append(string.Format(Resources.XmlMissingDictionaryValueAttributeExceptionMessage, _keyTypeInfo.TypeName));
                else
                    code.Append(string.Format(Resources.XmlMissingDictionaryValueElementExceptionMessage, _keyTypeInfo.TypeName));
                code.AppendLine("\");");
            }
            // If string is not read from text, assume it as empty string.
            else if (isText && _valueTypeInfo.RealValueType == typeof(string))
            {
                code.AppendLine("  value = string.Empty;");
            }
            else
            {
                code.AppendLine("  value = null;");
            }

            code.AppendLine();
            code.AppendLine(Compiler.ObjectInstanceName + "[key] = value;");
        }

        public void AppendReadXmlAttributes(StringBuilder code)
        {
            if (_keySerializationMethod == SerializationNodeType.Attribute)
            {
                code.AppendLine("if(reader.MoveToFirstAttribute()) {");
                code.AppendLine("  do { ");

                code.AppendLine("    if(reader.LocalName == options.KeyNodeName) {");
                code.AppendLine("      keyFound = true;");

                // Read key from xml
                code.Append(_keySetterFunctionStart);
                if (_keyTypeInfo.DeserializerConfigured)
                    Compiler.AppendParseString(code, _keyTypeInfo, null, _keyTypeInfo.TypeName);
                code.Append(_keySetterFunctionEnd);

                code.AppendLine("    }");

                if (_valueSerializationMethod == SerializationNodeType.Attribute)
                {
                    code.AppendLine("    else if(reader.LocalName == options.ValueNodeName) {");
                    code.AppendLine("      valueFound = true;");

                    // Read key from xml
                    code.Append(_valueSetterFunctionStart);
                    if (_valueTypeInfo.DeserializerConfigured)
                        Compiler.AppendParseString(code, _valueTypeInfo, null, _valueTypeInfo.TypeName);
                    code.Append(_valueSetterFunctionEnd);

                    code.AppendLine("    }");
                }

                var isText = _valueSerializationMethod == SerializationNodeType.Text || _valueSerializationMethod == SerializationNodeType.TextAsCData;
                if (_valueTypeInfo.CanBeNull && isText)
                {
                    code.AppendLine("    else if(reader.LocalName == Serializer.TypeAttributeName && reader.Value == \"null\") {");
                    code.AppendLine("      valueFound = true;");
                    code.AppendLine("      value = null;");
                    code.AppendLine("    }");
                }

                code.AppendLine("  } while (reader.MoveToNextAttribute());");

                code.AppendLine("  reader.MoveToElement();");
                code.AppendLine("}");
            }
        }

        public void AppendReadValueFromXmlText(StringBuilder code)
        {
            if (!_valueTypeInfo.DeserializerConfigured)
            {
                if (_valueTypeInfo.CanBeNull)
                    code.AppendLine("if(!valueFound)");
                code.Append(_valueSetterFunctionStart);
                code.AppendLine(_valueSetterFunctionEnd);
            }
            else
            {
                code.AppendLine("if(!reader.IsEmptyElement) {");
                code.AppendLine("  reader.Read();"); // Move to value
                code.AppendLine("  valueFound = true;");

                code.Append(_valueSetterFunctionStart);
                Compiler.AppendParseString(code, _valueTypeInfo, null, _valueTypeInfo.TypeName);
                code.AppendLine(_valueSetterFunctionEnd);
                code.AppendLine("  reader.MoveUpToNextElement();"); // Consume element

                code.AppendLine("} else {");
                code.AppendLine("  reader.Skip();"); // If empty element, skip element
                code.AppendLine("}");
            }
        }

        public void AppendReadXmlElements(StringBuilder code)
        {
            if (_valueSerializationMethod == SerializationNodeType.Element || _valueSerializationMethod == SerializationNodeType.ElementAsCData)
            {
                code.AppendLine("if(!reader.IsEmptyElement)");
                code.AppendLine("  while (reader.Read()) {");
                code.AppendLine("    while (reader.NodeType == XmlNodeType.Element) {");

                if (_keySerializationMethod != SerializationNodeType.Attribute)
                {
                    code.AppendLine("      if(reader.LocalName == options.KeyNodeName) {");

                    if (!_keyTypeInfo.DeserializerConfigured)
                    {
                        code.AppendLine("        keyFound = true;");
                        code.Append(_keySetterFunctionStart);
                        code.AppendLine(_keySetterFunctionEnd);
                    }
                    else
                    {
                        code.AppendLine("if(!reader.IsEmptyElement) {");
                        code.AppendLine("  reader.Read();"); // Move to value
                        code.AppendLine("  keyFound = true;");

                        code.Append(_keySetterFunctionStart);
                        Compiler.AppendParseString(code, _keyTypeInfo, null, _keyTypeInfo.TypeName);
                        code.AppendLine(_keySetterFunctionEnd);
                        code.AppendLine("  reader.MoveUpToNextElement();"); // Consume element

                        code.AppendLine("} else {");
                        code.AppendLine("  reader.Skip();"); // If empty element, skip element
                        code.AppendLine("}");
                    }

                    code.AppendLine("      }");
                    code.AppendLine("      else");
                }

                code.AppendLine("      if(reader.LocalName == options.ValueNodeName) {");
                code.AppendLine("        valueFound = true;");

                // Read key from xml
                code.Append(_valueSetterFunctionStart);
                if (_valueTypeInfo.DeserializerConfigured)
                    Compiler.AppendParseString(code, _valueTypeInfo, null, _valueTypeInfo.TypeName);
                code.Append(_valueSetterFunctionEnd);

                code.AppendLine("      } else reader.Skip();");
                code.AppendLine("    }");

                code.AppendLine("    if(reader.NodeType == XmlNodeType.EndElement)");
                code.AppendLine("      break;");

                code.AppendLine("  }");
                code.AppendLine("reader.Read();");
            }
        }

        #endregion

    }
}