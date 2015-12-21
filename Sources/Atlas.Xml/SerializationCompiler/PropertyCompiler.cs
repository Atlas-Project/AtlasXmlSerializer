using System.Reflection;
using System.Text;

namespace Atlas.Xml.SerializationCompiler
{
    internal class PropertyCompiler
    {

        #region Constructor & Initial Fields

        readonly MemberInfo _member;
        readonly XmlSerializationMemberAttribute _serializationAttribute;

        public PropertyCompiler(MemberInfo property, int order, StringBuilder classLevelCodePart)
        {
            _member = property;
            _serializationAttribute = XmlSerializationMemberAttribute.FromMember(_member);

            Order = order;

            Initialize(classLevelCodePart);
        }

        #endregion

        #region Public Properties

        public SerializationNodeType SerializationMethod { get; private set; }

        public int SerializationTypeOrder
        {
            get
            {
                return SerializationMethod == SerializationNodeType.Attribute ? 0 : 1;
            }
        }

        public int InheritanceDepth { get; private set; }

        public int Order { get; private set; }

        #endregion

        #region Fields

        private CompilerPropertyInfo _property;
        private string _xmlName;
        private string _reflectedTypeName;

        private string _getterFunction;
        private string _getterFunctionEnd;
        private string _serializerFunctionStart;
        private string _serializerFunctionEnd;

        private string _setterFunctionStart;
        private string _setterFunctionEnd;

        #endregion

        #region Initialization

        private void Initialize(StringBuilder classLevelCodePart)
        {
            ConfigureAttributes();

            SerializationMethod = _serializationAttribute.NodeType;
            if (SerializationMethod != SerializationNodeType.None)
            {
                _property = new CompilerPropertyInfo(_member);
                if (SerializationMethod == SerializationNodeType.Auto)
                    SerializationMethod = _property.PrefferedSerializationType;

                if (SerializationMethod != SerializationNodeType.None)
                {
                    ConfigureMemberType();
                    ConfigureSerializationOrder();
                    ConfigureGetter(classLevelCodePart);
                    ConfigureSetter(classLevelCodePart);
                    ConfigureTypeConverter(classLevelCodePart);
                    ConfigureSerializer(classLevelCodePart);
                }
            }
        }

        private void ConfigureSerializationOrder()
        {
            var type = _member.DeclaringType;
            while (type != typeof(object))
            {
                InheritanceDepth++;
                type = type.BaseType;
            }
        }

        private void ConfigureAttributes()
        {
            if (_serializationAttribute.NodeType != SerializationNodeType.None)
            {
                _xmlName = _serializationAttribute.NodeName;
                if (_serializationAttribute.Order >= 0)
                    Order = _serializationAttribute.Order;
            }
        }

        private void ConfigureMemberType()
        {
            if (string.IsNullOrEmpty(_xmlName))
                _xmlName = _member.Name;

            _reflectedTypeName = CachedTypeResolver.GetFriendlyNameForCode(_member.ReflectedType);
        }

        private void ConfigureGetter(StringBuilder classLevelCodePart)
        {
            if (_property.ShouldUseCustomSerializationMethod)
            {
                _getterFunction = "get" + _member.Name;

                classLevelCodePart.Append("Func<");

                if (_property.IsStatic)
                {
                    classLevelCodePart.Append("string> ");
                    classLevelCodePart.Append(_getterFunction);
                    classLevelCodePart.Append(" = ReflectionHelper.CreateStaticGetStringMethod<");

                    _getterFunction += "()";
                }
                else
                {
                    classLevelCodePart.Append(_reflectedTypeName);
                    classLevelCodePart.Append(", string> ");
                    classLevelCodePart.Append(_getterFunction);
                    classLevelCodePart.Append(" = ReflectionHelper.CreateGetStringMethod<");

                    _getterFunction += "(" + Compiler.ObjectInstanceName + ")";
                }

                classLevelCodePart.Append(_reflectedTypeName);
                classLevelCodePart.Append(">(\"");
                classLevelCodePart.Append(_member.Name);
                classLevelCodePart.Append(CompilerPropertyInfo.CustomSerializationMethodSuffix);
                classLevelCodePart.AppendLine("\");");
            }
            else if (_property.IsPublicGet)
            {
                _getterFunction = (_property.IsStatic ? _reflectedTypeName : Compiler.ObjectInstanceName)
                    + "." + _member.Name;
            }
            else if (_property.CanGet)
            {
                _getterFunction = "get" + _member.Name;

                classLevelCodePart.Append("Func<");

                if (_property.IsStatic)
                {
                    classLevelCodePart.Append(_property.TypeName);
                    classLevelCodePart.Append("> ");
                    classLevelCodePart.Append(_getterFunction);
                    classLevelCodePart.Append(" = ReflectionHelper.CreateStaticGetter<");

                    _getterFunction += "()";
                }
                else
                {
                    classLevelCodePart.Append(_reflectedTypeName);
                    classLevelCodePart.Append(", ");
                    classLevelCodePart.Append(_property.TypeName);
                    classLevelCodePart.Append("> ");
                    classLevelCodePart.Append(_getterFunction);
                    classLevelCodePart.Append(" = ReflectionHelper.CreateGetter<");

                    _getterFunction += "(" + Compiler.ObjectInstanceName + ")";
                }

                classLevelCodePart.Append(_reflectedTypeName);
                classLevelCodePart.Append(", ");
                classLevelCodePart.Append(_property.TypeName);
                classLevelCodePart.Append(">(\"");
                classLevelCodePart.Append(_member.Name);
                classLevelCodePart.AppendLine("\");");
            }

            // If nullable, use .Value suffix for getter
            if (_property.IsNullableType && !_property.ShouldUseCustomSerializationMethod)
                _getterFunctionEnd = ".Value";
            else
                _getterFunctionEnd = "";
        }

