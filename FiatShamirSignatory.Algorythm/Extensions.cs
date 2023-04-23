using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Math;

namespace FiatShamirSignatory.Algorythm
{
    public static class Extensions
    {
        public static string ToBitString(this BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string Join<T>(this IEnumerable<T> data, string sep = ", ")
        {
            return string.Join(sep, data);
        }

        public static string Join<T>(this IEnumerable<T> data, char sep)
        {
            return string.Join(sep, data);
        }

        public static string BytesString(this BigInteger x) 
        {
            return $"{x}<{x.ToByteArray().Join(',')}>";
        }

        public static string Text(this byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(bytes);
        }

        public static string UTF8(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Hex(this byte[] bytes, bool upperCase = false)
        {
            return upperCase
                ? Convert.ToHexString(bytes)
                : Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
