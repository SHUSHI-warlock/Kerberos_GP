using System;
using System.Collections.Generic;
using System.Text;

namespace AppServer
{
    public class DesKey
    {
        BitSequence key;
        public DesKey(byte[] key)
        {
            this.key = new BitSequence(key);
        }
        public DesKey()
        {
        }

        public BitSequence getKey()
        {
            return key;
        }

        public byte[] getKeyBytes()
        {
            return key.toBytes();
        }

        public void GenKey()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            byte[] a = new byte[8];
            rnd.NextBytes(a);

            this.key = new BitSequence(a);
        }

        /**
         * 根据指定的数据生成的秘钥
         * @param str
         */
        public void GenKey(byte[] pwd)
        {
            sbyte[] str = ByteConverter.UbyteToSbyte(pwd);         
            string seed = "";
            for (int i = 0; i < str.Length; i++)
            {
                seed += str[i];
            }

            byte[] a = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                a[i] = (byte)Hash0(seed + 256 * i);
            }
            this.key = new BitSequence(a);
        }

        private static int Hash1(string str)
        {
            int seed = 131;
            int hash = 0;
            int count;
            char[] bitarray = str.ToCharArray();
            count = bitarray.Length;
            while (count > 0)
            {
                hash = hash * seed + (bitarray[bitarray.Length - count]);
                count--;
            }
            return (hash & 0x7FFFFFFF);
        }

        public static int Hash0(string str)
        {
            int BitsInUnignedInt = (int)(sizeof(int) * 8);
            int ThreeQuarters = (int)((BitsInUnignedInt * 3) / 4);
            int OneEighth = (int)(BitsInUnignedInt / 8);
            int hash = 0;
            unchecked
            {
                int HighBits = (int)(0xFFFFFFFF) << (BitsInUnignedInt - OneEighth);
                int test = 0;
                int count;
                char[] bitarray = str.ToCharArray();
                count = bitarray.Length;
                while (count > 0)
                {
                    hash = (hash << OneEighth) + (bitarray[bitarray.Length - count]);
                    if ((test = hash & HighBits) != 0)
                    {
                        hash = ((hash ^ (test >> ThreeQuarters)) & (~HighBits));
                    }
                    count--;
                }
            }
            return (hash & 0x7FFFFFFF);
        }


    }
}
