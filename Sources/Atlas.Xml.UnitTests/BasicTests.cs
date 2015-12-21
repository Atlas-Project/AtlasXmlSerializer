using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;

namespace Atlas.Xml.UnitTests
{
    [TestClass]
    public class BasicTests
    {

        public class Basic1
        {

            public Byte MemberByte { get; set; }
            public SByte MemberSByte { get; set; }

            public Int16 MemberInt16 { get; set; }
            public Int32 MemberInt32 { get; set; }
            public Int64 MemberInt64 { get; set; }
            public UInt16 MemberUInt16 { get; set; }
            public UInt32 MemberUInt32 { get; set; }
            public UInt64 MemberUInt64 { get; set; }

            public Decimal MemberDecimal { get; set; }
            public Single MemberSingle { get; set; }
            public Double MemberDouble { get; set; }

            public DateTime MemberDate { get; set; }

            public TimeSpan MemberTimeSpan { get; set; }

            public Guid MemberGuid { get; set; }

            public string MemberString { get; set; }

            public Rectangle MemberRectangle { get; set; }

            public static Basic1 GetSample()
            {
                return new Basic1
                {
                    MemberRectangle = new Rectangle(100, 100, 300, 300),
                    MemberByte = 50,
                    MemberSByte = -50,
                    MemberInt16 = -1000,
                    MemberInt32 = -20000000,
                    MemberInt64 = -1234567890123456,
                    MemberUInt16 = 1000,
                    MemberUInt32 = 20000000,
                    MemberUInt64 = 1234567890123456,
                    MemberDate = new DateTime(2010, 1, 1),
                    MemberDecimal = 100.1234567890m,
                    MemberDouble = (double)122.123,
                    MemberGuid = new Guid("{599CF792-722F-45E7-8C62-3774AF4E7F7B}"),
                    MemberSingle = (float)133.345,
                    MemberString = "Test String",
                    MemberTimeSpan = TimeSpan.FromMinutes(30),
                };
            }

