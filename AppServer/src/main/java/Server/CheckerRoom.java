package Server;

import Service.CheckerCore;
import Service.Constant;
import Service.Room;
import Service.RoomInfo;
import Service.User.Player;
import com.google.gson.Gson;


public class CheckerRoom implements Room {
    private NettyChannelManager nettyChannelManager;
    private static Gson gson = new Gson();

    private CheckerCore game;

    private RoomInfo roomInfo;

    //记录玩家位置信息
    private boolean[] playerPos;
    private static final int[] playerIndex = {0,1,4,2,5,3,6};


    public CheckerRoom(NettyChannelManager nettyChannelManager, RoomInfo roomInfo)
    {
        game = new CheckerCore();
        this.roomInfo = roomInfo;
        this.roomInfo.setRoomState(1); //未开始
        this.nettyChannelManager = nettyChannelManager;

        playerPos = new boolean[7];
        for(int i=1;i<7;i++)
            playerPos[i]=false;
    }

    /**
     * 根据发送的报文来选择屏蔽一些信息
     * @return
     */
    public RoomInfo getRoomInfo() {
            return roomInfo;
    }

    public RoomInfo getRefreashInfo(){
        return new RoomInfo(roomInfo.getRoomId(), roomInfo.getRoomName(), roomInfo.getRoomMaxPlayer(),
                roomInfo.getPlayerNum(), roomInfo.getHasPsw(), roomInfo.getRoomState());
        //发送刷新的消息
    }

    @Override
    public void enterRoom(Player p) {

        p.roomId = roomInfo.getRoomId();
        //分配位置
        for(int i = 1; i<= roomInfo.getRoomMaxPlayer(); i++)
        {
            if(!playerPos[playerIndex[i]]) {
                playerPos[playerIndex[i]] = true;
                p.pos = playerIndex[i];
                break;
            }
        }
        roomInfo.addPlayer(p);

        // TODO: 2021/5/13 通知其他玩家有人进入房间
        NettyMessage playerStateMessage = new NettyMessage(2,6,0);
        playerStateMessage.setMessageBody(gson.toJson(p));

        sendOthers(p.getUserId(),playerStateMessage);

        // TODO: 2021/5/13 返回房间详细信息+用户信息
        NettyMessage roomMessage = new NettyMessage(2,4,0 );
        roomMessage.setMessageBody(gson.toJson(roomInfo));

        send2Player(p.getUserId(),roomMessage);

        p.userState = Constant.unprepared;
    }

    @Override
    public void exitRoom(Player p) {
        p.userState = Constant.exit_rome;

        roomInfo.removePlayer(p);
        playerPos[p.pos]=false;
        p.roomId = -1;

        NettyMessage playerStateMessage = new NettyMessage(2,6,0);
        playerStateMessage.setMessageBody(gson.toJson(p));

        sendAll(playerStateMessage);
    }

    @Override
    public void prepare(Player p) {
        if(p.userState == Constant.unprepared)
        {
            p.userState = Constant.prepared;
            NettyMessage playerStateMessage = new NettyMessage(2,6,0);
            playerStateMessage.setMessageBody(gson.toJson(p));

            sendOthers(p.getUserId(),playerStateMessage);
        }

    }

    @Override
    public void unPrepare(Player p) {
        if(p.userState == Constant.prepared)
        {
            p.userState = Constant.unprepared;
            NettyMessage playerStateMessage = new NettyMessage(2,6,0);
            playerStateMessage.setMessageBody(gson.toJson(p));
            sendOthers(p.getUserId(),playerStateMessage);
        }
    }

    private void send2Player(String id, NettyMessage message){
        nettyChannelManager.send(id,message);
    }
    private void sendAll(NettyMessage message){
        for(Player p : roomInfo.getPlayers()){
            if(p!=null)
                nettyChannelManager.send(p.getUserId(),message);
        }
    }

    private void sendOthers(String id,NettyMessage message){
        for(Player p : roomInfo.getPlayers()){
            if(p!=null&&!p.getUserId().equals(id))
                nettyChannelManager.send(p.getUserId(),message);
        }
    }

}
