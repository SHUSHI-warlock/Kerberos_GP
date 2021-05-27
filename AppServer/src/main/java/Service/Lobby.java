package Service;


import Service.User.Player;

public interface Lobby {

    void enterLobby(Player p);
    void exitLobby(Player p);
    void getRoomInfo(Player p);
    void enterRoom(Player p,Room room,String psw);
    void exitRoom(Player p,Room room);
    void createRoom(Player p,String roomInfo);
    void deleteRoom(Room room);

}
