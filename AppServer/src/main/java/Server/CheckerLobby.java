package Server;

import Service.Lobby;
import Service.Room;
import Service.User.Player;
import org.apache.log4j.Logger;


import java.util.ArrayList;
import java.util.List;

public class CheckerLobby extends Thread implements Lobby {
    private static Logger logger = Logger.getLogger(CheckerLobby.class);

    private List<Player> players = null;
    private List<Room> rooms = null;

    private NettyChannelManager ncm;

    public NettyChannelManager getNcm() {
        return ncm;
    }

    public CheckerLobby(){
        ncm = new NettyChannelManager();
        players = new ArrayList<>();
        rooms = new ArrayList<>();
    }

    @Override
    public void run() {
        logger.info("大厅开启！");
    }

    @Override
    public void enterLobby(Player p) {
        logger.info("玩家进入大厅！");
        players.add(p);
    }

    @Override
    public void exitLobby(Player p) {
        logger.info("退出大厅！");

    }

    @Override
    public void getRoomInfo(Player p) {
        logger.info("获取房间信息！");

    }

    @Override
    public void enterRoom(Player p, Room room, String psw) {
        logger.info("玩家进入房间！");

    }

    @Override
    public void createRoom(Player p, String roomInfo) {
        logger.info("创建房间！");

    }

    @Override
    public void deleteRoom(Room room) {
        logger.info("房间删除！");

    }

    @Override
    public void exitRoom(Player p, Room room) {
        logger.info("玩家退出房间！");

    }


}
