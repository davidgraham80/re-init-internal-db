using System;
using System.Net;
using System.IO;
using SevenZip.Sdk.Compression.Lzma;
using SevenZip.Sdk;

namespace SevenZip
{
    public class SevenZipHelper
    {
        /// <summary>
        /// Compress byte array with LZMA algorithm
        /// </summary>
        /// <param name="data">Byte array to compress</param>
        /// <returns>Compressed byte array</returns>
        public static byte[] CompressBytes(byte[] data)
        {
            #region Lzma properties
            CoderPropId[] propIDs = 
			{
				CoderPropId.DictionarySize,
				CoderPropId.PosStateBits,
				CoderPropId.LitContextBits,
				CoderPropId.LitPosBits,
				CoderPropId.Algorithm,
				CoderPropId.NumFastBytes,
				CoderPropId.MatchFinder,
				CoderPropId.EndMarker
			};
            object[] properties = 
			{
				1 << 24,
				2,
				3,
				0,
				2,
				256,
				"bt4",
				false
			};
            #endregion
            if (data.Length > 100 * 1024 * 1024 )
                GC.Collect();
            using (MemoryStream inStream = new MemoryStream(data))
            {
                using (MemoryStream outStream = new MemoryStream(data.Length + 10240 ))
                {
                    Encoder encoder = new Encoder();
                    encoder.SetCoderProperties(propIDs, properties);
                    encoder.WriteCoderProperties(outStream);
                    long streamSize = inStream.Length;
                    for (int i = 0; i < 8; i++)
                        outStream.WriteByte((byte)(streamSize >> (8 * i)));
                    encoder.Code(inStream, outStream, -1, -1, null);
                    return outStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Decompress byte array compressed with LZMA algorithm
        /// </summary>
        /// <param name="data">Byte array to decompress</param>
        /// <returns>Decompressed byte array</returns>
        public static byte[] ExtractBytes(byte[] data)
        {
            using (MemoryStream inStream = new MemoryStream(data))
            {
                Decoder decoder = new Decoder();
                inStream.Seek(0, 0);
                using (MemoryStream outStream = new MemoryStream())
                {
                    byte[] LZMAproperties = new byte[5];
                    #region Read LZMA properties
                    if (inStream.Read(LZMAproperties, 0, 5) != 5)
                    {
                        throw new LzmaException();
                    }
                    long outSize = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        int b = inStream.ReadByte();
                        if (b < 0)
                        {
                            throw new LzmaException();
                        }
                        outSize |= ((long)(byte)b) << (i << 3);
                    }
                    #endregion
                    decoder.SetDecoderProperties(LZMAproperties);
                    decoder.Code(inStream, outStream, inStream.Length - inStream.Position, outSize, null);
                    return outStream.ToArray();
                }
            }
        }
    }
}

