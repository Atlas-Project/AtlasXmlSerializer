using System;
using System.Text;

namespace Atlas.Xml.SerializationCompiler
{
    internal class ValueTypeCompiler
    {

        #region Constructor & Initial Fields

        readonly Type _type;
        readonly XmlSerializationTypeAttribute _serializationAttribute;

        public ValueTypeCompiler(Type type, StringBuilder classLevelCodePart)
        {
            _type = type;
            _serializationAttribute = XmlSerializationTypeAttribute.FromType(_type);

            Initialize(classLevelCodePart);
        }

        #endregion

        #region Public Properties

        public SerializationNodeType SerializationMethod { get; private set; }

        #endregion

        #region Fields

        private CompilerTypeInfo _typeInfo;

        private string _getterFunction;
        private string _getterFunctionEnd;

        private string _setterFunctionStart = string.Empty;
        private string _setterFunctionEnd;

        #endregion

        #region Initialization

        private void Initialize(StringBuilder classLevelCodePart)
        {
            SerializationMethod = _serializationAttribute.NodeType;
            if (SerializationMethod != SerializationNodeType.None)
            {
                _typeInfo = new CompilerTypeInfo(_type);
                if (SerializationMethod == SerializationNodeType.Auto)
                    SerializationMethod = SerializationNodeType.Text;

                if (SerializationMethod != SerializationNodeType.None)
                {
                    ConfigureGetter(classLevelCodePart);
                    ConfigureSetter(classLevelCodePart);
                    ConfigureTypeConverter(classLevelCodePart);
                }
            }
        }

        private void ConfigureGetter(StringBuilder classLevelCodePart)
        {
            _getterFunction = Compiler.ObjectInstanceName;

            // If nullable, use .Value suffix for getter
            if (_typeInfo.IsNullableType && !_typeInfo.ShouldUseCustomSerializationMethod)
                _getterFunctionEnd = ".Value";
            else
                _getterFunctionEnd = "";
        }

        private void ConfigureSetter(StringBuilder classLevelCodePart)
        {
            _setterFunctionStart = Compiler.ObjectInstanceName + " = ";
            _setterFunctionEnd = ";";
        }

        private void ConfigureTypeConverter(StringBuilder classLevelCodePart)
        {
            // Converter declaration
            if (_typeInfo.ShouldUseTypeConverterOnSerialization || _typeInfo.ShouldUseTypeConverterOnDeserialization)
            {
                // var tc"MemberName" = System.ComponentModel.TypeDescriptor.GetConverter(typeof("ValueType"));
                classLevelCodePart.Append("System.ComponentModel.TypeConverter tc");
                classLevelCodePart.Append(" = System.ComponentModel.TypeDescriptor.GetConverter(typeof(");
                classLevelCodePart.Append(_typeInfo.TypeName);
                classLevelCodePart.AppendLine("));");
            }
        }

        #endregion

        #region Writer Methods

        public void AppendWriteToXml(StringBuilder code)
        {
            var getterFunction = _getterFunction;

            if (_typeInfo.CanBeNull)
            {
                // if("Value" != null)
                // or:
                // if(tmp"Member" != null) {
                code.Append("if(");
                code.Append(getterFunction);
                code.AppendLine(" != null)");
            }

            getterFunction += _getterFunctionEnd;

            if (SerializationMethod == SerializationNodeType.ElementAsCData || SerializationMethod == SerializationNodeType.TextAsCData)
                Compiler.AppendWriteCData(code, _typeInfo, getterFunction, null);
            else
                AppendWriteValueToXml(code, getterFunction);

            code.AppendLine();
        }

        private void AppendWriteValueToXml(StringBuilder code, string getterFunction)
        {
            // writer.WriteString("ValueAsString");
            code.Append("writer.WriteString(");
            Compiler.AppendToString(code, _typeInfo, getterFunction, null);
            code.AppendLine(");");
        }

        #endregion

        #region Reader Methods

        public void AppendReadText(StringBuilder code)
        {
            AppendReadValueFromXml(code, false);
        }

        public void AppendNullCheck(StringBuilder code)
        {
            if (!_typeInfo.CanBeNull)
            {
                code.AppendLine("else {");
                code.Append("  throw new XmlSerializationException(\"");
                code.Append(string.Format(Resources.XmlMissingValueTextExceptionMessage, _typeInfo.TypeName));
                code.AppendLine("\");");
                code.AppendLine("}");
            }
        }

        public void AppendReadTextEmptyStringFix(StringBuilder code)
        {
            if (_typeInfo.ValueType == typeof(string))
            {
                code.Append("if(");
                code.Append(_getterFunction);
                code.AppendLine(" == null)");
                code.Append(_setterFunctionStart);
                code.Append("\"\"");
                code.Append(_setterFunctionEnd);
            }
        }

        private void AppendReadValueFromXml(StringBuilder code, bool isElement)
        {
            code.Append(_setterFunctionStart);
            Compiler.AppendParseString(code, _typeInfo, null, _typeInfo.TypeName);
            code.AppendLine(_setterFunctionEnd);
        }

        #endregion

    }
}