            public override bool Equals(object other)
            {
                var b = other as Basic1;
                if (b != null)
                {
                    if (b.MemberByte == MemberByte && b.MemberDate == MemberDate && b.MemberDecimal == MemberDecimal && b.MemberDouble == MemberDouble && b.MemberGuid == MemberGuid
                        && b.MemberInt16 == MemberInt16 && b.MemberInt32 == MemberInt32 && b.MemberInt64 == MemberInt64 && b.MemberSByte == MemberSByte && b.MemberSingle == MemberSingle
                        && b.MemberString == MemberString && b.MemberTimeSpan == MemberTimeSpan && b.MemberUInt16 == MemberUInt16 && b.MemberUInt32 == MemberUInt32 && b.MemberUInt64 == MemberUInt64)
                    {
                        if (MemberRectangle == null)
                            return b.MemberRectangle == null;

                        if (MemberRectangle.Equals(b.MemberRectangle))
                            return true;
                    }
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
        [TestCategory("Xml.Serialization.Basic")]
        public void BasicSerialization()
        {
            var a = Basic1.GetSample();
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Basic1 MemberByte=""50"" MemberSByte=""-50"" MemberInt16=""-1000"" MemberInt32=""-20000000"" MemberInt64=""-1234567890123456"" "
                + @"MemberUInt16=""1000"" MemberUInt32=""20000000"" MemberUInt64=""1234567890123456"" MemberDecimal=""100.1234567890"" MemberSingle=""133.345"" MemberDouble=""122.123"" "
                + @"MemberDate=""2010-01-01T00:00:00"" MemberTimeSpan=""PT30M"" MemberGuid=""599cf792-722f-45e7-8c62-3774af4e7f7b"" MemberString=""Test String"" MemberRectangle=""100, 100, 300, 300"" />");

            var b = Serializer.Deserialize<Basic1>(serialized);
            Assert.AreEqual(a, b);
        }

        public class Basic1_Nullable
        {

            public Byte? MemberByte { get; set; }
            public SByte? MemberSByte { get; set; }

            public Int16? MemberInt16 { get; set; }
            public Int32? MemberInt32 { get; set; }
            public Int64? MemberInt64 { get; set; }
            public UInt16? MemberUInt16 { get; set; }
            public UInt32? MemberUInt32 { get; set; }
            public UInt64? MemberUInt64 { get; set; }

            public Decimal? MemberDecimal { get; set; }
            public Single? MemberSingle { get; set; }
            public Double? MemberDouble { get; set; }

            public DateTime? MemberDate { get; set; }
            public TimeSpan? MemberTimeSpan { get; set; }

            public Guid? MemberGuid { get; set; }

            public static Basic1_Nullable GetSample()
            {
                return new Basic1_Nullable
                {
                    MemberByte = 50,
                    MemberSByte = -50,
                    MemberInt16 = -1000,
                    MemberInt32 = -20000000,
                    MemberInt64 = -1234567890123456,
                    MemberUInt16 = 1000,
                    MemberUInt32 = 20000000,
                    MemberUInt64 = 1234567890123456,
                    MemberDate = new DateTime(2010, 1, 1),
                    MemberDecimal = 100.1234567890m,
                    MemberDouble = (double)122.123,
                    MemberGuid = new Guid("{599CF792-722F-45E7-8C62-3774AF4E7F7B}"),
                    MemberSingle = (float)133.345,
                    MemberTimeSpan = TimeSpan.FromMinutes(30),
                };
            }

            public override bool Equals(object other)
            {
                var b = other as Basic1_Nullable;
                if (b != null)
                {
                    if (b.MemberByte == MemberByte && b.MemberDate == MemberDate && b.MemberDecimal == MemberDecimal && b.MemberDouble == MemberDouble && b.MemberGuid == MemberGuid
                        && b.MemberInt16 == MemberInt16 && b.MemberInt32 == MemberInt32 && b.MemberInt64 == MemberInt64 && b.MemberSByte == MemberSByte && b.MemberSingle == MemberSingle
                        && b.MemberTimeSpan == MemberTimeSpan && b.MemberUInt16 == MemberUInt16 && b.MemberUInt32 == MemberUInt32 && b.MemberUInt64 == MemberUInt64)
                    {
                        return true;
                    }
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
        [TestCategory("Xml.Serialization.Basic")]
        public void Nullables()
        {
            var a = Basic1_Nullable.GetSample();
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Basic1_Nullable MemberByte=""50"" MemberSByte=""-50"" MemberInt16=""-1000"" MemberInt32=""-20000000"" MemberInt64=""-1234567890123456"" "
                + @"MemberUInt16=""1000"" MemberUInt32=""20000000"" MemberUInt64=""1234567890123456"" MemberDecimal=""100.1234567890"" MemberSingle=""133.345"" MemberDouble=""122.123"" "
                + @"MemberDate=""2010-01-01T00:00:00"" MemberTimeSpan=""PT30M"" MemberGuid=""599cf792-722f-45e7-8c62-3774af4e7f7b"" />");

            var b = Serializer.Deserialize<Basic1_Nullable>(serialized);
            Assert.AreEqual(a, b);

            a.MemberByte = null;
            a.MemberDate = null;
            a.MemberDecimal = null;

            serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Basic1_Nullable MemberSByte=""-50"" MemberInt16=""-1000"" MemberInt32=""-20000000"" MemberInt64=""-1234567890123456"" "
                + @"MemberUInt16=""1000"" MemberUInt32=""20000000"" MemberUInt64=""1234567890123456"" MemberSingle=""133.345"" MemberDouble=""122.123"" "
                + @"MemberTimeSpan=""PT30M"" MemberGuid=""599cf792-722f-45e7-8c62-3774af4e7f7b"" />");

            b = Serializer.Deserialize<Basic1_Nullable>(serialized);
            Assert.AreEqual(a, b);

            a = new Basic1_Nullable();
            serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Basic1_Nullable />");

            b = Serializer.Deserialize<Basic1_Nullable>(serialized);
            Assert.AreEqual(a, b);
        }

        public class Basic1_NamingAndTypes
        {

            [XmlSerializationMember(NodeName = "mByte")]
            public string SerializedAsmByte { get; set; }

            [XmlSerializationMember(NodeName = "mInt32", NodeType = SerializationNodeType.Auto)]
            public string SerializedAsmInt32ToAuto { get; set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.Attribute)]
            public string SerializedAsAttribute { get; set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.Element)]
            public string SerializedAsElement { get; set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.ElementAsCData)]
            public string SerializedAsCData { get; set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.Text)]
            public string SerializedAsText { get; set; }

            [XmlSerializationMember(Order = 0)]
            public string SerializedAsFirstAttribute { get; set; }

            [XmlSerializationMember(Order = 0, NodeType = SerializationNodeType.Element)]
            public string SerializedAsFirstElement { get; set; }

            public static Basic1_NamingAndTypes GetSample()
            {
                return new Basic1_NamingAndTypes
                {
                    SerializedAsmByte = "a",
                    SerializedAsmInt32ToAuto = "b",
                    SerializedAsAttribute = "c",
                    SerializedAsElement = "d",
                    SerializedAsCData = "e",
                    SerializedAsText = "f",
                    SerializedAsFirstAttribute = "g",
                    SerializedAsFirstElement = "h",
                };
            }

