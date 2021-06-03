using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Client.Utils.RSAUtil
{
    class RSAUtils
    {
        //测试
        public static void main(String[] args)
        {
            KeyPair keyPair = new KeyPair();
            keyPair.GenKey(1024);

            // byte[] m = { 98, -77, 94, 2, -39, 73, -45, 91 };
            byte[] m = { 9, 49, 9, 2, 39, 73, 45, 91 };
            Console.WriteLine("明文：" + string.Join(",", m));
            byte[] c = RSAUtils.Encryption(keyPair.getPk(), m);
            Console.WriteLine("密文：" + string.Join(",", c));
            byte[] mm = RSAUtils.Decryption(keyPair.getSk(), c);
            Console.WriteLine("解密后结果:" + string.Join(",", mm));
        }

        /**
         * 最大加密128字节
         * @param pk
         * @param M
         * @return
         */
        public static byte[] Encryption(PublicKey pk, byte[] M)
        {
            BigInteger m = new BigInteger(ByteTransUtil.ReverseBytes(M));//改为大端字节顺序写入，采用无符号编码，匹配java
            BigInteger c = BigInteger.ModPow(m, pk.getE(), pk.getN());
            return ByteTransUtil.ReverseBytes(c.ToByteArray());
        }

        public static byte[] Decryption(PrivateKey sk, byte[] C)
        {
            BigInteger c = new BigInteger(ByteTransUtil.ReverseBytes(C));//改为大端字节顺序写入，采用无符号编码，匹配java
            BigInteger m = BigInteger.ModPow(c, sk.getD(), sk.getN());
            return ByteTransUtil.ReverseBytes(m.ToByteArray());
        }

    }
}
