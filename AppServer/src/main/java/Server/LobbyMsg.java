package Server;

import Service.RoomInfo;
import Service.User.User;

import java.util.List;

/**
 * 大厅消息结构体，使用json转换
 * <p>成员：</p>
 *      <p>List &lt;RoomInfo> roomInfos</p>
 *      <p>List &lt;User> users</p>
 */
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
