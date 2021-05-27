package Service;

import Service.User.Player;

public interface Room {

    void enterRoom(Player p);
    void exitRoom(Player p);
    void prepare(Player p);
    void unPrepare(Player p);
}
