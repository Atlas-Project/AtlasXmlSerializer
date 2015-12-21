using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Atlas.Xml.UnitTests
{
    [TestClass]
    public class DictionaryTests
    {

        public DictionaryTests()
        {
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Dictionary")]
        public void DictionaryOfIntString()
        {
            var a = new Dictionary<int, string> {
                { 1, "A" },
                { 2, "" },
                { 3, null },
            };

            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Dictionary><item k=""1"">A</item><item k=""2""></item><item _type=""null"" k=""3"" /></Dictionary>");

            var b = Serializer.Deserialize<Dictionary<int, string>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Dictionary")]
        public void DictionaryOfIntObjectValueType()
        {
            var a = new Dictionary<int, object> {
                { 1, "A" },
                { 2, "" },
                { 3, null },
                { 4, new DateTime(2014, 1, 1) },
                { 5, true },
            };

            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Dictionary><item k=""1""><v _type=""System.String"">A</v></item><item k=""2""><v _type=""System.String""></v></item><item k=""3"" /><item k=""4""><v _type=""System.DateTime"">2014-01-01T00:00:00</v></item><item k=""5""><v _type=""System.Boolean"">true</v></item></Dictionary>");

            var b = Serializer.Deserialize<Dictionary<int, object>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        public class IdName
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as IdName;
                if (other != null && other.Id == Id && other.Name == Name)
                    return true;

                return false;
            }

            public override int GetHashCode()
            {
                // Not important for tests
                return base.GetHashCode();
            }

        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Dictionary")]
        public void DictionaryOfIntObject()
        {
            var a = new Dictionary<int, object> {
                { 1, "A" },
                { 2, "" },
                { 3, null },
                { 4, new IdName { Id = 4, Name = "Four" } },
                { 5, new IdName { Id = 5, Name = "Five" } },
                { 6, new IdName { } },
                { 7, null },
            };

            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Dictionary><item k=""1""><v _type=""System.String"">A</v></item><item k=""2""><v _type=""System.String""></v></item><item k=""3"" />"
                + @"<item k=""4""><v _type=""Atlas.Xml.UnitTests.DictionaryTests+IdName, Atlas.Xml.UnitTests"" Id=""4"" Name=""Four"" /></item><item k=""5"">"
                + @"<v _type=""Atlas.Xml.UnitTests.DictionaryTests+IdName, Atlas.Xml.UnitTests"" Id=""5"" Name=""Five"" /></item><item k=""6"">"
                + @"<v _type=""Atlas.Xml.UnitTests.DictionaryTests+IdName, Atlas.Xml.UnitTests"" Id=""0"" /></item><item k=""7"" /></Dictionary>");

            var b = Serializer.Deserialize<Dictionary<int, object>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

        [TestMethod]
        [TestCategory("Xml.Serialization.Dictionary")]
        public void DictionaryOfObjectObject()
        {
            var a = new Dictionary<object, object> {
                { 1, "A" },
                { new DateTime(2014,1,1), "" },
                { Color.Red, null },
                { "", new IdName { Id = 4, Name = "Four" } },
                { 2, new IdName { Id = 5, Name = "Five" } },
                { 3, new IdName { } },
                { 4, null },
            };

            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Dictionary><item><k _type=""System.Int32"">1</k><v _type=""System.String"">A</v></item><item><k _type=""System.DateTime"">2014-01-01T00:00:00</k>"
                + @"<v _type=""System.String""></v></item><item><k _type=""System.Drawing.Color, System.Drawing"">Red</k></item><item><k _type=""System.String""></k>"
                + @"<v _type=""Atlas.Xml.UnitTests.DictionaryTests+IdName, Atlas.Xml.UnitTests"" Id=""4"" Name=""Four"" /></item><item><k _type=""System.Int32"">2</k>"
                + @"<v _type=""Atlas.Xml.UnitTests.DictionaryTests+IdName, Atlas.Xml.UnitTests"" Id=""5"" Name=""Five"" /></item><item><k _type=""System.Int32"">3</k>"
                + @"<v _type=""Atlas.Xml.UnitTests.DictionaryTests+IdName, Atlas.Xml.UnitTests"" Id=""0"" /></item><item><k _type=""System.Int32"">4</k></item></Dictionary>");

            var b = Serializer.Deserialize<Dictionary<object, object>>(serialized);
            Assert.IsTrue(Enumerable.SequenceEqual(a, b));
        }

    }
}