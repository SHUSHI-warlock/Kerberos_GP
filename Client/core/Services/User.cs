using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.core.Services
{
    public class User
    {
        public string userId { get; set; }
        public UserState userState { get; set; }

        public User() { }

        public User(string user)
        {
            this.userId = user;
            userState = UserState.offline;
        }

        public User(string user,UserState state)
        {
            this.userId = user;
            userState = state;
        }
    }
    public enum UserState {
        offline = 0,        //离线
        online = 1,         //在线，大厅中
        enter_room = 2,     //进入房间中
        exit_rome = 3,      //退出房间中
        in_room = 4,        //在房间中
        prepared = 5,       //准备
        unprepared = 6,     //未准备
        move = 7,
        wait = 8,
        complete=9
}

}