        private void ConfigureSetter(StringBuilder classLevelCodePart)
        {
            if (_property.ShouldUseCustomDeserializationMethod)
            {
                var setterFunction = "set" + _member.Name;

                _setterFunctionStart = setterFunction + "(";
                _setterFunctionEnd = ");";

                classLevelCodePart.Append("Action<");

                if (_property.IsStatic)
                {
                    classLevelCodePart.Append("string> ");
                    classLevelCodePart.Append(setterFunction);
                    classLevelCodePart.Append(" = ReflectionHelper.CreateStaticSetStringMethod<");
                }
                else
                {
                    classLevelCodePart.Append(_reflectedTypeName);
                    classLevelCodePart.Append(", string> ");
                    classLevelCodePart.Append(setterFunction);
                    classLevelCodePart.Append(" = ReflectionHelper.CreateSetStringMethod<");

                    _setterFunctionStart += Compiler.ObjectInstanceName + ", ";
                }
                classLevelCodePart.Append(_reflectedTypeName);
                classLevelCodePart.Append(">(\"");
                classLevelCodePart.Append(_member.Name);
                classLevelCodePart.Append(CompilerPropertyInfo.CustomDeserializationMethodSuffix);
                classLevelCodePart.AppendLine("\");");
            }
            else if (_property.IsPublicSet)
            {
                _setterFunctionStart = (_property.IsStatic ? _reflectedTypeName : Compiler.ObjectInstanceName)
                        + "." + _member.Name + " = ";
                _setterFunctionEnd = ";";
            }
            // If not public settable, create setter method
            else if (_property.CanSet)
            {
                var setterFunction = "set" + _member.Name;

                _setterFunctionStart = setterFunction + "(";
                _setterFunctionEnd = ");";

                classLevelCodePart.Append("Action<");

                if (_property.IsStatic)
                {
                    classLevelCodePart.Append(_property.TypeName);
                    classLevelCodePart.Append("> ");
                    classLevelCodePart.Append(setterFunction);
                    classLevelCodePart.Append(" = ReflectionHelper.CreateStaticSetter<");
                }
                else
                {
                    classLevelCodePart.Append(_reflectedTypeName);
                    classLevelCodePart.Append(", ");
                    classLevelCodePart.Append(_property.TypeName);
                    classLevelCodePart.Append("> ");
                    classLevelCodePart.Append(setterFunction);
                    classLevelCodePart.Append(" = ReflectionHelper.CreateSetter<");

                    _setterFunctionStart += Compiler.ObjectInstanceName + ", ";
                }

                classLevelCodePart.Append(_reflectedTypeName);
                classLevelCodePart.Append(", ");
                classLevelCodePart.Append(_property.TypeName);
                classLevelCodePart.Append(">(\"");
                classLevelCodePart.Append(_member.Name);
                classLevelCodePart.AppendLine("\");");
            }
        }

