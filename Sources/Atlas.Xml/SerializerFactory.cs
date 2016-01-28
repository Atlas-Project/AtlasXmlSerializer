using Atlas.Xml.SerializationCompiler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Atlas.Xml
{

    /// <summary>
    /// Provides static methods to create generic xml serializer instances
    /// </summary>
    /// <typeparam name="T">Type to be serialized</typeparam>
    public static class SerializerFactory<T>
    {

        /// <summary>
        /// Holds current instance of specified type's serializer.
        /// </summary>
        public static IXmlSerializer<T> Instance { get; internal set; }

        static SerializerFactory()
        {
            Instance = SerializerFactory.GetSerializer(typeof(T)) as IXmlSerializer<T>;
        }

    }

    /// <summary>
    /// Provides static methods to create generic xml serializer instances
    /// </summary>
    public static class SerializerFactory
    {

        private static ConcurrentDictionary<Type, IXmlSerializable> _serializerCache = new ConcurrentDictionary<Type, IXmlSerializable>(new Dictionary<Type, IXmlSerializable>
        {
            { typeof(object), new ObjectSerializer() },
            { typeof(byte[]), new ByteArraySerializer() }
        });

        /// <summary>
        /// Gets dynamically created serializer for specified type
        /// </summary>
        /// <param name="type">Type to be serialized</param>
        /// <returns>Serializer for specified type</returns>
        /// <remarks>Serializer will be created on first use.</remarks>
        public static IXmlSerializable GetSerializer(Type type)
        {
            ArgumentValidation.NotNull(type, nameof(type));

            return _serializerCache.GetOrAdd(type, (key) => Compiler.Compile(type));
        }

        /// <summary>
        /// Registers/Overrides a serializer for type.
        /// </summary>
        /// <param name="type">Type to be serialized. If type already has a serializer, an exception will be thrown. Must not be null.</param>
        /// <param name="serializer">Serializer to be registered. Must not be null.</param>
        public static void Register(Type type, IXmlSerializable serializer)
        {
            ArgumentValidation.NotNull(type, nameof(type));
            ArgumentValidation.NotNull(serializer, nameof(serializer));
            _serializerCache.AddOrUpdate(type, serializer, (key, value) =>
            {
                // Update SerializerFactory<T> to hold new type as serializer
                var serializerInstanceHolder = Type.GetType("Atlas.Xml.SerializerFactory`1[" + type.GetNameForGetType() + "]");
                if (serializerInstanceHolder != null)
                {
                    var property = serializerInstanceHolder.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (property != null)
                        property.SetValue(null, serializer);
                }

                return value;
            });
        }

    }

}