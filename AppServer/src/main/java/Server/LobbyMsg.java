package Server;

import Service.RoomInfo;
import Service.User.User;

import java.util.List;

public class LobbyMsg {
    public List<RoomInfo> roomInfos;
    public List<User> users;

    public List<RoomInfo> getRoomInfos() {
        return roomInfos;
    }

    public List<User> getUsers() {
        return users;
    }

    public void setRoomInfos(List<RoomInfo> roomInfos) {
        this.roomInfos = roomInfos;
    }

    public void setUsers(List<User> users) {
        this.users = users;
    }
}
