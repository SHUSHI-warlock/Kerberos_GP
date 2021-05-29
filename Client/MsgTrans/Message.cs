using Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MsgTrans
{
    public class Message
    {
        public const  int HEAD_LEN = 8;

        public int MessageP2P { get; set; }
        public int MessageType { get; set; }
        public int StateCode { get; set; }
        public int UnDefineCode { get; set; }
        public int Length { get; set; }

        /**
	    * 消息头，8字节长度，依次由四个数字组成：messageP2P|messageType|stateCode|UnEncode|length
	    * ，数字按大端存取
	    */
        private byte[] messageHead;

        /**
         * 消息体，默认为UTF-8编码
         */
        private byte[] messageBody;

        public Message(int p2p,int type,int state)
        {
            MessageP2P =  p2p;
            MessageType = type;
            StateCode = state;
            UnDefineCode = -1;
            Length = 0;
        }

        /// <summary>
        /// 用报文内容初始化
        /// </summary>
        /// <param name="msg"></param>
        public Message(string msg)
        {
            if (msg == null || msg.Length == 0)
            {
                return;
            }
            this.messageBody = Encoding.UTF8.GetBytes(msg); ;
            this.Length = this.messageBody.Length;
        }

        /// <summary>
        /// 报文头初始化
        /// </summary>
        /// <param name="head"></param>
        public Message(byte[] head)
        {
            if (head == null || head.Length < HEAD_LEN)
            {
                return;
            }
            this.messageHead = new byte[HEAD_LEN];
            Array.Copy(head, 0, messageHead, 0, HEAD_LEN);
            this.parseHead();
        }

        public void SetBody(byte[] body)
        {
            messageBody = body;
        }

        public void SetBody(Object o)
        {
            string json = JsonConvert.SerializeObject(o);
            this.messageBody = Encoding.UTF8.GetBytes(json); 
            this.Length = this.messageBody.Length;
        }

        /// <summary>
        /// 解析报文头 
        /// </summary>
        private void parseHead()
        {
            if (messageHead == null || messageHead.Length != HEAD_LEN)
            {
                return;
            }
            this.MessageP2P = messageHead[0];
            this.MessageType = messageHead[1];
            this.StateCode = messageHead[2];
            this.UnDefineCode = messageHead[3];
            byte[] tmps = new byte[4];
            Array.Copy(messageHead, 4, tmps, 0, 4);
            this.Length = ByteTransUtil.byteArrayToInt(tmps, false);
        }

        /// <summary>
        /// 生成报文头
        /// </summary>
        private void composeHead()
        {
            this.messageHead = new byte[HEAD_LEN];
            messageHead[0] = (byte)(this.MessageP2P);
            messageHead[1] = (byte)this.MessageType;
            messageHead[2] = (byte)this.StateCode;
            messageHead[3] = (byte)this.UnDefineCode;
            Array.Copy(ByteTransUtil.intToByteArray(this.Length, false), 0, messageHead, 4, 4);
        }

        /// <summary>
        /// 从消息体返回字符串
        /// </summary>
        /// <returns></returns>
        public String bodyToString()
        {
            String body = null;
            if (this.messageBody != null && this.messageBody.Length > 0)
            {
                body = Encoding.UTF8.GetString(messageBody);
            }
            return body;
        }

        /// <summary>
        /// 生成完整消息对应的字节数组，如果没有消息体，就只有头部
        /// </summary>
        /// <returns></returns>
        public byte[] composeFull()
        {
            if (this.messageBody != null)
            {
                this.Length = this.messageBody.Length;
            }

            byte[] data = new byte[this.Length + HEAD_LEN];
            composeHead();
            Array.Copy(messageHead, 0, data, 0, HEAD_LEN);

            if (this.messageBody != null)
            {
                Array.Copy(this.messageBody, 0, data, HEAD_LEN, this.Length);
            }
            return data;
        }

    }
}
