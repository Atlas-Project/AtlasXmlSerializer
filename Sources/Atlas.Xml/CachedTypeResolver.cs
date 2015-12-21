using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.IO;

namespace Atlas
{
    /// <summary>
    /// Provides static cached methods for resolving types
    /// </summary>
    public static class CachedTypeResolver
    {

        private static ConcurrentDictionary<Type, string> _typeFriendlynamesForCode = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Gets code friendly name for type from cache. e.g.: Instead of List`1[Int32], this method will return List<Int32>
        /// </summary>
        /// <param name="type">Type to get code representation</param>
        /// <returns>Type's code representation</returns>
        public static string GetFriendlyNameForCode(this Type type)
        {
            return _typeFriendlynamesForCode.GetOrAdd(type, (key) => key.GetFriendlyNameForCodeNoCache());
        }

        /// <summary>
        /// Gets code friendly name for type without caching it. e.g.: Instead of List`1[Int32], this method will return List<Int32>
        /// </summary>
        /// <param name="type">Type to get code representation</param>
        /// <returns>Type's code representation</returns>
        public static string GetFriendlyNameForCodeNoCache(this Type type)
        {
            ArgumentValidation.NotNull(type, nameof(type));

            var codeDomProvider = CodeDomProvider.CreateProvider("C#");
            var typeReferenceExpression = new CodeTypeReferenceExpression(new CodeTypeReference(type));
            using (var writer = new StringWriter())
            {
                codeDomProvider.GenerateCodeFromExpression(typeReferenceExpression, writer, new CodeGeneratorOptions());
                return writer.GetStringBuilder().ToString();
            }
        }

        private static ConcurrentDictionary<string, Type> _typesFromString = new ConcurrentDictionary<string, Type>();

        /// <summary>
        /// Gets type from string respresentation. Type will be cached.
        /// </summary>
        /// <param name="type">Type name to be searched</param>
        /// <returns>Found type</returns>
        public static Type GetType(string type)
        {
            return _typesFromString.GetOrAdd(type, (key) => GetTypeNoCache(key));
        }

        /// <summary>
        /// Gets type from string representation without caching it.
        /// </summary>
        /// <param name="type">Type name to be searched</param>
        /// <returns>Found type</returns>
        public static Type GetTypeNoCache(string type)
        {
            var result = Type.GetType(type, false, false);

            if (result != null)
                return result;

            int lastComma = type.LastIndexOf(",");
            type = type.Substring(0, lastComma);

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                result = asm.GetType(type, false, false);
                if (result != null)
                    return result;
            }

            return Type.GetType(type, true, false); ;
        }

        /// <summary>
        /// Gets name for <c>Type</c>'s GetType method. Similar to <c>Type</c>'s ToString method, but adds assembly name if required without version information.
        /// </summary>
        /// <param name="type">Type to get name</param>
        /// <returns>Name of type</returns>
        public static string GetNameForGetType(this Type type)
        {
            var name = type.AssemblyQualifiedName.Replace(", mscorlib", "");

            var versionIndex = name.IndexOf(", Version=");
            while (versionIndex > 0)
            {
                var endIndex = name.IndexOf("]", versionIndex + 1);
                if (endIndex > 0)
                {
                    name = name.Substring(0, versionIndex) + name.Substring(endIndex);
                    versionIndex = name.IndexOf(", Version=", versionIndex + 1);
                }
                else
                {
                    name = name.Substring(0, versionIndex);
                    break;
                }
            }

            return name;
        }

        /// <summary>
        /// Gets type's namespace and name joined by '.' character. Name will be cleared from generic declaration characters.
        /// </summary>
        /// <param name="type">Type to get name of</param>
        /// <returns>Type's namespace and name without generic declaration characters</returns>
        public static string GetNonGenericNameWithNamespace(this Type type)
        {
            return type.Namespace + "." + type.GetNonGenericName();
        }

        /// <summary>
        /// Returns type's name without generic declaration characters
        /// </summary>
        /// <param name="type">Type to get name of</param>
        /// <returns>Type's name without generic declaration characters</returns>
        public static string GetNonGenericName(this Type type)
        {
            return type.IsArray ? "Array" : type.Name.Split('`')[0];
        }

    }
}