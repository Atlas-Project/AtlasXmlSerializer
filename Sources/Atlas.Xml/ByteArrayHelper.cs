using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

namespace Atlas
{
    public static class ByteArrayHelper
    {

        const int ConversionBufferSize = 4080;
        const int ConversionHalfBufferSize = ConversionBufferSize / 2;

        #region BinHex => byte[]

        /// <summary>
        /// Converts hex coded string (ie: 15A4C2) into byte[] (eg: new byte[] { 21, 164, 194 })
        /// </summary>
        /// <param name="hexString">Hex string to be converted</param>
        /// <returns>new byte[] containing conversion result</returns>
        public static byte[] ToByteArray(string hexString)
        {
            var result = new byte[hexString.Length / 2];

            unchecked
            {
                char c;
                for (int i = 0; i < result.Length; i++)
                {
                    // First byte
                    c = hexString[i * 2];
                    result[i] = (byte)((c < 0x40 ? c - 0x30 : (c < 0x47 ? c - 0x37 : c - 0x57)) << 4);

                    // Second byte
                    c = hexString[i * 2 + 1];
                    result[i] = (byte)(c < 0x40 ? c - 0x30 : (c < 0x47 ? c - 0x37 : c - 0x57));
                }
            }

            return result;
        }

        public static byte[] ReadBinHex(this XmlReader reader)
        {
            using (var stream = new MemoryStream())
            {
                var buffer = new byte[ConversionBufferSize];
                int bytesRead;
                while ((bytesRead = reader.ReadContentAsBinHex(buffer, 0, ConversionHalfBufferSize)) > 0)
                    stream.Write(buffer, 0, bytesRead);

                return stream.ToArray();
            }
        }

        public static byte[] ReadBase64(this XmlReader reader)
        {
            using (var stream = new MemoryStream())
            {
                var buffer = new byte[ConversionBufferSize];
                int bytesRead;
                while ((bytesRead = reader.ReadContentAsBase64(buffer, 0, ConversionHalfBufferSize)) > 0)
                    stream.Write(buffer, 0, bytesRead);

                return stream.ToArray();
            }
        }

        #endregion

        #region byte[] => Bin Hex 

        private static readonly uint[] _hexConversionLookup = CreateHexConversionLookup();
        private static uint[] CreateHexConversionLookup()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private unsafe static readonly uint* _byteHexCharsP = (uint*)GCHandle.Alloc(_hexConversionLookup, GCHandleType.Pinned).AddrOfPinnedObject();

        /// <summary>
        /// Writes bytes to xml writer with BinHex format (ie: 15A4C2)
        /// </summary>
        /// <param name="writer">Writer to be written to</param>
        /// <param name="bytes">Bytes to be written</param>
        public static unsafe void WriteBinHex(this XmlWriter writer, byte[] bytes)
        {
            fixed (byte* bytesP = bytes)
            {
                var bufferIndex = 0;
                var bufferLength = bytes.Length < ConversionBufferSize ? bytes.Length : ConversionBufferSize;
                var charBuffer = new char[bufferLength * 2];
                fixed (char* bufferP = charBuffer)
                {
                    uint* buffer = (uint*)bufferP;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        buffer[bufferIndex] = _byteHexCharsP[bytes[i]];

                        bufferIndex++;
                        if (bufferIndex == bufferLength)
                        {
                            writer.WriteRaw(charBuffer, 0, charBuffer.Length);
                            bufferIndex = 0;
                        }
                    }
                }

                if (bufferIndex > 0)
                    writer.WriteRaw(charBuffer, 0, bufferIndex * 2);
            }
        }

        /// <summary>
        /// Writes bytes to stream with BinHex format (ie: 15A4C2)
        /// </summary>
        /// <param name="writer">Stream to be written to</param>
        /// <param name="bytes">Bytes to be written</param>
        public static unsafe void WriteBinHex(this StreamWriter writer, byte[] bytes)
        {
            fixed (byte* bytesP = bytes)
            {
                var bufferIndex = 0;
                var bufferLength = bytes.Length < ConversionBufferSize ? bytes.Length : ConversionBufferSize;
                var charBuffer = new char[bufferLength * 2];
                fixed (char* bufferP = charBuffer)
                {
                    uint* buffer = (uint*)bufferP;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        buffer[bufferIndex] = _byteHexCharsP[bytes[i]];

                        bufferIndex++;
                        if (bufferIndex == bufferLength)
                        {
                            writer.Write(charBuffer, 0, charBuffer.Length);
                            bufferIndex = 0;
                        }
                    }
                }

                if (bufferIndex > 0)
                    writer.Write(charBuffer, 0, bufferIndex * 2);
            }
        }

        /// <summary>
        /// Converts byte array to BinHex format (ie: 15A4C2)
        /// </summary>
        /// <param name="bytes">Bytes to be converted</param>
        /// <returns>A new string containing BinHex representation of bytes</returns>
        public unsafe static string ToHexString(this byte[] bytes)
        {
            var lookupP = _byteHexCharsP;
            var result = new string((char)0, bytes.Length * 2);
            fixed (byte* bytesP = bytes)
            fixed (char* resultP = result)
            {
                uint* resultP2 = (uint*)resultP;
                for (int i = 0; i < bytes.Length; i++)
                    resultP2[i] = lookupP[bytesP[i]];
            }

            return result;
        }

        #endregion

    }
}
