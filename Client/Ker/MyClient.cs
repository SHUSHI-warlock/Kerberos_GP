using AppServer;
using Client.Utils.DesUtil;
using Client.Utils.RSAUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Ker
{
    public class MyClient
    {
        public String id; //客户端ID
        public String password; //客户端密码
                                //public String asKey; //和AS服务器之间的加密秘钥
        public DesKey asKey;
        //public String tgsKey; //和TGS之间的加密秘钥
        public DesKey tgsKey;
        //public String vKey; //和服务器之间的加密秘钥
        public DesKey vKey;
        public DesKey signKey; //和AS之间注册使用的对称秘钥
        public PublicKey pk; //AS服务器的公钥
        public String IDtgs = "0"; //TGS服务器ID
        public String IDv = "0"; //服务器ID
        public AuthenticationMessage toASmessage; //AS服务器认证报文
        public AuthenticationMessage fromASmessage; //AS返回报文
        public AuthenticationMessage toTGSmessage; //TGS认证报文
        public AuthenticationMessage fromTGSmessage; //TGS返回报文
        public String TGSticket; //访问TGS票据
        public AuthenticationMessage toVmessage; //服务器认证报文
        public AuthenticationMessage fromVmessage; //服务器返回报文
        public String Vticket; //访问服务器票据
        public Authenticator TGSauthenticator; //向TGS
        public Authenticator Vauthenticator; //向V
        public byte[] tgsTicket;
        public byte[] vTicket;
        public int status1 = 0;

        public string VTime;

        public void generateKey()
        {
            asKey = new DesKey();
            byte[] pass = Encoding.Default.GetBytes(this.password);
            asKey.GenKey(pass);
        }
        public void generateKey(String password)
        {
            asKey = new DesKey();
            byte[] pass = Encoding.Default.GetBytes(password);
            asKey.GenKey(pass);
        }


        public String ASconfirm(String id, String idTgs)
        {

            String ts = byteManage.generateTime();
            if (id.Length < 20)
            {
                for (int i = id.Length; i < 20; i++)
                {
                    id += " ";
                }
            }
            this.id = id;
            String mid = id + idTgs + ts;
            toASmessage = new AuthenticationMessage();
            toASmessage.IDc = id;
            toASmessage.IDtgs = idTgs;
            this.IDtgs = idTgs;
            toASmessage.TS = ts;

            return mid;
        }

        public void manageAS(AuthenticationMessage fromASmessage)
        {
            tgsKey = new DesKey();
            this.tgsKey = fromASmessage.Key;
            this.tgsTicket = fromASmessage.ticket1;
        }

        public bool VerifyAS(AuthenticationMessage au)
        {
            if (!(this.IDtgs.Equals(au.IDtgs)))
            {
                Console.WriteLine("解密失败");
                return false;
            }
            String ts = byteManage.generateTime();
            String Ts = ts.Trim();
            
            String hour = Ts.Substring(0, 2);
            String TS = au.TS;
            String hour2 = TS.Substring(0, 2);
            if (!(hour.Equals(hour2)))
            {
                Console.WriteLine("时钟不同步");
                return false;
            }
            //当前时间戳和fromTGSmessage中的时间戳进行对比
            return true;
        }

        public byte[] TGSconfirm(String id, byte[] TGSticket, String IDc, String ADc, DesKey tgsKey)
        {
            String ts = byteManage.generateTime();
            TGSauthenticator = new Authenticator(IDc, ADc, ts);
            byte[] au = TGSauthenticator.Encrypt(tgsKey);
            byte[] mid = byteManage.concat(TGSticket, au);
            byte[] miid = byteManage.concat(Encoding.Default.GetBytes(id), mid);
            return miid;
        }

        public void manageTGS(AuthenticationMessage fromTGSmessage)
        {
            vKey = new DesKey();
            this.vKey = fromTGSmessage.Key;
            this.vTicket = fromTGSmessage.ticket1;
        }

        public bool VerifyTGS(AuthenticationMessage au)
        {
            if (!(this.IDv.Equals(au.IDv)))
            {
                Console.WriteLine("解密失败");
                return false;
            }
            String ts = byteManage.generateTime(); //查看时间戳
            String Ts = ts.Trim();
            String hour = Ts.Substring(0, 2);
            String TS = au.TS;
            String hour2 = TS.Substring(0, 2);
            if (!(hour.Equals(hour2)))
            {
                Console.WriteLine("时钟不同步");
                return false;
            }

            return true;
        }

        public byte[] Vconfirm(byte[] Vticket, String IDc, String ADc, DesKey vKey)
        {
            String ts = byteManage.generateTime(out VTime);

            //VTime = DateTime.Now.AddMinutes(1).ToString("HH:mm:ss");

            Vauthenticator = new Authenticator(IDc, ADc, ts);
            byte[] au = Vauthenticator.Encrypt(vKey);
            byte[] mid = byteManage.concat(Vticket, au);
            //向V发送
            return mid;
        }

        public void VerifyV()
        {
            //对时间戳进行判断
        }

    }
}
