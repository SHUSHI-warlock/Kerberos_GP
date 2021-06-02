using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Utils
{
    class ByteConverter
    {
        public static byte[] SbyteToUbyte(sbyte[] SignedByte)
        {
            byte[] UnsignedByte = new byte[SignedByte.Length];
            Buffer.BlockCopy(SignedByte, 0, UnsignedByte, 0, SignedByte.Length);
            return UnsignedByte;
        }

        public static sbyte[] UbyteToSbyte(byte[] UnsignedByte)
        {
            sbyte[] SignedByte = new sbyte[UnsignedByte.Length];
            Buffer.BlockCopy(UnsignedByte, 0, SignedByte, 0, UnsignedByte.Length);
            /*
            for (int i = 0; i < UnsignedByte.Length; i++)
            {
                if (UnsignedByte[i] > 127)
                    SignedByte[i] = (sbyte)(UnsignedByte[i] - 256);
                else
                    SignedByte[i] = (sbyte)UnsignedByte[i];
            }
            */
            return SignedByte;
        }

    }
}
