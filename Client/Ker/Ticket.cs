using Client.Utils.DesUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Ker
{
    public class Ticket
    {
        public DesKey Key;
        public String IDc; //客户端ID
        public String ADc; //客户端网络地址
        public String ID; //服务器ID（TGS服务器ID）
        public String TS; //时间戳
        public int Lifetime; //票据有效期
        public Ticket()
        {
            this.Key = new DesKey();
            this.IDc = "";
            this.ADc = "";
            this.ID = "";
            this.TS = "";
            this.Lifetime = 2;
        }
        /*
        Ticket(String key,String idc,String ad,String id,String ts,int lifetime){
            this.Key=key;
            this.IDc=idc;
            this.ADc=ad;
            this.ID=id;
            this.TS=ts;
            this.Lifetime=lifetime;
        }

         */
        Ticket(DesKey key, String idc, String ad, String id, String ts, int lifetime)
        {
            Key = new DesKey();
            Key = key;
            this.IDc = idc;
            this.ADc = ad;
            this.ID = id;
            this.TS = ts;
            this.Lifetime = lifetime;
        }

        public byte[] ticketEncrypt(DesKey Key, DesKey thKey)
        {
            //thKey=new DesKey();
            //byte[] key=this.Key.getKeyBytes();
            byte[] key = thKey.getKeyBytes();

            String mid = this.IDc + this.ADc + this.ID + this.TS + Lifetime.ToString();

            //System.out.println(mid);
            byte[] miid = System.Text.Encoding.Default.GetBytes(mid);
            //byte[] miid = mid.getBytes(StandardCharsets.UTF_8);
            //String s=new String(miid);
            //System.out.println(s);
            byte[] fin = byteManage.concat(key, miid);
            //System.out.println(fin.length);
            DESUtils des = new DESUtils(Key);
            byte[] result = des.Encryption(fin);
            //调用DES对mid进行加密
            return result;
        }

        public void ticketDecrypt(byte[] message, DesKey Key)
        {
            //调用DES解密函数对字符串先进行解密
            DESUtils des = new DESUtils(Key);
            byte[] Message = des.Decryption(message);
            //String s=new String(Message);
            //System.out.println(s);

            byte[] k = byteManage.subBytes(Message, 0, 8);
            DesKey key = new DesKey(k);
            //String K=new String(k);
            this.Key = key;
            byte[] id = byteManage.subBytes(Message, 8, 20);
            String I = Encoding.UTF8.GetString(id);
            //String I = new String(id);
            //System.out.println(I);
            this.IDc = I;
            byte[] ad = byteManage.subBytes(Message, 28, 16);
            String A = Encoding.UTF8.GetString(ad);
            //String A = new String(ad);
            //System.out.println(A);
            this.ADc = A;
            byte[] idv = byteManage.subBytes(Message, 44, 1);
            String v = Encoding.UTF8.GetString(idv);
            //String v = new String(idv);
            this.ID = v;
            byte[] ts = byteManage.subBytes(Message, 45, 12);
            String t = Encoding.UTF8.GetString(ts);
            //String t = new String(ts);
            //System.out.println(t);
            this.TS = t;
            byte[] li = byteManage.subBytes(Message, 57, 1);
            String l = Encoding.UTF8.GetString(li);
            //String l = new String(li);
            //System.out.println(l);
            int L = int.Parse(l);
            //int L = Integer.parseInt(l);
            this.Lifetime = L;

        }
    }
}