            public override bool Equals(object other)
            {
                var b = other as Basic1_NamingAndTypes;
                if (b != null)
                {
                    if (b.SerializedAsmByte == SerializedAsmByte && b.SerializedAsmInt32ToAuto == SerializedAsmInt32ToAuto && b.SerializedAsAttribute == SerializedAsAttribute && b.SerializedAsElement == SerializedAsElement
                        && b.SerializedAsCData == SerializedAsCData && b.SerializedAsText == SerializedAsText && b.SerializedAsFirstAttribute == SerializedAsFirstAttribute && b.SerializedAsFirstElement == SerializedAsFirstElement)
                    {
                        return true;
                    }
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
        [TestCategory("Xml.Serialization.Basic")]
        public void NamingAndNodeTypes()
        {
            var a = Basic1_NamingAndTypes.GetSample();
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Basic1_NamingAndTypes SerializedAsFirstAttribute=""g"" mByte=""a"" mInt32=""b"" SerializedAsAttribute=""c""><SerializedAsFirstElement>h</SerializedAsFirstElement>"
                + @"<SerializedAsElement>d</SerializedAsElement><SerializedAsCData><![CDATA[e]]></SerializedAsCData>f</Basic1_NamingAndTypes>");

            var b = Serializer.Deserialize<Basic1_NamingAndTypes>(serialized);
            Assert.AreEqual(a, b);

            a = new Basic1_NamingAndTypes();
            serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Basic1_NamingAndTypes />");

            b = Serializer.Deserialize<Basic1_NamingAndTypes>(serialized);
            Assert.AreEqual(a, b);
        }
       
        public class Basic1_Privates
        {

            public Basic1_Privates()
            {

            }

            public Basic1_Privates(int id, string name, DateTime date, int index)
            {
                Id = id;
                Name = name;
                Date = date;
                _index = index;
            }

            [XmlSerializationMember(NodeType = SerializationNodeType.Attribute, NodeName = "i")]
            private int _index;

            [XmlSerializationMember(NodeType = SerializationNodeType.Attribute)]
            public int Id { get; private set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.Element)]
            public string Name { get; internal set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.Element)]
            private DateTime Date { get; set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.Element)]
            public static string StaticName { get; set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.Element)]
            public string Description
            {
                get
                {
                    return "Get Only Property";
                }
            }

            public override bool Equals(object other)
            {
                var b = other as Basic1_Privates;
                if (b != null)
                {
                    if (b.Id == Id && b.Name == Name && b.Date == Date && b.Description == Description && b._index == _index)
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
        [TestCategory("Xml.Serialization.Basic")]
        public void Privates()
        {
            Basic1_Privates.StaticName = "Static1";
            var a = new Basic1_Privates(123, "Test", new DateTime(2014, 1, 1), 7);
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<Basic1_Privates Id=""123"" i=""7""><Name>Test</Name><Date>2014-01-01T00:00:00</Date><StaticName>Static1</StaticName><Description>Get Only Property</Description></Basic1_Privates>");

            Basic1_Privates.StaticName = "Static2";

            var b = Serializer.Deserialize<Basic1_Privates>(serialized);
            Assert.AreEqual(a, b);
            Assert.AreEqual(Basic1_Privates.StaticName, "Static1");
        }

        public class StringTest
        {

            [XmlSerializationMember(NodeType = SerializationNodeType.Attribute)]
            public string A { get; set; }

            [XmlSerializationMember(NodeType = SerializationNodeType.Text)]
            public string B { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as StringTest;
                if (other != null && other.A == A && other.B == B)
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
        [TestCategory("Xml.Serialization.Basic")]
        public void NullAndEmptyStrings()
        {
            var a = new StringTest { A = "AA", B = "BB" };
            var serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<StringTest A=""AA"">BB</StringTest>");

            var ad = Serializer.Deserialize<StringTest>(serialized);
            Assert.AreEqual(a, ad);

            a = new StringTest { A = "", B = "" };
            serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<StringTest A=""""></StringTest>");

            ad = Serializer.Deserialize<StringTest>(serialized);
            Assert.AreEqual(a, ad);

            a = new StringTest { A = null, B = null };
            serialized = Serializer.Serialize(a);
            Assert.AreEqual(serialized, @"<StringTest />");

            ad = Serializer.Deserialize<StringTest>(serialized);
            Assert.AreEqual(a, ad);
        }

    }
}