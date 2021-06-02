using Client.MsgTrans;
using Client.Utils.DesUtil;
using Client.Utils.RSAUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Controls
{
    public enum EncryptionType {
        Plain = 0,
        Des = 1,
        Rsa_pk =2,
        Rsa_sk =3
    }

    public class MsgRecord 
    {
        public int MessageP2P { get; private set; }
        public int MessageType { get; private set; }
        public int StateCode { get; private set; }
        public int Length { get; private set; }

        public DateTime Time { get; private set; }
        /// <summary>
        /// 加密类型
        /// </summary>
        public EncryptionType Type { get;private set; }
        
        public string Key { get;private set; }

        /// <summary>
        /// 明文
        /// </summary>
        public string M { get;private set; }
        /// <summary>
        /// 密文
        /// </summary>
        public string C { get; private set; }

        /// <summary>
        /// 默认构造函数 解析头部
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        public MsgRecord(Message msg, EncryptionType type, string key, string m,string c)
        {
            Time = DateTime.Now;
            Type = type;
            MessageP2P = msg.MessageP2P;
            MessageType = msg.MessageType;
            StateCode = msg.StateCode;
            Length = msg.Length;

            Key = key;
            M = m;
            C = c;
        }
        /*
        /// <summary>
        /// RSA公钥 加密
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="pk"></param>
        public  MsgRecord(Message msg, PublicKey pk):this(msg,EncryptionType.Rsa_pk)
        {
            PK = pk;
            //此时msg里面时明文
            M = msg.bodyToString();
            C = Encoding.UTF8.GetString(RSAUtils.Encryption(pk, msg.GetBody()));
        }
        /// <summary>
        /// RSA私钥 加密
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="sk"></param>
        public  MsgRecord(Message msg, PrivateKey sk) : this(msg, EncryptionType.Rsa_sk)
        {
            SK = sk;
            //此时msg里面时密文
            C = msg.bodyToString();
            M = Encoding.UTF8.GetString(RSAUtils.Decryption(sk, msg.GetBody()));
        }
        /// <summary>
        /// Des加密
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="dk"></param>
        public  MsgRecord(Message msg, DesKey dk) : this(msg, EncryptionType.Des)
        {
            DK = dk;
            DESUtils dESUtils = new DESUtils(dk);
            if (msg.MessageP2P == 4)
            {//如果是客户端发给服务器，msg里面时明文
                M = msg.bodyToString();
                C = Encoding.UTF8.GetString(dESUtils.Encryption(msg.GetBody()));
            }
            else
            {
                C = msg.bodyToString();
                M = Encoding.UTF8.GetString(dESUtils.Decryption(msg.GetBody()));
            }
        }
        /// <summary>
        /// 明文
        /// </summary>
        /// <param name="msg"></param>
        public  MsgRecord(Message msg) : this(msg, EncryptionType.Plain)
        {
            M = msg.bodyToString();
            C = M;
        }
        */
    }
}
