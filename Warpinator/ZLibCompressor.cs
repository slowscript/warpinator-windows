using System;
using System.IO;
using ComponentAce.Compression.Libs.zlib;

namespace Warpinator
{
    class ZLibCompressor
    {
        const int DefaultCompressionLevel = 6;

        public static byte[] Compress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var zs = new ZOutputStream(ms, DefaultCompressionLevel))
                    zs.Write(data, 0, data.Length);
                return ms.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var ds = new ZOutputStream(ms))
                    ds.Write(data, 0, data.Length);
                return ms.ToArray();
            }
        }
    }
}
