using System.Xml;

namespace Atlas.Xml
{
    /// <summary>
    /// Provides static methods for xml operations
    /// </summary>
    public static class XmlHelper
    {

        /// <summary>
        /// Skips current element and moves to next element. Similar to XmlReader.Skip, but this method also skips even if current node is text node.
        /// </summary>
        /// <param name="reader">XmlReader with current node of element or text of element.</param>
        public static bool MoveUpToNextElement(this XmlReader reader)
        {
            if (reader.NodeType != XmlNodeType.EndElement)
            {
                while (reader.Read())
                {
                    // Break on end element
                    if (reader.NodeType == XmlNodeType.EndElement)
                        break;

                    // Skip child elements
                    while (reader.NodeType == XmlNodeType.Element)
                        reader.Skip();
                }
            }

            // Read past end element
            return reader.Read();
        }

        /// <summary>
        /// Skips current element and moves to next element. Similar to XmlReader.Skip, but this method also skips even if current node is text node.
        /// </summary>
        /// <param name="reader">XmlReader with current node of element or text of element.</param>
        public static bool MoveToFirstChild(this XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        return true;

                    if (reader.NodeType == XmlNodeType.EndElement)
                        break;
                }
            }

            // Read past end element
            reader.Read();

            return false;
        }

    }
}