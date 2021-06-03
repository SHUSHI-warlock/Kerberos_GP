using AppServer;
using Client.Utils.DesUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Ker
{
    public class Authenticator
    {
        public String IDc; //客户端ID
        public String ADc; //客户端网络地址
        public String TS; //时间戳

        public Authenticator()
        {
            this.IDc = "";
            this.ADc = "";
            this.TS = "";
        }

        public Authenticator(String id, String ad, String ts)
        {
            this.IDc = id;
            this.ADc = ad;
            this.TS = ts;
        }

        public byte[] Encrypt(DesKey Key)
        {
            String mid = IDc + ADc + TS;
            byte[] message = System.Text.Encoding.Default.GetBytes(mid);
            //byte[] message = mid.getBytes();
            DESUtils des = new DESUtils(Key);
            byte[] result = des.Encryption(message);
            //调用DES加密函数对mid进行加密
            return result;
        }

        public void Decrypt(byte[] message, DesKey Key)
        {

            DESUtils des = new DESUtils(Key);
            byte[] Message = des.Decryption(message);//首先调用DES解密函数对message进行解密

            byte[] id = byteManage.subBytes(Message, 0, 20);
            String ID = Encoding.UTF8.GetString(id);
            //String ID = new String(id);
            this.IDc = ID;
            byte[] ad = byteManage.subBytes(Message, 20, 16);
            String AD = Encoding.UTF8.GetString(ad);
            //String AD = new String(ad);
            this.ADc = AD;
            byte[] ts = byteManage.subBytes(Message, 36, 12);
            String T = Encoding.UTF8.GetString(ts);
            //String T = new String(ts);
            this.TS = T;

        }
    }
}