        private void ConfigureTypeConverter(StringBuilder classLevelCodePart)
        {
            // Converter declaration
            if (_property.ShouldUseTypeConverterOnSerialization || _property.ShouldUseTypeConverterOnDeserialization)
            {
                // var tc"MemberName" = System.ComponentModel.TypeDescriptor.GetConverter(typeof("ValueType"));
                classLevelCodePart.Append("System.ComponentModel.TypeConverter tc");
                classLevelCodePart.Append(_member.Name);
                classLevelCodePart.Append(" = System.ComponentModel.TypeDescriptor.GetConverter(typeof(");
                classLevelCodePart.Append(_property.TypeName);
                classLevelCodePart.AppendLine("));");
            }
        }

        private void ConfigureSerializer(StringBuilder classLevelCodePart)
        {
            var serializerInstance = "SerializerFactory<" + _property.TypeName + ">.Instance";
            string serializerOptionsInstance = ConfigureSerializerOptions(classLevelCodePart, serializerInstance + ".DefaultSerializationOptions", _serializationAttribute.ToOptions(), _member.Name);

            if (!_property.DeserializerConfigured && (_property.CanSet || (_property.IsCollection && _property.CanGet)))
            {
                if (_property.IsCollection && !_property.CanSet)
                {
                    _setterFunctionStart = string.Empty;
                    _setterFunctionEnd = ";";
                }

                _setterFunctionStart += serializerInstance + ".Deserialize(reader, ";

                // If property is readonly, deserialize with existing instance
                if (_property.IsCollection && !_property.CanSet)
                    _setterFunctionStart += _getterFunction + ", ";

                _setterFunctionStart += serializerOptionsInstance + ")";
            }

            if (!_property.SerializerConfigured && _property.CanGet)
            {
                _serializerFunctionStart = serializerInstance + ".Serialize(writer, ";
                _serializerFunctionEnd = ", " + serializerOptionsInstance + ");";
            }
        }

        public static string ConfigureSerializerOptions(StringBuilder classLevelCodePart, string defaultOptionsCode, SerializationOptions options, string memberName = null, string variableNamePrefix = "serializerOptionsFor")
        {
            string serializerOptionsInstance;
            if (options == null || options.IsDefault)
            {
                serializerOptionsInstance = defaultOptionsCode;
            }
            else
            {
                // var collectionSerializerOptions"MemberName" = new SerializationOptions { .. };
                serializerOptionsInstance = variableNamePrefix + memberName;

                classLevelCodePart.Append("SerializationOptions ");
                classLevelCodePart.Append(serializerOptionsInstance);
                classLevelCodePart.AppendLine(" = new SerializationOptions {");

                if (options.ChildElementName != null)
                {
                    classLevelCodePart.Append("  ChildElementName = \"");
                    classLevelCodePart.Append(options.ChildElementName ?? "null");
                    classLevelCodePart.AppendLine("\", ");
                }

                if (options.ValueNodeName != null)
                {
                    classLevelCodePart.Append("  ValueNodeName = \"");
                    classLevelCodePart.Append(options.ValueNodeName ?? "null");
                    classLevelCodePart.AppendLine("\", ");
                }

                if (options.KeyNodeName != null)
                {
                    classLevelCodePart.Append("  KeyNodeName = \"");
                    classLevelCodePart.Append(options.KeyNodeName);
                    classLevelCodePart.AppendLine("\", ");
                }

                classLevelCodePart.Append("};");
            }

            return serializerOptionsInstance;
        }

        #endregion

        #region Writer Methods

        public void AppendWriteToXml(StringBuilder code)
        {
            var getterFunction = _getterFunction;

            bool isCodeBlock = false;
            var isElement = SerializationMethod == SerializationNodeType.Element || SerializationMethod == SerializationNodeType.ElementAsCData;

            if (_property.CanBeNull)
            {
                if (!_property.IsPublicGet)
                {
                    // var tmp"Member" = "GetMemberValue";
                    isCodeBlock = true;
                    getterFunction = "tmp" + _member.Name;

                    code.Append("var ");
                    code.Append(getterFunction);
                    code.Append(" = ");
                    code.Append(_getterFunction);
                    code.AppendLine(";");
                }
                else if (isElement)
                {
                    isCodeBlock = true;
                }

                // if("Value" != null)
                // or:
                // if(tmp"Member" != null) {
                code.Append("if(");
                code.Append(getterFunction);
                code.AppendLine(" != null)");

                if (isCodeBlock)
                    code.AppendLine("{");
            }

            getterFunction += _getterFunctionEnd;

            if (SerializationMethod == SerializationNodeType.Attribute)
            {
                // writer.WriteAttributeString("Name", "ValueAsString");
                code.Append("writer.WriteAttributeString(\"");
                code.Append(_xmlName);
                code.Append("\", ");
                Compiler.AppendToString(code, _property, getterFunction, _member.Name);
                code.AppendLine(");");
            }
            else
            {
                if (isElement)
                {
                    // writer.WriteStartElement("Name");
                    code.Append("writer.WriteStartElement(\"");
                    code.Append(_xmlName);
                    code.AppendLine("\");");
                }

                if (SerializationMethod == SerializationNodeType.ElementAsCData || SerializationMethod == SerializationNodeType.TextAsCData)
                    Compiler.AppendWriteCData(code, _property, getterFunction, _member.Name);
                else
                    AppendWriteValueToXml(code, getterFunction);

                // writer.WriteEndElement();
                if (isElement)
                    code.AppendLine("writer.WriteEndElement();");
            }

            if (isCodeBlock)
                code.AppendLine("}");

            code.AppendLine();
        }

