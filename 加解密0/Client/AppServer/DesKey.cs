using System;
using System.Collections.Generic;
using System.Text;

namespace AppServer
{
    class DesKey
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
        public void GenKey(byte[] str)
        {
            int seed = str.GetHashCode();
            Random rnd = new Random(seed);
            byte[] a = new byte[8];
            rnd.NextBytes(a);
            this.key = new BitSequence(a);
        }


    }
}
