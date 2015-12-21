using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Atlas.Xml.UnitTests
{
    [TestClass]
    public class PerformanceTests
    {

        private const int PerformanceIterations = 60000;

        [TestMethod]
        [TestCategory("Xml.Performance")]
        public void Test1Performance()
        {
            var a = BasicTests.Basic1.GetSample();
            for (int i = 0; i < PerformanceIterations; i++)
            {
                var serialized = Serializer.Serialize(a, "root");
                var b = Serializer.Deserialize<BasicTests.Basic1>(serialized);
            }
        }

        [TestMethod]
        [TestCategory("Xml.Performance")]
        public void Test1Performance_MS()
        {
            var a = BasicTests.Basic1.GetSample();

            var msSerializer = new XmlSerializer(typeof(BasicTests.Basic1));

            var writerSettings = new XmlWriterSettings
            {
                Indent = false
            };

            for (var i = 0; i < PerformanceIterations; i++)
            {
                var builder = new StringBuilder();
                using (var writer = XmlWriter.Create(builder, writerSettings))
                    msSerializer.Serialize(writer, a);

                using (StringReader reader = new StringReader(builder.ToString()))
                {
                    var b = (BasicTests.Basic1)msSerializer.Deserialize(reader);
                }
            }
        }

        [TestMethod]
        [TestCategory("Xml.Performance")]
        public void Test2PerformanceOfList()
        {
            var a = new List<BasicTests.Basic1>();
            for (int i = 0; i < 20; i++)
                a.Add(BasicTests.Basic1.GetSample());

            for (int i = 0; i < PerformanceIterations / 4; i++)
            {
                var serialized = Serializer.Serialize(a, "root");
                var b = Serializer.Deserialize<BasicTests.Basic1>(serialized);
            }
        }

        [TestMethod]
        [TestCategory("Xml.Performance")]
        public void Test2PerformanceOfList_MS()
        {
            var a = new List<BasicTests.Basic1>();
            for (int i = 0; i < 20; i++)
                a.Add(BasicTests.Basic1.GetSample());

            var msSerializer = new XmlSerializer(typeof(List<BasicTests.Basic1>));

            var writerSettings = new XmlWriterSettings
            {
                Indent = false
            };

            for (var i = 0; i < PerformanceIterations / 4; i++)
            {
                var builder = new StringBuilder();
                using (var writer = XmlWriter.Create(builder, writerSettings))
                    msSerializer.Serialize(writer, a);

                using (StringReader reader = new StringReader(builder.ToString()))
                {
                    var b = (List<BasicTests.Basic1>)msSerializer.Deserialize(reader);
                }
            }
        }
    }
}