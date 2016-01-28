using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Atlas.Xml.UnitTests
{
    [TestClass]
    public class ByteArrayTests
    {

        public ByteArrayTests()
        {
        }

        byte[] GetBytes(int size)
        {
            Random rnd = new Random();
            var bytes = new byte[size];
            rnd.NextBytes(bytes);
            return bytes;
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.ByteArray")]
        public void Base64()
        {
            for (int i = 0; i < 3; i++)
            {
                var a = GetBytes(16 * (i + 1) * 17);
                var serialized = Serializer.Serialize(a);

                var base64 = Convert.ToBase64String(a);

                Assert.AreEqual(serialized, @"<Array>" + base64 + "</Array>");

                var b = Serializer.Deserialize<byte[]>(serialized);
                Assert.IsTrue(Enumerable.SequenceEqual(a, b));
            }
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.ByteArray")]
        public void BinHex()
        {
            for (int i = 0; i < 3; i++)
            {
                var a = GetBytes(16 * (i + 1) * 17);
                var serialized = Serializer.Serialize(a, null, false, null, new SerializationOptions { ByteArraySerializationType = ByteArraySerializationType.BinHex });

                var binHex = a.ToHexString();
                Assert.AreEqual(serialized, @"<Array>" + binHex + "</Array>");

                var b = Serializer.Deserialize<byte[]>(serialized, new SerializationOptions { ByteArraySerializationType = ByteArraySerializationType.BinHex });
                Assert.IsTrue(Enumerable.SequenceEqual(a, b));
            }
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.ByteArray")]
        public void Elements()
        {
            for (int i = 0; i < 3; i++)
            {
                var a = GetBytes(16 * (i + 1) * 17);
                var serialized = Serializer.Serialize(a, null, false, null, new SerializationOptions { ChildElementName = "a", ByteArraySerializationType = ByteArraySerializationType.Elements });

                var elements = string.Join("", a.Select(p => "<a>" + p + "</a>").ToArray());
                Assert.AreEqual(serialized, @"<Array>" + elements + "</Array>");

                var b = Serializer.Deserialize<byte[]>(serialized, new SerializationOptions { ChildElementName = "a", ByteArraySerializationType = ByteArraySerializationType.Elements });
                Assert.IsTrue(Enumerable.SequenceEqual(a, b));
            }
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.ByteArray")]
        public void EmptyArray()
        {
            var a = new byte[] { };
            var serialized = Serializer.Serialize(a);

            Assert.AreEqual(serialized, @"<Array />");

            var b = Serializer.Deserialize<byte[]>(serialized);
            Assert.IsTrue(a.Length == 0 && b.Length == 0);
        }

    }
}