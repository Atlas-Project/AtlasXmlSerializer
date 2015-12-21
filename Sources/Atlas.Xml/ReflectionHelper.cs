using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Atlas
{
    /// <summary>
    /// Defines static helper methods for reflection
    /// </summary>
    public static class ReflectionHelper
    {

        #region Constants

        /// <summary>
        /// Default binging for dynamic type member discovery
        /// </summary>
        public const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        #endregion

        #region Getter / Setter Helpers

        /// <summary>
        /// Creates a getter function for Type's member
        /// </summary>
        /// <typeparam name="TType">Type containing the member</typeparam>
        /// <typeparam name="TMemberType">Type of member</typeparam>
        /// <param name="memberName">Name of member</param>
        /// <param name="bindingFlags">Binding flags for member search</param>
        /// <returns>Function to get value of member</returns>
        public static Func<TType, TMemberType> CreateGetter<TType, TMemberType>(string memberName, BindingFlags bindingFlags = DefaultBindingFlags)
        {
            ArgumentValidation.NotEmpty(memberName, nameof(memberName));

            var member = typeof(TType).GetMember(memberName, bindingFlags).FirstOrDefault();

            if (member != null)
            {
                var memberAsProperty = member as PropertyInfo;
                if (memberAsProperty != null)
                    return CreateGetterPrivate<TType, TMemberType>(memberAsProperty);

                var memberAsField = member as FieldInfo;
                if (memberAsField != null)
                    return CreateGetterPrivate<TType, TMemberType>(memberAsField);
            }

            return null;
        }

        /// <summary>
        /// Creates a getter function for Type's property
        /// </summary>
        /// <typeparam name="TType">Type containing the property</typeparam>
        /// <typeparam name="TMemberType">Type of property</typeparam>
        /// <param name="property">Property of type</param>
        /// <returns>Function to get value of property</returns>
        public static Func<TType, TMemberType> CreateGetter<TType, TMemberType>(this PropertyInfo property)
        {
            ArgumentValidation.NotNull(property, nameof(property));

            return CreateGetterPrivate<TType, TMemberType>(property);
        }

        private static Func<TType, TValue> CreateGetterPrivate<TType, TValue>(PropertyInfo property)
        {
            if (property.GetMethod != null)
            {
                if (!property.GetMethod.IsStatic)
                    return (Func<TType, TValue>)Delegate.CreateDelegate(typeof(Func<TType, TValue>), property.GetMethod);
                else
                    return WrapStaticGetter<TType, TValue>((Func<TValue>)Delegate.CreateDelegate(typeof(Func<TValue>), property.GetMethod));
            }

            return null;
        }

        /// <summary>
        /// Creates a getter function for Type's field
        /// </summary>
        /// <typeparam name="TType">Type containing the field</typeparam>
        /// <typeparam name="TMemberType">Type of field</typeparam>
        /// <param name="property">Field of type</param>
        /// <returns>Function to get value of field</returns>        
        public static Func<TType, TMemberType> CreateGetter<TType, TMemberType>(this FieldInfo field)
        {
            ArgumentValidation.NotNull(field, nameof(field));

            return CreateGetterPrivate<TType, TMemberType>(field);
        }

        private static Func<TType, TMemberType> CreateGetterPrivate<TType, TMemberType>(FieldInfo field)
        {
            if (field.IsLiteral)
            {
                TMemberType value = (TMemberType)field.GetRawConstantValue();
                return (t) => value;
            }
            else
            {
                var method = new DynamicMethod(string.Empty, typeof(TMemberType), new[] { typeof(TType) }, true);

                var ilGenerator = method.GetILGenerator();

                if (field.IsStatic)
                {
                    ilGenerator.Emit(OpCodes.Ldsfld, field);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldfld, field);
                }

                ilGenerator.Emit(OpCodes.Ret);

                return (Func<TType, TMemberType>)method.CreateDelegate(typeof(Func<TType, TMemberType>));
            }
        }

        /// <summary>
        /// Creates a setter function for Type's member
        /// </summary>
        /// <typeparam name="TType">Type containing the member</typeparam>
        /// <typeparam name="TMemberType">Type of member</typeparam>
        /// <param name="memberName">Name of member</param>
        /// <param name="bindingFlags">Binding flags for member search</param>
        /// <returns>Action to set value of member</returns>
        public static Action<TType, TMemberType> CreateSetter<TType, TMemberType>(string memberName, BindingFlags bindingFlags = DefaultBindingFlags)
        {
            ArgumentValidation.NotEmpty(memberName, nameof(memberName));

            var member = typeof(TType).GetMember(memberName, bindingFlags).FirstOrDefault();

            if (member != null)
            {
                var memberAsProperty = member as PropertyInfo;
                if (memberAsProperty != null)
                    return CreateSetterPrivate<TType, TMemberType>(memberAsProperty);

                var memberAsField = member as FieldInfo;
                if (memberAsField != null)
                    return CreateSetterPrivate<TType, TMemberType>(memberAsField);
            }

            return null;
        }

        /// <summary>
        /// Creates a setter function for Type's property
        /// </summary>
        /// <typeparam name="TType">Type containing the property</typeparam>
        /// <typeparam name="TMemberType">Type of property</typeparam>
        /// <param name="property">Property of type</param>
        /// <returns>Action to set value of property</returns>
        public static Action<TType, TMemberType> CreateSetter<TType, TMemberType>(this PropertyInfo property)
        {
            ArgumentValidation.NotNull(property, nameof(property));

            return CreateSetterPrivate<TType, TMemberType>(property);
        }

        private static Action<TType, TMemberType> CreateSetterPrivate<TType, TMemberType>(PropertyInfo property)
        {
            if (property.GetMethod != null)
            {
                if (!property.GetMethod.IsStatic)
                    return (Action<TType, TMemberType>)Delegate.CreateDelegate(typeof(Action<TType, TMemberType>), property.SetMethod);
                else
                    return WrapStaticSetter<TType, TMemberType>((Action<TMemberType>)Delegate.CreateDelegate(typeof(Action<TMemberType>), property.SetMethod));
            }

            return null;
        }

        /// <summary>
        /// Creates a setter function for Type's field
        /// </summary>
        /// <typeparam name="TType">Type containing the field</typeparam>
        /// <typeparam name="TMemberType">Type of field</typeparam>
        /// <param name="field">Field of type</param>
        /// <returns>Action to set value of field</returns>        
        public static Action<TType, TMemberType> CreateSetter<TType, TMemberType>(this FieldInfo field)
        {
            ArgumentValidation.NotNull(field, nameof(field));

            return CreateSetterPrivate<TType, TMemberType>(field);
        }

        private static Action<TType, TMemberType> CreateSetterPrivate<TType, TMemberType>(FieldInfo field)
        {
            var method = new DynamicMethod(string.Empty, null, new[] { typeof(TType), typeof(TMemberType) }, true);

            var ilGenerator = method.GetILGenerator();

            if (field.IsStatic)
            {
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Stfld, field);
            }

            ilGenerator.Emit(OpCodes.Ret);

            return (Action<TType, TMemberType>)method.CreateDelegate(typeof(Action<TType, TMemberType>));
        }

        private static Action<TType, TMemberType> WrapStaticSetter<TType, TMemberType>(Action<TMemberType> action)
        {
            return (t, m) => action(m);
        }

        private static Func<TType, TMemberType> WrapStaticGetter<TType, TMemberType>(Func<TMemberType> function)
        {
            return (t) => function();
        }

        /// <summary>
        /// Creates a getter function for Type's parameterless string method. (i.e. ToString())
        /// </summary>
        /// <typeparam name="TType">Type containing the method</typeparam>
        /// <param name="methodName">Name of method</param>
        /// <returns>Function to execute method and return string value</returns>              
        public static Func<TType, string> CreateGetStringMethod<TType>(string methodName)
        {
            ArgumentValidation.NotEmpty(methodName, nameof(methodName));

            var method = typeof(TType).GetMethod(methodName, DefaultBindingFlags, null, Type.EmptyTypes, null);
            if (method != null)
            {
                if (!method.IsStatic)
                    return (Func<TType, string>)Delegate.CreateDelegate(typeof(Func<TType, string>), method);
                else
                    return WrapStaticGetter<TType, string>((Func<string>)Delegate.CreateDelegate(typeof(Func<string>), method));
            }

            return null;
        }

        /// <summary>
        /// Creates a setter function for Type's method with single string parameter. (i.e. SetValue(string))
        /// </summary>
        /// <typeparam name="TType">Type containing the method</typeparam>
        /// <param name="methodName">Name of method</param>
        /// <returns>Action to execute method with string parameter</returns>   
        public static Action<TType, string> CreateSetStringMethod<TType>(string methodName)
        {
            ArgumentValidation.NotEmpty(methodName, nameof(methodName));

            var method = typeof(TType).GetMethod(methodName, DefaultBindingFlags, null, new[] { typeof(string) }, null);
            if (method != null)
            {
                if (!method.IsStatic)
                    return (Action<TType, string>)Delegate.CreateDelegate(typeof(Action<TType, string>), method);
                else
                    return WrapStaticSetter<TType, string>((Action<string>)Delegate.CreateDelegate(typeof(Action<string>), method));
            }

            return null;
        }

        #endregion

        #region Static Getter / Setter Helpers

        /// <summary>
        /// Creates a getter function for Type's static member
        /// </summary>
        /// <typeparam name="TType">Type containing the member</typeparam>
        /// <typeparam name="TMemberType">Type of member</typeparam>
        /// <param name="memberName">Name of member</param>
        /// <param name="bindingFlags">Binding flags for member search</param>
        /// <returns>Function to get value of static member</returns>
        public static Func<TMemberType> CreateStaticGetter<TType, TMemberType>(string memberName, BindingFlags bindingFlags = DefaultBindingFlags)
        {
            ArgumentValidation.NotEmpty(memberName, nameof(memberName));

            var member = typeof(TType).GetMember(memberName, bindingFlags).FirstOrDefault();

            if (member != null)
            {
                var memberAsProperty = member as PropertyInfo;
                if (memberAsProperty != null)
                    return CreateStaticGetterPrivate<TMemberType>(memberAsProperty);

                var memberAsField = member as FieldInfo;
                if (memberAsField != null)
                    return CreateStaticGetterPrivate<TMemberType>(memberAsField);
            }

            return null;
        }

        /// <summary>
        /// Creates a getter function for static property
        /// </summary>
        /// <typeparam name="TMemberType">Type of property</typeparam>
        /// <param name="property">The property</param>
        /// <returns>Function to get value of static property</returns>
        public static Func<TMemberType> CreateStaticGetter<TMemberType>(this PropertyInfo property)
        {
            ArgumentValidation.NotNull(property, nameof(property));

            return CreateStaticGetterPrivate<TMemberType>(property);
        }

        private static Func<TMemberType> CreateStaticGetterPrivate<TMemberType>(PropertyInfo property)
        {
            if (property.GetMethod != null && property.GetMethod.IsStatic)
                return (Func<TMemberType>)Delegate.CreateDelegate(typeof(Func<TMemberType>), property.GetMethod);

            return null;
        }

        /// <summary>
        /// Creates a getter function for Type's static field
        /// </summary>
        /// <typeparam name="TMemberType">Type of field</typeparam>
        /// <param name="property">Field of type</param>
        /// <returns>Function to get value of static field</returns>        
        public static Func<TMemberType> CreateStaticGetter<TMemberType>(this FieldInfo field)
        {
            ArgumentValidation.NotNull(field, nameof(field));

            return CreateStaticGetterPrivate<TMemberType>(field);
        }

        private static Func<TMemberType> CreateStaticGetterPrivate<TMemberType>(FieldInfo field)
        {
            if (field.IsStatic)
            {
                if (field.IsLiteral) // Constant
                {
                    TMemberType value = (TMemberType)field.GetRawConstantValue();
                    return () => value;
                }
                else // Static
                {
                    var method = new DynamicMethod(string.Empty, typeof(TMemberType), null, true);

                    var ilGenerator = method.GetILGenerator();
                    ilGenerator.Emit(OpCodes.Ldsfld, field);
                    ilGenerator.Emit(OpCodes.Ret);
                    return (Func<TMemberType>)method.CreateDelegate(typeof(Func<TMemberType>));
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a setter function for Type's static member
        /// </summary>
        /// <typeparam name="TType">Type containing the member</typeparam>
        /// <typeparam name="TMemberType">Type of member</typeparam>
        /// <param name="memberName">Name of member</param>
        /// <param name="bindingFlags">Binding flags for member search</param>
        /// <returns>Action to set value of static member</returns>
        public static Action<TMemberType> CreateStaticSetter<TType, TMemberType>(string memberName, BindingFlags bindingFlags = DefaultBindingFlags)
        {
            ArgumentValidation.NotNull(memberName, nameof(memberName));

            var member = typeof(TType).GetMember(memberName, bindingFlags).FirstOrDefault();

            if (member != null)
            {
                var memberAsProperty = member as PropertyInfo;
                if (memberAsProperty != null)
                    return CreateStaticSetterPrivate<TMemberType>(memberAsProperty);

                var memberAsField = member as FieldInfo;
                if (memberAsField != null)
                    return CreateStaticSetterPrivate<TMemberType>(memberAsField);
            }

            return null;
        }

        /// <summary>
        /// Creates a setter function for Type's static property
        /// </summary>
        /// <typeparam name="TMemberType">Type of property</typeparam>
        /// <param name="property">Property of type</param>
        /// <returns>Action to set value of static property</returns>
        public static Action<TMemberType> CreateStaticSetter<TMemberType>(this PropertyInfo property)
        {
            ArgumentValidation.NotNull(property, nameof(property));

            return CreateStaticSetterPrivate<TMemberType>(property);
        }

        private static Action<TMemberType> CreateStaticSetterPrivate<TMemberType>(PropertyInfo property)
        {
            if (property.SetMethod != null && property.SetMethod.IsStatic)
                return (Action<TMemberType>)Delegate.CreateDelegate(typeof(Action<TMemberType>), property.SetMethod);

            return null;
        }

        /// <summary>
        /// Creates a setter function for Type's static field
        /// </summary>
        /// <typeparam name="TMemberType">Type of field</typeparam>
        /// <param name="field">Field of type</param>
        /// <returns>Action to set value of static field</returns>       
        public static Action<TMemberType> CreateStaticSetter<TMemberType>(this FieldInfo field)
        {
            ArgumentValidation.NotNull(field, nameof(field));

            return CreateStaticSetterPrivate<TMemberType>(field);
        }

        private static Action<TMemberType> CreateStaticSetterPrivate<TMemberType>(FieldInfo field)
        {
            if (field.IsStatic)
            {
                var method = new DynamicMethod(string.Empty, null, new[] { typeof(TMemberType) }, true);

                var ilGenerator = method.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Stsfld, field);
                ilGenerator.Emit(OpCodes.Ret);

                return (Action<TMemberType>)method.CreateDelegate(typeof(Action<TMemberType>));
            }

            return null;
        }

        /// <summary>
        /// Creates a getter function for Type's static parameterless string method. (i.e. ToString())
        /// </summary>
        /// <typeparam name="TType">Type containing the method</typeparam>
        /// <param name="methodName">Name of method</param>
        /// <returns>Function to execute static method and return string value</returns>              
        public static Func<string> CreateStaticGetStringMethod<TType>(string methodName)
        {
            ArgumentValidation.NotEmpty(methodName, nameof(methodName));

            var method = typeof(TType).GetMethod(methodName, DefaultBindingFlags, null, Type.EmptyTypes, null);
            if (method != null && method.IsStatic)
                return (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), method);

            return null;
        }

        /// <summary>
        /// Creates a setter function for Type's static method with single string parameter. (i.e. Parse(string))
        /// </summary>
        /// <typeparam name="TType">Type containing the method</typeparam>
        /// <param name="methodName">Name of method</param>
        /// <returns>Action to execute static method with string parameter</returns>   
        public static Action<string> CreateStaticSetStringMethod<TType>(string methodName)
        {
            ArgumentValidation.NotEmpty(methodName, nameof(methodName));

            var method = typeof(TType).GetMethod(methodName, DefaultBindingFlags, null, new[] { typeof(string) }, null);
            if (method != null && method.IsStatic)
                return (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), method);

            return null;
        }

        #endregion

        #region Hierarchy

        /// <summary>
        /// Gets type's base types in hierarchical order
        /// </summary>
        /// <param name="type">Type to find base types</param>
        /// <param name="includeInterfaces">If true, includes interfaces in search</param>
        /// <returns>Found types, in hierarchical order. The type parameter will be the first element in list</returns>
        public static List<Type> GetTypeHierarchy(this Type type, bool includeInterfaces = true)
        {
            var result = new List<Type>();

            if (type != null)
            {
                var iteratingType = type;
                while (iteratingType != null && iteratingType != typeof(object))
                {
                    result.Add(iteratingType);
                    iteratingType = iteratingType.BaseType;
                }

                result.AddRange(type.GetInterfaces().Except(result));
            }

            return result;
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Gets attributes of T from all specified members.
        /// </summary>
        /// <typeparam name="T">Type of attribute to search</typeparam>
        /// <param name="members">Types to be searched</param>
        /// <returns>Found attributes in given member order</returns>
        public static List<T> GetAttributes<T>(this IEnumerable<Type> types) where T : Attribute
        {
            var result = new List<T>();

            foreach (var subType in types)
                result.AddRange(subType.GetCustomAttributes<T>(false).Except(result));

            return result;
        }

        #endregion

    }
}