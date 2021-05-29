package Service;

import Service.User.Player;
import org.apache.log4j.Logger;

public class RoomInfo {
    protected static Logger logger = Logger.getLogger(RoomInfo.class);

    private int RoomId;
    private String RoomName;
    private String RoomPsw;
    private int RoomMaxPlayer;
    private int HasPsw;
    private Player[] players;
    private int PlayerNum;
    private int RoomState;

    /*setter*/
    public void setRoomState(int state)
    {
        RoomState = state;
    }
    public void setRoomId(int roomId) {
        this.RoomId = roomId;
    }
    public void setRoomPsw(String roomPsw) {
        this.RoomPsw = roomPsw;
    }

    /*getter*/
    public String getRoomPsw() {
        return RoomPsw;
    }
    public String getRoomName() {
        return RoomName;
    }
    public int getRoomState() {
        return RoomState;
    }
    public int getRoomId() {
        return RoomId;
    }
    public Player[] getPlayers() { return players; }
    public int getRoomMaxPlayer() {
        return RoomMaxPlayer;
    }
    public int getPlayerNum() {
        return PlayerNum;
    }
    public int getHasPsw(){return HasPsw;}


    public void addPlayer(Player p){
        if(players==null)
            players = new Player[7];
        if(PlayerNum == RoomMaxPlayer) {
            logger.error("房间已满但是还在加人！");
            return;
        }
        players[p.pos]=p;
        PlayerNum++;
    }
    public void removePlayer(Player p){
        if(players==null|| PlayerNum ==0)
            return;
        players[p.pos]=null;
        PlayerNum--;
    }

    public RoomInfo()
    {
        //players = new ArrayList<>();
        PlayerNum = 0;
    }
    //发送创建房间信息
    public RoomInfo(String name,String Psw,int maxPlayer)
    {
        RoomName = name;
        RoomPsw = "";
        if(Psw.equals(""))
            HasPsw = -1;
        else{
            HasPsw = 1;
            RoomPsw = Psw;
        }
        RoomMaxPlayer = maxPlayer;
        PlayerNum = 0;
    }

    //加入房间消息
    public RoomInfo(int id,String psw){
        this.RoomId = id;
        RoomPsw = psw;
    }

    public RoomInfo(int id,String name,int maxPlayer,int players,int hasPsw,int state){
        //this();
        this.RoomName = name;
        this.HasPsw = hasPsw;
        this.RoomMaxPlayer = maxPlayer;
        this.RoomId = id;
        this.PlayerNum = players;
        this.RoomState = state;
    }

    public boolean isPlayerFull()
    {
        return PlayerNum == RoomMaxPlayer;
    }

   }
