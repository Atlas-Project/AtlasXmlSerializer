using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Atlas
{
    /// <summary>
    /// Defines static methods for argument validation.
    /// </summary>
    public static class ArgumentValidation
    {
        /// <summary>
        /// Checks argument and throws exception if argument is null.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">The argument</param>
        /// <param name="argumentName">Name of argument to be included in excetion message</param>
        /// <exception cref="ArgumentNullException"></exception>
        [DebuggerStepThrough]
        public static void NotNull<T>(T argument, string argumentName) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        /// <summary>
        /// Checks nullable argument and throws exception if argument is null or has default value.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">The argument</param>
        /// <param name="argumentName">Name of argument to be included in excetion message</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [DebuggerStepThrough]
        public static void NotEmpty<T>(T? argument, string argumentName) where T : struct
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);

            if (Nullable.Equals(argument, default(T)))
                throw new ArgumentException(Resources.ArgumentMustNotBeEmptyMessage, argumentName);
        }

        /// <summary>
        /// Checks struct argument and throws exception if argument has default value.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">The argument</param>
        /// <param name="argumentName">Name of argument to be included in excetion message</param>
        /// <exception cref="ArgumentException"></exception>
        [DebuggerStepThrough]
        public static void NotEmpty<T>(T argument, string argumentName) where T : struct
        {
            if (Equals(argument, default(T)))
                throw new ArgumentException(Resources.ArgumentMustNotBeEmptyMessage, argumentName);
        }

        /// <summary>
        /// Checks string argument and throws exception if argument is null or empty.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">The argument</param>
        /// <param name="argumentName">Name of argument to be included in excetion message</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>        
        [DebuggerStepThrough]
        public static void NotEmpty(string argument, string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);

            if (argument.Length == 0)
                throw new ArgumentException(Resources.ArgumentMustNotBeEmptyMessage, argumentName);
        }

        /// <summary>
        /// Checks IEnumerable argument and throws exception if argument has no itmes.
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <param name="argument">The argument</param>
        /// <param name="argumentName">Name of argument to be included in excetion message</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>        
        [DebuggerStepThrough]
        public static void NotEmpty<T>(IEnumerable<T> argument, string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);

            if (!argument.Any())
                throw new ArgumentException(Resources.ArgumentMustNotBeEmptyMessage, argumentName);
        }

        /// <summary>
        /// Throws exception if condition is false.
        /// </summary>
        /// <param name="condition">The condition</param>
        /// <param name="message">The message with formatting. If message is null, default message will be used.</param>
        /// <param name="formatParameters">Format parameters for message</param>
        [DebuggerStepThrough]
        public static void Ensure(bool condition, string message = null, params object[] formatParameters)
        {
            if (!condition)
            {
                if (formatParameters == null || message == null)
                    throw new ArgumentException(message ?? Resources.ArgumentValidationFailedMessage);
                else
                    throw new ArgumentException(string.Format(message ?? Resources.ArgumentValidationFailedMessage, formatParameters));
            }
        }

    }
}