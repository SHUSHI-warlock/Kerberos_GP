using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.core.Services
{
    interface ILobby
    {
        void enterLobby(Player p);
        void exitLobby(Player p);
        void getRoomInfo(Player p);
        void enterRoom(Player p, IRoom room);
        void exitRoom(Player p, IRoom room);
        void createRoom(Player p, RoomInfo roomInfo);
        void deleteRoom(IRoom room);
        
    }
}
