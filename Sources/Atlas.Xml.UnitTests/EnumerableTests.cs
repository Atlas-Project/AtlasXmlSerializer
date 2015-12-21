using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Xml.UnitTests
{
    [TestClass]
    public class EnumerableTests
    {

        [TestMethod]
        [TestCategory("Xml.Serialization.Enumerable")]
        public void ListOfInt()
        {
            var a = new List<int> { 1, 2, 3 };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<List><item>1</item><item>2</item><item>3</item></List>");

            var b = Serializer.Deserialize<List<int>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Enumerable")]
        public void ListOfString()
        {
            var a = new List<string> { "A", "", null };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<List><item>A</item><item></item><item _type=""null"" /></List>");

            var b = Serializer.Deserialize<List<string>>(serialized);
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
        [TestCategory("Xml.Serialization.Enumerable")]
        public void ListOfClass()
        {
            var a = new List<Basic2> {
                new Basic2 {Id=1, Name="One" },
                new Basic2 {Id=2, Name="Two" },
                null,
                new Basic2 {Id=3, Name="Three" },
            };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<List><item Id=""1"" Name=""One"" /><item Id=""2"" Name=""Two"" /><item _type=""null"" /><item Id=""3"" Name=""Three"" /></List>");

            var b = Serializer.Deserialize<List<Basic2>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        [XmlSerializationType(ChildValueNodeType = SerializationNodeType.Text)]
        public class Basic2_Collection1 : List<int>
        {
        }

        [XmlSerializationType(ChildElementName = "x", ChildValueNodeType = SerializationNodeType.Element, ChildValueNodeName = "y")]
        public class Basic2_Collection2 : Basic2_Collection1
        {
        }

        [XmlSerializationType(ChildElementName = "a", ChildValueNodeType = SerializationNodeType.Attribute, ChildValueNodeName = "b")]
        public class Basic2_Collection3 : Basic2_Collection2
        {
        }

        [XmlSerializationType(ChildElementName = "a", ChildValueNodeType = SerializationNodeType.TextAsCData)]
        public class Basic2_Collection4 : List<int>
        {
        }

        [XmlSerializationType(ChildElementName = "a", ChildValueNodeType = SerializationNodeType.ElementAsCData)]
        public class Basic2_Collection5 : List<int>
        {
        }

        [XmlSerializationType(ChildElementName = "a", ChildValueNodeType = SerializationNodeType.Auto)]
        public class Basic2_Collection6 : List<int>
        {
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Enumerable")]
        public void ListOfIntWithDifferentTypes()
        {
            // Text under child: item
            var a = new Basic2_Collection1 { 1, 2, 3 };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Basic2_Collection1><item>1</item><item>2</item><item>3</item></Basic2_Collection1>");

            var b = Serializer.Deserialize<Basic2_Collection1>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));

            // Element y under child: x
            var c = new Basic2_Collection2 { 1, 2, 3 };
            serialized = Serializer.Serialize(c);
            Assert.AreEqual(serialized, @"<Basic2_Collection2><x><y>1</y></x><x><y>2</y></x><x><y>3</y></x></Basic2_Collection2>");

            var d = Serializer.Deserialize<Basic2_Collection2>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(c, d));

            // Attribute b in child: a
            var e = new Basic2_Collection3 { 1, 2, 3 };
            serialized = Serializer.Serialize(e);
            Assert.AreEqual(serialized, @"<Basic2_Collection3><a b=""1"" /><a b=""2"" /><a b=""3"" /></Basic2_Collection3>");

            var f = Serializer.Deserialize<Basic2_Collection3>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(e, f));

            // CData under child: a
            var g = new Basic2_Collection4 { 1, 2, 3 };
            serialized = Serializer.Serialize(g);
            Assert.AreEqual(serialized, @"<Basic2_Collection4><a><![CDATA[1]]></a><a><![CDATA[2]]></a><a><![CDATA[3]]></a></Basic2_Collection4>");

            var h = Serializer.Deserialize<Basic2_Collection4>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(g, h));

            // CData Element v under child: a
            var i = new Basic2_Collection5 { 1, 2, 3 };
            serialized = Serializer.Serialize(i);
            Assert.AreEqual(serialized, @"<Basic2_Collection5><a><v><![CDATA[1]]></v></a><a><v><![CDATA[2]]></v></a><a><v><![CDATA[3]]></v></a></Basic2_Collection5>");

            var j = Serializer.Deserialize<Basic2_Collection5>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(i, j));

            // Auto: Text under child: a
            var k = new Basic2_Collection6 { 1, 2, 3 };
            serialized = Serializer.Serialize(k);
            Assert.AreEqual(serialized, @"<Basic2_Collection6><a>1</a><a>2</a><a>3</a></Basic2_Collection6>");

            var l = Serializer.Deserialize<Basic2_Collection6>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(k, l));
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
        [TestCategory("Xml.Serialization.Enumerable")]
        public void ListOfClassToText()
        {
            var a = new List<Basic2_NameToText> {
                new Basic2_NameToText{Id=1, Name="One" },
                new Basic2_NameToText{Id=2, Name="Two" },
                null,
                new Basic2_NameToText{Id=3, Name="Three" },
            };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<List><item Id=""1"">One</item><item Id=""2"">Two</item><item _type=""null"" /><item Id=""3"">Three</item></List>");

            var b = Serializer.Deserialize<List<Basic2_NameToText>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Enumerable")]
        public void EnumerableOfClass()
        {
            IEnumerable<Basic2_NameToText> a = new List<Basic2_NameToText> {
                new Basic2_NameToText{Id=1, Name="One" },
                new Basic2_NameToText{Id=2, Name="Two" },
                null,
                new Basic2_NameToText{Id=3, Name="Three" },
            };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<IEnumerable _type=""System.Collections.Generic.List`1[[Atlas.Xml.UnitTests.EnumerableTests+Basic2_NameToText, Atlas.Xml.UnitTests]]""><item Id=""1"">One</item><item Id=""2"">Two</item><item _type=""null"" /><item Id=""3"">Three</item></IEnumerable>");

            var b = Serializer.Deserialize<IEnumerable<Basic2_NameToText>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));

            IEnumerable<object> c = new List<Basic2_NameToText> {
                new Basic2_NameToText{Id=1, Name="One" },
                new Basic2_NameToText{Id=2, Name="Two" },
                null,
                new Basic2_NameToText{Id=3, Name="Three" },
            };
            serialized = Serializer.Serialize(c);
            Assert.AreEqual(serialized, @"<IEnumerable _type=""System.Collections.Generic.List`1[[Atlas.Xml.UnitTests.EnumerableTests+Basic2_NameToText, Atlas.Xml.UnitTests]]""><item Id=""1"">One</item><item Id=""2"">Two</item><item _type=""null"" /><item Id=""3"">Three</item></IEnumerable>");

            var d = Serializer.Deserialize<IEnumerable<object>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(c, d));

            IEnumerable<object> e = new List<object> {
                new Basic2_NameToText{Id=1, Name="One" },
                new Basic2_NameToText{Id=2, Name="Two" },
                null,
                new Basic2_NameToText{Id=3, Name="Three" },
            };
            serialized = Serializer.Serialize(e);
            Assert.AreEqual(serialized, @"<IEnumerable _type=""System.Collections.Generic.List`1[[System.Object]]""><item _type=""Atlas.Xml.UnitTests.EnumerableTests+Basic2_NameToText, Atlas.Xml.UnitTests"" Id=""1"">One</item>"
                                        + @"<item _type=""Atlas.Xml.UnitTests.EnumerableTests+Basic2_NameToText, Atlas.Xml.UnitTests"" Id=""2"">Two</item><item _type=""null"" /><item _type=""Atlas.Xml.UnitTests.EnumerableTests+Basic2_NameToText, Atlas.Xml.UnitTests"" Id=""3"">Three</item></IEnumerable>");

            var f = Serializer.Deserialize<IEnumerable<object>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(e, f));
        }

    }
}