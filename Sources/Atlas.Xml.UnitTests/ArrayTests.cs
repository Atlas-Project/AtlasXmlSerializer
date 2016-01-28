using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace Atlas.Xml.UnitTests
{
    [TestClass]
    public class ArrayTests
    {

        public ArrayTests()
        {
            //Atlas.Xml.SerializerFactory.Register(typeof(Basic2_NameToText), new CX());
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Array")]
        public void ArrayOfInt()
        {
            var a = new int[] { 1, 2, 3 };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Array><item>1</item><item>2</item><item>3</item></Array>");

            var b = Serializer.Deserialize<List<int>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Array")]
        public void ArrayOfString()
        {
            var a = new string[] { "A", "", null };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Array><item>A</item><item></item><item _type=""null"" /></Array>");

            var b = Serializer.Deserialize<string[]>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        public class Basic2
        {

            public int Id { get; set; }
            public string Name { get; set; }

            public override bool Equals(object other)
            {
                var b = other as Basic2;
                if (b != null)
                {
                    if (b.Id == Id && b.Name == Name)
                        return true;
                }

                return false;
            }

            public override int GetHashCode()
            {
                // Not important for tests
                return base.GetHashCode();
            }

        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Array")]
        public void ArrayOfClass()
        {
            var a = new Basic2[] {
                new Basic2 {Id=1, Name="One" },
                new Basic2 {Id=2, Name="Two" },
                null,
                new Basic2 {Id=3, Name="Three" },
            };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Array><item Id=""1"" Name=""One"" /><item Id=""2"" Name=""Two"" /><item _type=""null"" /><item Id=""3"" Name=""Three"" /></Array>");

            var b = Serializer.Deserialize<List<Basic2>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        public class Basic2_NameToText
        {

            public int Id { get; set; }
            [XmlSerializationMember(NodeType = SerializationNodeType.Text)]
            public string Name { get; set; }

            public override bool Equals(object other)
            {
                var b = other as Basic2_NameToText;
                if (b != null)
                {
                    if (b.Id == Id && b.Name == Name)
                        return true;
                }

                return false;
            }

            public override int GetHashCode()
            {
                // Not important for tests
                return base.GetHashCode();
            }

        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Array")]
        public void ArrayOfClassToText()
        {
            var a = new Basic2_NameToText[] {
                new Basic2_NameToText{Id=1, Name="One" },
                new Basic2_NameToText{Id=2, Name="Two" },
                null,
                new Basic2_NameToText{Id=3, Name="Three" },
            };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Array><item Id=""1"">One</item><item Id=""2"">Two</item><item _type=""null"" /><item Id=""3"">Three</item></Array>");

            var b = Serializer.Deserialize<List<Basic2_NameToText>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Array")]
        public void EnumerableOfClassArray()
        {
            IEnumerable<Basic2_NameToText> a = new Basic2_NameToText[] {
                new Basic2_NameToText{Id=1, Name="One" },
                new Basic2_NameToText{Id=2, Name="Two" },
                null,
                new Basic2_NameToText{Id=3, Name="Three" },
            };

            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<IEnumerable _type=""Atlas.Xml.UnitTests.ArrayTests+Basic2_NameToText[], Atlas.Xml.UnitTests""><item Id=""1"">One</item><item Id=""2"">Two</item><item _type=""null"" /><item Id=""3"">Three</item></IEnumerable>");

            var b = Serializer.Deserialize<IEnumerable<Basic2_NameToText>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));

            IEnumerable<object> c = new Basic2_NameToText[] {
                new Basic2_NameToText{Id=1, Name="One" },
                new Basic2_NameToText{Id=2, Name="Two" },
                null,
                new Basic2_NameToText{Id=3, Name="Three" },
            };
            serialized = Serializer.Serialize(c);
            Assert.AreEqual(serialized, @"<IEnumerable _type=""Atlas.Xml.UnitTests.ArrayTests+Basic2_NameToText[], Atlas.Xml.UnitTests""><item Id=""1"">One</item><item Id=""2"">Two</item><item _type=""null"" /><item Id=""3"">Three</item></IEnumerable>");

            var d = Serializer.Deserialize<IEnumerable<object>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(c, d));

            IEnumerable<object> e = new object[] {
                new Basic2_NameToText{Id=1, Name="One" },
                new Basic2_NameToText{Id=2, Name="" },
                null,
                new Basic2_NameToText{Id=3, Name=null },
            };
            serialized = Serializer.Serialize(e);
            Assert.AreEqual(serialized, @"<IEnumerable _type=""System.Object[]""><item _type=""Atlas.Xml.UnitTests.ArrayTests+Basic2_NameToText, Atlas.Xml.UnitTests"" Id=""1"">One</item>"
                                        + @"<item _type=""Atlas.Xml.UnitTests.ArrayTests+Basic2_NameToText, Atlas.Xml.UnitTests"" Id=""2""></item><item _type=""null"" /><item _type=""Atlas.Xml.UnitTests.ArrayTests+Basic2_NameToText, Atlas.Xml.UnitTests"" Id=""3"" /></IEnumerable>");

            var f = Serializer.Deserialize<IEnumerable<object>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(e, f));
        }

    }
}