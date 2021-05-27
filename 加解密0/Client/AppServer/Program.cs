using System;

namespace AppServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //输入秘钥
            DesKey a = new DesKey();
            a.GenKey();

            Console.WriteLine(string.Join(",", ByteConverter.UbyteToSbyte(a.getKey().toBytes())));

            DESUtils des = new DESUtils(a);

            //输入明文
            /*
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            byte[] m = new byte[127];
            rnd.NextBytes(m);
            */
            //sbyte[] sb = { -47, -128, 122, -35, 109, -81, 0, -9 };
            sbyte[] sb = {-98, -77, -94, -2, -39, -73, -45, -91 };
            byte[] m = ByteConverter.SbyteToUbyte(sb);
            Console.WriteLine("  明文  :" + string.Join(",",sb));
            byte[] C = des.Encryption(m);
            Console.WriteLine("加密结果:" + string.Join(",", ByteConverter.UbyteToSbyte(C)));
            byte[] M2 = des.Decryption(C);
            Console.WriteLine("解密结果:" + string.Join(",", ByteConverter.UbyteToSbyte(M2)));
        }
    }
}
