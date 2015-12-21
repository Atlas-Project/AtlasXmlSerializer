using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Atlas.Xml.SerializationCompiler
{
    /// <summary>
    /// Creates new serialization class from type
    /// </summary>
    public static class Compiler
    {

        #region Constants

        internal const string DynamicSerializationNamespace = "Atlas.Xml.SerializationCompiler.Dynamic.";

        internal const string ObjectInstanceName = "objectInstance";

        private const BindingFlags MemberSearchFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        #endregion

        #region c# Compiler

        private static CSharpCodeProvider GetCompiler()
        {
            return new CSharpCodeProvider();
        }

        private static CompilerParameters GetCompilerParameters()
        {

            var compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                CompilerOptions = "/optimize",
            };

            var assemblies = AppDomain.CurrentDomain
                            .GetAssemblies()
                            .Where(p => !p.IsDynamic && !string.IsNullOrEmpty(p.Location))
                            .Select(p => p.Location);

            compilerParams.ReferencedAssemblies.AddRange(assemblies.ToArray());

            return compilerParams;
        }

        #endregion

        #region Class Name

        private static int _classNameIndex;

        private static string GenerateNewClassName()
        {
            return "C" + Interlocked.Increment(ref _classNameIndex);
        }

        #endregion

        #region Type Compilers

        private static string GenerateSerializerCode(Type type, string className)
        {
            if (type.IsArray)
                return GenerateArraySerializerCode(type, className);
            else if (type.GetInterface("IDictionary") != null)
                return GenerateDictionarySerializerCode(type, className);
            else if (type.GetInterface("IEnumerable") != null && type != typeof(string))
                return GenerateEnumerableSerializerCode(type, className);

            var typeInfo = new CompilerTypeInfo(type);

            if (typeInfo.SerializerConfigured || typeInfo.DeserializerConfigured)
                return GenerateValueTypeSerializerCode(type, className);
            else
                return GenerateStandartSerializerCode(type, className);
        }

        private static string GenerateDictionarySerializerCode(Type type, string className)
        {
            var classLevelCodePart = new StringBuilder();
            var propertyCompilers = GetPropertyCompilers(type, classLevelCodePart);

            var writerCode = GenerateWriterCode(propertyCompilers);
            var attributeReaderCode = GenerateAttributeReaderCasesCode(propertyCompilers);
            var elementReaderCode = GenerateElementReaderCasesCode(propertyCompilers);
            var textReaderCode = GenerateTextReaderCode(propertyCompilers);

            var compiler = new DictionaryCompiler(type, classLevelCodePart);
            var childReaderCode = GenerateChildReaderCode(compiler);
            var childWriterCode = GenerateChildWriterCode(compiler);
            var keyVariablesDeclarationCode = GenerateKeyVariablesDeclarationCode(compiler);

            var code = FixCode(Resources.CodeTemplateForEnumerable, classLevelCodePart, type, attributeReaderCode, elementReaderCode, textReaderCode)
                .Replace("{className}", className)
                .Replace("{typeName}", CachedTypeResolver.GetFriendlyNameForCode(type))
                .Replace("{writerCode}", writerCode)
                .Replace("{childWriterCode}", childWriterCode)
                .Replace("{elementReaderCode}", elementReaderCode)
                .Replace("{textReaderCode}", textReaderCode)
                .Replace("{classLevelCodePart}", classLevelCodePart.ToString())
                .Replace("{keyVariablesDeclaration}", keyVariablesDeclarationCode);

            if (string.IsNullOrEmpty(elementReaderCode))
                code = RemoveCodeBetweenComments(code, "Element Switch", childReaderCode);
            else
                code = RemoveCodeBetweenComments(code, "Default Element Switch", childReaderCode);

            return code;
        }

        private static string GenerateEnumerableSerializerCode(Type type, string className)
        {
            var classLevelCodePart = new StringBuilder();
            var propertyCompilers = GetPropertyCompilers(type, classLevelCodePart);

            var writerCode = GenerateWriterCode(propertyCompilers);
            var attributeReaderCode = GenerateAttributeReaderCasesCode(propertyCompilers);
            var elementReaderCode = GenerateElementReaderCasesCode(propertyCompilers);
            var textReaderCode = GenerateTextReaderCode(propertyCompilers);

            var compiler = new EnumerableCompiler(type, classLevelCodePart);
            var childReaderCode = GenerateChildReaderCode(compiler);
            var childWriterCode = GenerateChildWriterCode(compiler);

            var code = FixCode(Resources.CodeTemplateForEnumerable, classLevelCodePart, type, attributeReaderCode, elementReaderCode, textReaderCode)
                .Replace("{className}", className)
                .Replace("{typeName}", CachedTypeResolver.GetFriendlyNameForCode(type))
                .Replace("{childTypeName}", CachedTypeResolver.GetFriendlyNameForCode(compiler.ChildType))
                .Replace("{writerCode}", writerCode)
                .Replace("{childWriterCode}", childWriterCode)
                .Replace("{elementReaderCode}", elementReaderCode)
                .Replace("{textReaderCode}", textReaderCode)
                .Replace("{classLevelCodePart}", classLevelCodePart.ToString())
                .Replace("{keyVariablesDeclaration}", "");

            if (string.IsNullOrEmpty(elementReaderCode))
                code = RemoveCodeBetweenComments(code, "Element Switch", childReaderCode);
            else
                code = RemoveCodeBetweenComments(code, "Default Element Switch", childReaderCode);

            return code;
        }

        private static string GenerateArraySerializerCode(Type type, string className)
        {
            var classLevelCodePart = new StringBuilder();

            var compiler = new EnumerableCompiler(type, classLevelCodePart);

            var childReaderCode = GenerateChildReaderCode(compiler);
            var childWriterCode = GenerateChildWriterCode(compiler);

            var code = FixCode(Resources.CodeTemplateForArray, classLevelCodePart, type, null, null, null)
                .Replace("{className}", className)
                .Replace("{typeName}", CachedTypeResolver.GetFriendlyNameForCode(type))
                .Replace("{childTypeName}", CachedTypeResolver.GetFriendlyNameForCode(compiler.ChildType))
                .Replace("{childWriterCode}", childWriterCode)
                .Replace("{childReaderCode}", childReaderCode)
                .Replace("{classLevelCodePart}", classLevelCodePart.ToString());

            return code;
        }

        private static string GenerateValueTypeSerializerCode(Type type, string className)
        {
            var classLevelCodePart = new StringBuilder();
            var compiler = new ValueTypeCompiler(type, classLevelCodePart);

            var writerCode = GenerateWriterCode(compiler);
            var textReaderCode = GenerateTextReaderCode(compiler);
            var textReaderEmptyStringFixCode = GenerateTextReaderCodeEmptyStringFix(compiler);
            var nullCheckCode = GenerateNullCheckCode(compiler);

            var code = FixCode(Resources.CodeTemplateForValueType, classLevelCodePart, type, null, null, textReaderCode)
                .Replace("{className}", className)
                .Replace("{typeName}", CachedTypeResolver.GetFriendlyNameForCode(type))
                .Replace("{writerCode}", writerCode)
                .Replace("{classLevelCodePart}", classLevelCodePart.ToString())
                .Replace("{textReaderCode}", textReaderCode)
                .Replace("{emptyStringFixForTextReaderCode}", textReaderEmptyStringFixCode)
                .Replace("{nullCheckForNonNullables}", nullCheckCode);

            return code;
        }

        private static string GenerateStandartSerializerCode(Type type, string className)
        {
            var classLevelCodePart = new StringBuilder();
            var propertyCompilers = GetPropertyCompilers(type, classLevelCodePart);

            var writerCode = GenerateWriterCode(propertyCompilers);
            var attributeReaderCode = GenerateAttributeReaderCasesCode(propertyCompilers);
            var elementReaderCode = GenerateElementReaderCasesCode(propertyCompilers);
            var textReaderCode = GenerateTextReaderCode(propertyCompilers);
            var textReaderEmptyStringFixCode = GenerateTextReaderCodeEmptyStringFix(propertyCompilers);

            var code = FixCode(Resources.CodeTemplate, classLevelCodePart, type, attributeReaderCode, elementReaderCode, textReaderCode)
                .Replace("{className}", className)
                .Replace("{typeName}", CachedTypeResolver.GetFriendlyNameForCode(type))
                .Replace("{writerCode}", writerCode)
                .Replace("{classLevelCodePart}", classLevelCodePart.ToString())
                .Replace("{emptyStringFixForTextReaderCode}", textReaderEmptyStringFixCode);

            if (string.IsNullOrEmpty(elementReaderCode))
            {
                if (string.IsNullOrEmpty(textReaderCode))
                {
                    code = RemoveCodeBetweenComments(code, "Remove If No Text Or Elements", "reader.Skip();");
                }
                else
                {
                    code = RemoveCodeBetweenComments(code, "Remove If No Text Or Elements");
                    code = RemoveCodeBetweenComments(code, "Remove If No Elements", "reader.Skip();")
                        .Replace("{textReaderCode}", textReaderCode);
                }
            }
            else
            {
                code = RemoveCodeBetweenComments(code, "Remove If No Text Or Elements");
                code = RemoveCodeBetweenComments(code, "Remove If No Elements")
                        .Replace("{elementReaderCode}", elementReaderCode)
                        .Replace("{textReaderCode}", textReaderCode);
            }

            return code;
        }

        #endregion

        #region Helper Methods

        private static string FixCode(string code, StringBuilder classLevelCodePart, Type type, string attributeReaderCode, string elementReaderCode, string textReaderCode, bool isGetOnlyProperty = false)
        {

            #region Fix By Abstract

            string abstractTypeDeserializationException = string.Empty;
            string abstractUnspecifiedTypeDeserializationException = string.Empty;
            if (type.IsAbstract)
            {
                abstractTypeDeserializationException = "throw new XmlSerializationException(string.Format(\"Could not deserialize type: '{0}'! Declared type on xml element is abstract. ({1})\", typeof({typeName}), type));";
                abstractUnspecifiedTypeDeserializationException = "throw new XmlSerializationException(string.Format(\"Could not deserialize type: '{0}'! Xml element is missing it's type declaration (e.g.: first attribute with name: '" + Serializer.TypeAttributeName + "')\", typeof({typeName})));";
            }

            code = RemoveCodeBetweenComments(code, "Remove If Abstract", type.IsAbstract ? string.Empty : null)
                        .Replace("{abstractTypeDeserializationException}", abstractTypeDeserializationException)
                        .Replace("{abstractUnspecifiedTypeDeserializationException}", abstractUnspecifiedTypeDeserializationException);

            #endregion

            #region Fix By Sealed

            code = RemoveCodeBetweenComments(code, "Remove If Get Only Or Sealed", type.IsSealed || isGetOnlyProperty ? string.Empty : null);
            code = RemoveCodeBetweenComments(code, "Remove If Sealed", type.IsSealed ? string.Empty : null);

            #endregion

            #region Fix By attributeReaderCode

            if (string.IsNullOrEmpty(attributeReaderCode))
            {
                attributeReaderCode = type.IsSealed ? string.Empty : "reader.MoveToElement();";
                code = RemoveCodeBetweenComments(code, "Remove If No Attributes", string.Empty);
            }
            else
            {
                code = RemoveCodeBetweenComments(code, "Remove If No Attributes", null);
                code = code
                    .Replace("{attributeReaderCode}", attributeReaderCode);
            }

            #endregion

            #region Fix By overriden serialization options on type

            var options = XmlSerializationTypeAttribute.FromType(type).ToOptions();

            string serializationOptions = PropertyCompiler.ConfigureSerializerOptions(classLevelCodePart, "SerializationOptions.Default", options, null, "defaultSerializerOptions");
            code = code.Replace("{serializationOptionsCode}", serializationOptions);

            #endregion

            return code;
        }

        private static string RemoveCodeBetweenComments(string code, string commentedBlockName, string replaceBlockWith = null)
        {
            var startComment = "//BEGIN: " + commentedBlockName;
            var startIndex = code.IndexOf(startComment);
            while (startIndex >= 0)
            {
                var endComment = "//END: " + commentedBlockName;
                var endIndex = code.IndexOf(endComment, startIndex);

                if (replaceBlockWith == null) // Remove comments only
                {
                    var lfPos = code.IndexOf("\n", endIndex);
                    code = code.Remove(endIndex, lfPos + 1 - endIndex);

                    lfPos = code.IndexOf("\n", startIndex);
                    code = code.Remove(startIndex, lfPos + 1 - startIndex);
                }
                else // Replace comment block with new code
                {
                    code = code.Remove(startIndex, endIndex + endComment.Length - startIndex);
                    code = code.Insert(startIndex, replaceBlockWith);
                }

                startIndex = code.IndexOf(startComment, startIndex);
            }

            return code;
        }

        private static IEnumerable<PropertyCompiler> GetPropertyCompilers(Type type, StringBuilder classLevelCodePart)
        {
            var typeInfo = type.GetTypeInfo();

            var members = typeInfo.GetMembers(MemberSearchFlags)
                .Where(p => p.MemberType.HasFlag(MemberTypes.Property) || p.MemberType.HasFlag(MemberTypes.Field));

            var propertyCompilers = new List<PropertyCompiler>();
            int propertyIndex = 1;
            foreach (var member in members)
            {
                var propertyCompiler = new PropertyCompiler(member, propertyIndex, classLevelCodePart);
                if (propertyCompiler.SerializationMethod != SerializationNodeType.None)
                {
                    propertyCompilers.Add(propertyCompiler);
                    propertyIndex++;
                }
            }

            return propertyCompilers.OrderBy(p => p.SerializationTypeOrder).ThenBy(p => p.InheritanceDepth).ThenBy(p => p.Order);
        }

        #endregion

        #region Element Reader

        private static string GenerateElementReaderCasesCode(IEnumerable<PropertyCompiler> propertyCompilers)
        {
            var code = new StringBuilder();

            foreach (var compiler in propertyCompilers)
                compiler.AppendReadElementCase(code);

            return code.ToString();
        }

        private static string GenerateChildReaderCode(EnumerableCompiler compiler)
        {
            var code = new StringBuilder();
            compiler.AppendReadChildFromXml(code);
            return code.ToString();
        }

        private static string GenerateChildReaderCode(DictionaryCompiler compiler)
        {
            var code = new StringBuilder();
            compiler.AppendReadChildFromXml(code);
            return code.ToString();
        }

        private static string GenerateKeyVariablesDeclarationCode(DictionaryCompiler compiler)
        {
            var code = new StringBuilder();
            compiler.AppendKeyVariableDeclarations(code);
            return code.ToString();
        }

        #endregion

        #region Attribute Reader

        private static string GenerateAttributeReaderCasesCode(IEnumerable<PropertyCompiler> propertyCompilers)
        {
            var code = new StringBuilder();

            foreach (var compiler in propertyCompilers)
                compiler.AppendReadAttributeCase(code);

            return code.ToString();
        }

        #endregion

        #region Text Reader

        private static string GenerateTextReaderCode(IEnumerable<PropertyCompiler> propertyCompilers)
        {
            var code = new StringBuilder();

            var textProperty = propertyCompilers.FirstOrDefault(p => p.SerializationMethod == SerializationNodeType.Text || p.SerializationMethod == SerializationNodeType.TextAsCData);
            if (textProperty != null)
            {
                code.AppendLine("if(reader.NodeType == XmlNodeType.CDATA || reader.NodeType == XmlNodeType.Text)");
                textProperty.AppendReadText(code);
            }

            return code.ToString();
        }

        private static string GenerateTextReaderCodeEmptyStringFix(IEnumerable<PropertyCompiler> propertyCompilers)
        {
            var code = new StringBuilder();

            var textProperty = propertyCompilers.FirstOrDefault(p => p.SerializationMethod == SerializationNodeType.Text || p.SerializationMethod == SerializationNodeType.TextAsCData);
            if (textProperty != null)
                textProperty.AppendReadTextEmptyStringFix(code);

            return code.ToString();
        }

        private static string GenerateTextReaderCode(ValueTypeCompiler compiler)
        {
            var code = new StringBuilder();

            code.AppendLine("if(reader.NodeType == XmlNodeType.CDATA || reader.NodeType == XmlNodeType.Text)");
            compiler.AppendReadText(code);

            return code.ToString();
        }

        private static string GenerateTextReaderCodeEmptyStringFix(ValueTypeCompiler compiler)
        {
            var code = new StringBuilder();
            compiler.AppendReadTextEmptyStringFix(code);
            return code.ToString();
        }

        private static string GenerateNullCheckCode(ValueTypeCompiler compiler)
        {
            var code = new StringBuilder();
            compiler.AppendNullCheck(code);
            return code.ToString();
        }

        #endregion

        #region Writer

        private static string GenerateWriterCode(IEnumerable<PropertyCompiler> propertyCompilers)
        {
            var code = new StringBuilder();

            foreach (var compiler in propertyCompilers)
                compiler.AppendWriteToXml(code);

            return code.ToString();
        }

        private static string GenerateWriterCode(ValueTypeCompiler compiler)
        {
            var code = new StringBuilder();
            compiler.AppendWriteToXml(code);
            return code.ToString();
        }

        private static string GenerateChildWriterCode(EnumerableCompiler compiler)
        {
            var code = new StringBuilder();
            compiler.AppendWriteChildToXml(code);
            return code.ToString();
        }

        private static string GenerateChildWriterCode(DictionaryCompiler compiler)
        {
            var code = new StringBuilder();
            compiler.AppendWriteChildToXml(code);
            return code.ToString();
        }

        #endregion

        #region Static Methods

        #region String Conversion

        internal static void AppendToString(StringBuilder code, CompilerTypeInfo type, string getterFunction, string memberName)
        {
            if (type.ShouldUseXmlConvertOnGet)
            {
                // XmlConvert.ToString("Value")
                code.Append("XmlConvert.ToString(");
                code.Append(getterFunction);

                if (type.ValueType == typeof(DateTime))
                    code.Append(", XmlDateTimeSerializationMode.RoundtripKind");

                code.Append(")");
            }
            else if (type.ShouldUseTypeConverterOnSerialization)
            {
                // tc"MemberName".ConvertToInvariantString("Value");
                code.Append("tc");
                if (memberName != null)
                    code.Append(memberName);
                code.Append(".ConvertToInvariantString(");
                code.Append(getterFunction);
                code.Append(")");
            }
            else if (type.ValueType == typeof(string))
            {
                // "Value"
                code.Append(getterFunction);
            }
            else
            {
                // "Value".ToString()
                code.Append(getterFunction);
                code.Append(".ToString(");

                // "Value".ToString(CultureInfo.InvariantCulture)
                if (type.ShouldUseToStringWithCultureOnSerialization)
                    code.Append("System.Globalization.CultureInfo.InvariantCulture");

                code.Append(")");
            }
        }

        internal static void AppendParseString(StringBuilder code, CompilerTypeInfo type, string memberName, string valueTypeName, string getReaderValueCode = "reader.Value")
        {
            if (type.ShouldUseXmlConvertOnSet)
            {
                // XmlConvert.To"Type"(reader.Value)
                code.Append("XmlConvert.To");
                code.Append(type.ValueType.Name);
                code.Append("(");
                code.Append(getReaderValueCode);

                if (type.ValueType == typeof(DateTime))
                    code.Append(", XmlDateTimeSerializationMode.RoundtripKind");

                code.Append(")");
            }
            else if (type.ShouldUseTypeConverterOnDeserialization)
            {
                // ("Type")tc"MemberName".ConvertFromInvariantString(reader.Value)
                code.Append("(");
                code.Append(valueTypeName);
                code.Append(")");

                code.Append("tc");
                if (memberName != null)
                    code.Append(memberName);

                code.Append(".ConvertFromInvariantString(");
                code.Append(getReaderValueCode);
                code.Append(")");
            }
            else if (type.ValueType == typeof(string))
            {
                // reader.Value
                code.Append(getReaderValueCode);
            }
            else if (type.ShouldUseParseOnDeserialization)
            {
                // "Type".Parse(reader.Value)
                code.Append(valueTypeName);
                code.Append(".Parse(");
                code.Append(getReaderValueCode);

                if (type.ShouldUseParseOnDeserializationWithCulture)
                    code.Append(", System.Globalization.CultureInfo.InvariantCulture");

                code.Append(")");
            }
        }

        #endregion

        #region Writer Helpers

        internal static void AppendWriteCData(StringBuilder code, CompilerTypeInfo type, string getterFunction, string memberName)
        {
            // writer.WriteCData("ValueAsString");
            code.Append("writer.WriteCData(");
            AppendToString(code, type, getterFunction, memberName);
            code.AppendLine(");");
        }

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates dynamic serializer class from type for serialization of that type.
        /// </summary>
        /// <param name="type">Type to create serializer.</param>
        /// <returns>Created dynamic serializer of type</returns>
        public static IXmlSerializable Compile(Type type)
        {
            ArgumentValidation.NotNull(type, nameof(type));

            var className = GenerateNewClassName();
            var code = GenerateSerializerCode(type, className);

            var compiler = GetCompiler();
            var compileResult = compiler.CompileAssemblyFromSource(GetCompilerParameters(), code);
            if (compileResult.Errors.Count > 0)
                throw new XmlSerializationException(compileResult.Errors[0].ErrorText);

            var serializer = compileResult.CompiledAssembly.CreateInstance(DynamicSerializationNamespace + className);
            return serializer as IXmlSerializable;
        }

        #endregion

    }
}