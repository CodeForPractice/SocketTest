using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketTest.Common
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：ByteUtil.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/11/24 11:12:43
    /// </summary>
    public static class ByteUtil
    {
        public static readonly byte[] ZeroLengthBytes = BitConverter.GetBytes(0);
        public static readonly byte[] EmptyBytes = new byte[0];

        public static void EncodeString(this string data, out byte[] lengthBytes, out byte[] dataBytes)
        {
            if (data == null)
            {
                lengthBytes = EmptyBytes;
                dataBytes = ZeroLengthBytes;
            }
            else
            {
                dataBytes = Encoding.UTF8.GetBytes(data);
                lengthBytes = BitConverter.GetBytes(dataBytes.Length);
            }
        }

        public static byte[] EncodeDateTime(this DateTime data)
        {
            return BitConverter.GetBytes(data.Ticks);
        }

        public static string DecodeString(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            return Encoding.UTF8.GetString(sourceBuffer.DecodeBytes(startOffset, out nextStartOffset));
        }

        public static short DecodeShort(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var shortBytes = new byte[2];
            Buffer.BlockCopy(sourceBuffer, startOffset, shortBytes, 0, 2);
            nextStartOffset = startOffset + 2;
            return BitConverter.ToInt16(shortBytes, 0);
        }

        public static int DecodeInt(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var intBytes = new byte[4];
            Buffer.BlockCopy(sourceBuffer, startOffset, intBytes, 0, 4);
            nextStartOffset = startOffset + 4;
            return BitConverter.ToInt32(intBytes, 0);
        }

        public static long DecodeLong(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var longBytes = new byte[8];
            Buffer.BlockCopy(sourceBuffer, startOffset, longBytes, 0, 8);
            nextStartOffset = startOffset + 8;
            return BitConverter.ToInt64(longBytes, 0);
        }

        public static DateTime DecodeDateTime(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var longBytes = new byte[8];
            Buffer.BlockCopy(sourceBuffer, startOffset, longBytes, 0, 8);
            nextStartOffset = startOffset + 8;
            return new DateTime(BitConverter.ToInt64(longBytes, 0));
        }

        public static byte[] DecodeBytes(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var lengthBytes = new byte[4];
            Buffer.BlockCopy(sourceBuffer, startOffset, lengthBytes, 0, 4);
            startOffset += 4;
            var length = BitConverter.ToInt32(lengthBytes, 0);
            var dataBytes = new byte[length];
            Buffer.BlockCopy(sourceBuffer, startOffset, dataBytes, 0, length);
            startOffset += length;
            nextStartOffset = startOffset;
            return dataBytes;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] destination = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, destination, offset, data.Length);
                offset += data.Length;
            }
            return destination;
        }

        public static byte[] CombineToByte(this IEnumerable<byte[]> arrays)
        {
            byte[] destination = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, destination, offset, data.Length);
                offset += data.Length;
            }
            return destination;
        }
    }
}