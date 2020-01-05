using System;
using System.Collections.Generic;
using System.Text;

namespace BFAMExercise.Util
{
    public class ByteArrayUtil
    {
        public static IEnumerable<byte[]> Slice(byte[] source, byte separator)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source.Length == 0) yield break;

            int head = 0;
            for (var cur = 0; cur < source.Length; cur++)
            {
                if (source[cur] == separator)
                {
                    yield return SubByte(source, head, cur);
                    head = cur + 1;
                }
            }
            if (head == source.Length) yield break;
            yield return SubByte(source, head, source.Length - 1);
        }

        private static byte[] SubByte(byte[] source, int head, int cur)
        {
            var len = cur - head + 1;
            byte[] res = new byte[len];
            Array.Copy(source, head, res, 0, len);
            return res;
        }
    }
}
