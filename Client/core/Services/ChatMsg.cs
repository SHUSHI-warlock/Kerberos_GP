using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.core.Services
{
    public class ChatMsg
    {
        public string UserId { get; set; }
        public DateTime Time { get; set; }
        public string Msg { get; set; }

        public int Type {get; set;}

        public ChatMsg() { }
        public ChatMsg(string id,DateTime time,string msg)
        {
            UserId = id;
            Time = time;
            Msg = msg;
        }

    }
}
