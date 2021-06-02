package Server;

import Service.*;
import Service.User.Player;
import com.google.gson.Gson;
import org.apache.log4j.Logger;

public class CheckerRoom implements Room {
    private static org.apache.log4j.Logger logger = Logger.getLogger(CheckerRoom.class);
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
        this.roomInfo.setRoomState(0); //未开始
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
        NettyMessage playerStateMessage = new NettyMessage(5,6,0);
        playerStateMessage.setMessageBody(gson.toJson(p));

        sendOthers(p.getUserId(),playerStateMessage);

        // TODO: 2021/5/13 返回房间详细信息+用户信息
        NettyMessage roomMessage = new NettyMessage(5,4,0 );
        roomMessage.setMessageBody(gson.toJson(roomInfo));

        send2Player(p.getUserId(),roomMessage);

        p.userState = Constant.unprepared;
    }

    @Override
    public void exitRoom(Player p) {
        logger.info(String.format("用户%s退出房间%d",p.getUserId(),roomInfo.getRoomId()));

        p.userState = Constant.exit_rome;
        //向他自己也发送
        NettyMessage playerStateMessage = new NettyMessage(5,6,0);
        playerStateMessage.setMessageBody(gson.toJson(p));
        sendAll(playerStateMessage);

        roomInfo.removePlayer(p);
        playerPos[p.pos]=false;
        p.roomId = -1;
        p.userState = Constant.online;
    }

    @Override
    public void prepare(Player p) {
        if(p.userState == Constant.unprepared)
        {
            p.userState = Constant.prepared;
            NettyMessage playerStateMessage = new NettyMessage(5,6,0);
            playerStateMessage.setMessageBody(gson.toJson(p));
            sendOthers(p.getUserId(),playerStateMessage);
        }
        //判断是否开始了
        if(CanStart())
        {
            new Thread( new Runnable() {
                public void run(){
                    logger.debug(String.format("房间%d开始游戏！",roomInfo.getRoomId()));
                    roomInfo.setRoomState(1);

                    game.startGame(roomInfo.getPlayerNum());
                    int curr = game.getCurPlayer();

                    NettyMessage gameMessage = new NettyMessage(5,7,0);
                    //游戏开始时发送第一个移动玩家
                    GameMsg gameMsg = new GameMsg(0,curr);
                    gameMessage.setMessageBody(gson.toJson(gameMsg));
                    sendAll(gameMessage);
                    }
            }).start();
        }
    }

    @Override
    public void unPrepare(Player p) {
        if(p.userState == Constant.prepared)
        {
            p.userState = Constant.unprepared;
            NettyMessage playerStateMessage = new NettyMessage(5,6,0);
            playerStateMessage.setMessageBody(gson.toJson(p));
            sendOthers(p.getUserId(),playerStateMessage);
        }
    }

    public void msgHandle(String user,NettyMessage m)
    {
        logger.info(String.format("用户%s房间消息发送！",user));
        sendOthers(user,m);
    }

    private boolean CanStart()
    {
        if (roomInfo.getPlayerNum() == roomInfo.getRoomMaxPlayer())
        {
            for(Player p : roomInfo.getPlayers())
            if (p != null && p.userState == Constant.unprepared)
                return false;
            return true;
        }
        return false;
    }

    /**
     * 玩家提交移动
     * @param p
     * @param msg
     */
    public void playerMove(Player p,GameMsg msg)
    {
        NettyMessage backMessage = new NettyMessage(5,8,0);
        logger.info(String.format("%d号玩家提交移动结果！",msg.pos));

        int res = game.playerMove(msg);
        if(res==0) {
            logger.info(String.format("%d号玩家%s提交移动成功",msg.pos,p.getUserId()));
            send2Player(p.getUserId(),backMessage);
            //广播给其他玩家
            NettyMessage broadcastMsg = new NettyMessage(5,7,0);
            broadcastMsg.setMessageBody(gson.toJson(msg));
            sendOthers(p.getUserId(),broadcastMsg);

            //判断游戏是否结束
            if(game.isGameOver()){
                logger.info(String.format("房间游戏结束！"));
                NettyMessage bMsg = new NettyMessage(5,7,0);
                GameMsg gameMsg = new GameMsg(1,-1);
                bMsg.setMessageBody(gson.toJson(gameMsg));
                sendAll(bMsg);
                //游戏结束
                roomInfo.setRoomState(0);
            }
            else {//计算下一个移动玩家
                int nextPos = game.nextMove();
                logger.info(String.format("下一个移动的玩家为%d号玩家！",nextPos));
                ///不通知下一个玩家
//                NettyMessage bMsg = new NettyMessage(5,7,0);
//                GameMsg gameMsg = new GameMsg(2,nextPos);
//                bMsg.setMessageBody(gson.toJson(gameMsg));
//                sendAll(bMsg);
            }
        }
        else if(res==1) {
            logger.warn(String.format("当前不是%d号玩家%s的移动回合",msg.pos,p.getUserId()));
            backMessage.setStateCode(1);
            send2Player(p.getUserId(),backMessage);
        }
        else {
            logger.error(String.format("%d号玩家%s的移动无效！",msg.pos,p.getUserId()));
            backMessage.setStateCode(2);
            send2Player(p.getUserId(),backMessage);
        }
    }

    /**
     * 玩家跳过回合
     * @param p
     */
    public void playerSkip(Player p,GameMsg msg) {
        logger.info(String.format("%d号玩家跳过了移动回合！",msg.pos));
        //广播给其他玩家
        NettyMessage broadcastMsg = new NettyMessage(5,7,0);
        broadcastMsg.setMessageBody(gson.toJson(msg));
        sendOthers(p.getUserId(), broadcastMsg);

        int nextPos = game.nextMove();
        logger.info(String.format("下一个移动的玩家为%d号玩家！",nextPos));
//        NettyMessage bMsg = new NettyMessage(5,7,0);
//        GameMsg gameMsg = new GameMsg(2,nextPos);
//        bMsg.setMessageBody(gson.toJson(gameMsg));
//        sendAll(bMsg);
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
