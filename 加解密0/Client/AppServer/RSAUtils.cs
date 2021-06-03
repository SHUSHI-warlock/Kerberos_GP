using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AppServer
{
    class RSAUtils
    {
        //测试
        public static void main(String[] args)
        {
            KeyPair keyPair = new KeyPair();
            keyPair.GenKey(1024);

            // byte[] m = { 98, -77, 94, 2, -39, 73, -45, 91 };

            DesKey desKey = new DesKey();
            desKey.GenKey();

            byte[] key = RSAUtils.Encryption(keyPair.getPk(), desKey.getKeyBytes());

            byte[] key2 = RSAUtils.Decryption(keyPair.getSk(), key);

            DesKey newDeskey = new DesKey(key2);


        }

        /**
         * 最大加密128字节
         * @param pk
         * @param M
         * @return
         */
        public static byte[] Encryption(PublicKey pk, byte[] M)
        {
            BigInteger m = new BigInteger(M,true,true);//改为大端字节顺序写入，采用无符号编码，匹配java
            BigInteger c = BigInteger.ModPow(m, pk.getE(), pk.getN());
            return c.ToByteArray(true,true);
        }

        public static byte[] Decryption(PrivateKey sk, byte[] C)
        {
            BigInteger c = new BigInteger(C, true, true);//改为大端字节顺序写入，采用无符号编码，匹配java
            BigInteger m = BigInteger.ModPow(c, sk.getD(), sk.getN());
            return m.ToByteArray(true, true);
        }

    }
}
