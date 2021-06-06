using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppServer;
using Client.Utils;
using Client.Utils.DesUtil;

namespace Client.Ker
{
    public class AuthenticationMessage
    {
        byteManage byteManage = new byteManage();

        public String IDc; //客户端ID
        public String ADc; //客户端网络地址
        public String IDtgs; //TGS服务器ID
        public String IDv; //服务器ID
        public String TS; //时间戳
                          //public String Key; //加密秘钥（客户端和TGS之间、客户端和服务器V之间)
        public DesKey Key;
        public int Lifetime; //票据有效期
        public Authenticator authenticator; //认证字段(经过加密后的)
        public Ticket ticket; //票据
                              //public String ticket1; //加密票据
        public byte[] ticket1 = new byte[64];


        public AuthenticationMessage()
        {
            this.IDc = "";
            this.ADc = "";
            this.IDtgs = "";
            this.IDv = "";
            this.TS = "";
            this.Key = new DesKey();
            this.ticket = new Ticket();
            authenticator = new Authenticator();
        }

        public AuthenticationMessage(String id, String tgs, String ts)
        {
            this.IDc = id;
            this.IDtgs = tgs;
            this.TS = ts;
        }
        public AuthenticationMessage(DesKey key, String tgs, String ts, int lifetime, Ticket ticket)
        {
            this.Key = key;
            this.IDtgs = tgs;
            this.TS = ts;
            this.Lifetime = lifetime;
            this.ticket = ticket;
        }
        public AuthenticationMessage(String id, Ticket ticket, Authenticator au)
        {
            this.IDv = id;
            this.ticket = ticket;
            this.authenticator = au;
        }
        public AuthenticationMessage(DesKey key, String id, String ts, Ticket ticket)
        {
            this.Key = key;
            this.IDv = id;
            this.TS = ts;
            this.ticket = ticket;
        }
        public AuthenticationMessage(Ticket ticket, Authenticator au)
        {
            this.ticket = ticket;
            this.authenticator = au;
        }
        public AuthenticationMessage(String ts)
        {
            this.TS = ts;
        }



        public void CASMessage(byte[] message)
        { //AS接收到客户端消息

            byte[] MESSAGE = message;
            byte[] idc = byteManage.subBytes(MESSAGE, 0, 20);
            String id = Encoding.UTF8.GetString(idc);
            //String id = new String(idc);
            byte[] tg = byteManage.subBytes(MESSAGE, 20, 1);
            String tgs = Encoding.UTF8.GetString(tg);
            //String tgs = new String(tg);
            byte[] ts = byteManage.subBytes(MESSAGE, 21, 12);
            String Ts = Encoding.UTF8.GetString(ts);
            //String Ts = new String(ts);
            this.IDc = id.Trim();
            this.IDtgs = tgs;
            this.TS = Ts;
        }



        public void ASCMessage(byte[] message, DesKey Key)
        {  //客户端接收到AS消息
           //对信息进行解密
            DESUtils des = new DESUtils(Key);
            byte[] me = des.Decryption(message);
            //String as=new String(me);
            //System.out.println(as);
            //System.out.println(message.length);
            Console.WriteLine(Encoding.Default.GetString(me));
            

            //byte[] me=mess.getBytes();
            byte[] key = byteManage.subBytes(me, 0, 8);
            DesKey b = new DesKey(key);
            this.Key = new DesKey();
            this.Key = b;
            byte[] tgs = byteManage.subBytes(me, 8, 1);
            String id = Encoding.UTF8.GetString(tgs);
            //String id = new String(tgs);
            this.IDtgs = id;
            //System.out.println(IDtgs);
            byte[] ts = byteManage.subBytes(me, 9, 12);
            String T = Encoding.UTF8.GetString(ts);
            //String T = new String(ts);
            this.TS = T;
            byte[] li = byteManage.subBytes(me, 21, 1);
            String L = Encoding.UTF8.GetString(li);
            //String L = new String(li);
            this.Lifetime = int.Parse(L);
            //this.Lifetime = Integer.parseInt(L);
            byte[] tic = byteManage.subBytes(me, 22, 64);
            //String tick=new String(tic);
            ticket1 = tic;

        }

        public int CTGSMessage(byte[] message,DesKey TGSKeyAS)
        { //TGS接收到客户端信息
            byte[] MESSAGE = message;
            byte[] idv = byteManage.subBytes(MESSAGE, 0, 1);
            String v = Encoding.UTF8.GetString(idv);
            //String v = new String(idv);
            this.IDv = v;
            byte[] tic = byteManage.subBytes(MESSAGE, 1, 64);
            this.ticket1 = tic;
            //String tick=new String(tic);
            Ticket ticket = new Ticket();
            ticket.ticketDecrypt(tic, TGSKeyAS);
            this.ticket = new Ticket();
            this.ticket = ticket;
            this.Key = new DesKey();
            this.Key = ticket.Key;
            this.IDc = ticket.IDc.Trim();
            this.ADc = ticket.ADc;
            this.IDtgs = ticket.ID;
            this.TS = ticket.TS.Trim();
            this.Lifetime = ticket.Lifetime;
            byte[] au = byteManage.subBytes(MESSAGE, 65, 48);
            String Au = Encoding.UTF8.GetString(au);
            //String Au = new String(au);
            Authenticator AU = new Authenticator();
            AU.Decrypt(au, Key);
            //AU.Decrypt(Au,this.Key);
            this.authenticator = AU;
            return 0;
        }

        public int TGSCMessage(byte[] message, DesKey sKey)
        { //客户端接收到TGS消息

            DESUtils des = new DESUtils(sKey);
            byte[] MESSAGE = des.Decryption(message);

            byte[] key = byteManage.subBytes(MESSAGE, 0, 8);
            //String K = new String(key);
            DesKey a = new DesKey(key);
            this.Key = new DesKey();
            this.Key = a;
            byte[] id = byteManage.subBytes(MESSAGE, 8, 1);
            String v = Encoding.UTF8.GetString(id);
            //String v = new String(id);
            this.IDv = v;
            byte[] ts = byteManage.subBytes(MESSAGE, 9, 12);
            String t = Encoding.UTF8.GetString(ts);
            //String t = new String(ts);
            this.TS = t.Trim();
            byte[] tic = byteManage.subBytes(MESSAGE, 21, 64);
            //String tick = new String(tic);
            this.ticket1 = tic;
            return 0;

        }


        public void VCMessage(byte[] message, DesKey Kcv)
        {
            DESUtils des = new DESUtils(Kcv);
            byte[] MESSAGE = des.Decryption(message);
            //String t = new String(MESSAGE);
            String t = Encoding.Default.GetString(MESSAGE);
            this.TS = t.Trim();
        }

    }
}
