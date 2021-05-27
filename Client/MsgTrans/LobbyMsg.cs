using Client.core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MsgTrans
{
    public class LobbyMsg
    {
        public LobbyMsg() {}
        public LobbyMsg(List<RoomInfo> roomInfos,List<User> users)
        {
            this.roomInfos = roomInfos;
            this.users = users;
        }
        public List<RoomInfo> roomInfos { get; set; }
        public List<User> users { get; set; }

    }
}
