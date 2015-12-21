using System;
using Atlas.Xml;
using System.Xml;

namespace Atlas.Xml.SerializationCompiler.Dynamic
{
    public class CX : IXmlSerializer<System.Collections.Generic.Dictionary<int, object>>
    {

        public void Serialize(XmlWriter writer, System.Collections.Generic.Dictionary<int, object> objectInstance, SerializationOptions options)
        {
            var realType = objectInstance.GetType();
            if (realType != typeof(System.Collections.Generic.Dictionary<int, object>))
            {
                writer.WriteAttributeString(Serializer.TypeAttributeName, realType.GetNameForGetType());
                SerializerFactory.GetSerializer(realType).WriteXml(writer, objectInstance, options);
                return;
            }

            foreach (var item in objectInstance)
            {
                writer.WriteStartElement(options.ChildElementName);
                writer.WriteAttributeString(options.KeyNodeName, XmlConvert.ToString(item.Key));
                if (item.Value != null)
                {
                    writer.WriteStartElement(options.ValueNodeName); SerializerFactory<object>.Instance.Serialize(writer, item.Value, SerializerFactory<object>.Instance.DefaultSerializationOptions);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        public System.Collections.Generic.Dictionary<int, object> Deserialize(XmlReader reader, SerializationOptions options)
        {
            if (reader.MoveToFirstAttribute() && reader.LocalName == Serializer.TypeAttributeName)
            {
                var type = CachedTypeResolver.GetType(reader.Value);
                if (type != typeof(System.Collections.Generic.Dictionary<int, object>))
                    return (System.Collections.Generic.Dictionary<int, object>)SerializerFactory.GetSerializer(type).ReadXml(reader, options);
            }

            var objectInstance = new System.Collections.Generic.Dictionary<int, object>();

            Deserialize(reader, objectInstance, options);

            return objectInstance;
        }

        public void Deserialize(XmlReader reader, System.Collections.Generic.Dictionary<int, object> objectInstance, SerializationOptions options)
        {
            var keyFound = false;
            var key = default(int);
            var valueFound = false;
            var value = default(object);

            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    while (reader.NodeType == XmlNodeType.Element)
                    {
                        keyFound = false;
                        valueFound = false;
                        if (reader.MoveToFirstAttribute())
                        {
                            do
                            {
                                if (reader.LocalName == options.KeyNodeName)
                                {
                                    keyFound = true;
                                    key = XmlConvert.ToInt32(reader.Value);
                                }
                            }
                            while (reader.MoveToNextAttribute());

                            reader.MoveToElement();
                        }

                        if (!reader.IsEmptyElement)
                        {
                            while (reader.Read())
                            {
                                while (reader.NodeType == XmlNodeType.Element)
                                {
                                    if (reader.LocalName == options.ValueNodeName)
                                    {
                                        valueFound = true;
                                        value = SerializerFactory<object>.Instance.Deserialize(reader, SerializerFactory<object>.Instance.DefaultSerializationOptions);
                                    }
                                    else
                                    {
                                        reader.Skip();
                                    }
                                }

                                if (reader.NodeType == XmlNodeType.EndElement)
                                    break;
                            }
                        }

                        reader.Read();

                        if (!keyFound)
                            throw new XmlSerializationException("Could not deserialize type: 'int'! Dictionary child element is missing it's key attribute.");

                        if (!valueFound)
                            value = null;

                        objectInstance[key] = value;
                    }

                    if (reader.NodeType == XmlNodeType.EndElement)
                        break;
                }
            }

            reader.Read();
        }

        public SerializationOptions DefaultSerializationOptions
        {
            get { return SerializationOptions.Default; }
        }

        public void WriteXml(XmlWriter writer, object objectInstance, SerializationOptions options)
        {
            Serialize(writer, (System.Collections.Generic.Dictionary<int, object>)objectInstance, options);
        }

        public object ReadXml(XmlReader reader, SerializationOptions options)
        {
            return Deserialize(reader, options);
        }

        public void ReadXml(XmlReader reader, object objectInstance, SerializationOptions options)
        {
            Deserialize(reader, (System.Collections.Generic.Dictionary<int, object>)objectInstance, options);
        }

    }
}