        private void AppendWriteValueToXml(StringBuilder code, string getterFunction)
        {
            if (!_property.SerializerConfigured)
            {
                // "serializer".Serialize(writer, "ValueAsString", options);
                code.Append(_serializerFunctionStart);
                code.Append(getterFunction);
                code.AppendLine(_serializerFunctionEnd);
            }
            else
            {
                // writer.WriteString("ValueAsString");
                code.Append("writer.WriteString(");
                Compiler.AppendToString(code, _property, getterFunction, _member.Name);
                code.AppendLine(");");
            }
        }

        #endregion

        #region Reader Methods

        public void AppendReadElementCase(StringBuilder code)
        {
            if ((_property.CanSet || _property.IsCollection) && (SerializationMethod == SerializationNodeType.Element || SerializationMethod == SerializationNodeType.ElementAsCData))
            {
                AppendCaseStart(code);
                AppendReadValueFromXml(code, true);
                AppendCaseEnd(code);
            }
        }

        public void AppendReadAttributeCase(StringBuilder code)
        {
            if (_property.CanSet && SerializationMethod == SerializationNodeType.Attribute)
            {
                AppendCaseStart(code);
                AppendReadValueFromXml(code, false);
                AppendCaseEnd(code);
            }
        }

        public void AppendReadText(StringBuilder code)
        {
            if (_property.CanSet && (SerializationMethod == SerializationNodeType.Text || SerializationMethod == SerializationNodeType.TextAsCData))
                AppendReadValueFromXml(code, false);
        }

        public void AppendReadTextEmptyStringFix(StringBuilder code)
        {
            if (_property.CanSet && (SerializationMethod == SerializationNodeType.Text || SerializationMethod == SerializationNodeType.TextAsCData))
            {
                if (_property.ValueType == typeof(string))
                {
                    code.Append("if(");
                    code.Append(_getterFunction);
                    code.AppendLine(" == null)");
                    code.Append(_setterFunctionStart);
                    code.Append("\"\"");
                    code.Append(_setterFunctionEnd);
                }
            }
        }

        private void AppendCaseStart(StringBuilder code)
        {
            // case "Name":
            code.Append("case \"");
            code.Append(_xmlName);
            code.AppendLine("\":");
        }

        private void AppendCaseEnd(StringBuilder code)
        {
            // break;
            code.AppendLine("break;");
            code.AppendLine();
        }

        private void AppendReadValueFromXml(StringBuilder code, bool isElement)
        {
            if (!_property.DeserializerConfigured)
            {
                if (isElement)
                {
                    // "Instance"."Member" = "serializer".Deserialize(reader)
                    code.Append(_setterFunctionStart);
                    code.AppendLine(_setterFunctionEnd);
                }
            }
            else
            {
                if (isElement)
                {
                    // if(!reader.IsEmptyElement) {
                    code.AppendLine("if(!reader.IsEmptyElement) {");
                    // reader.Read();
                    code.AppendLine("reader.Read();"); // Move to value
                }

                code.Append(_setterFunctionStart);
                Compiler.AppendParseString(code, _property, _member.Name, _property.TypeName);
                code.AppendLine(_setterFunctionEnd);

                if (isElement)
                {
                    code.AppendLine("reader.MoveUpToNextElement();"); // Consume value
                    code.AppendLine("} else {");

                    code.Append(_setterFunctionStart);
                    Compiler.AppendParseString(code, _property, _member.Name, _property.TypeName, "\"\"");
                    code.AppendLine(_setterFunctionEnd);

                    code.AppendLine("reader.Skip();"); // Skip tp next element
                    code.AppendLine("}");
                }
            }
        }

        #endregion

    }
}