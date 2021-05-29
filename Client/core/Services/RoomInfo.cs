using Client.Utils.LogHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.core.Services
{
    public class RoomInfo : INotifyPropertyChanged
    {
        private static Logger logger = Logger.GetLogger();
        
        public Player[] players { get; set; }

        private int roomId;
        private String roomName;
        private String roomPsw;
        private int roomMaxPlayer;
        private int hasPsw;
        private int playerNum;
        private int roomState;

        public int RoomId
        {
            get { return roomId; }
            set
            {
                roomId = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RoomId"));
            }
        }
        public string RoomName
        {
            get { return roomName; }
            set
            {
                roomName = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RoomName"));
            }
        }
        public string RoomPsw
        {
            get { return roomPsw; }
            set { roomPsw = value; }
        }
        public int HasPsw
        {
            get { return hasPsw; }
            set
            {
                hasPsw = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("HasPsw"));
            }
        }
        public int PlayerNum
        {
            get { return playerNum; }
            set
            {
                playerNum = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("PlayerNum"));
            }
        }
        public int RoomMaxPlayer
        {
            get { return roomMaxPlayer; }
            set
            {
                roomMaxPlayer = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RoomMaxPlayer"));
            }
        }
        /// <summary>
        /// 房间状态
        /// 0是未开始 1是已开始
        /// </summary>
        public int RoomState
        {
            get { return roomState; }
            set
            {
                roomState = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RoomState"));
            }
        }

        /// <summary>
        /// 加一个玩家
        /// </summary>
        /// <param name="p"></param>
        public void addPlayer(Player p)
        {
            if (players == null)
                players = new Player[7];
            if(p.pos<1||p.pos>6)
            {
                logger.Error("用户添加时没有指定位置！");
                return;
            }
            if(PlayerNum==RoomMaxPlayer)
            {
                logger.Error("房间人数已达上限！");
                return;
            }
            players[p.pos]=p;
            PlayerNum++;
        }
        /// <summary>
        /// 删一个玩家
        /// </summary>
        /// <param name="p"></param>
        public void removePlayer(Player p)
        {
            if (players == null)
            {
                logger.Error("玩家列表未初始化！");
                return;
            }
            if (p.pos < 1 || p.pos > 6||players[p.pos]==null)
            {
                logger.Error("用户删除时没有指定位置！");
                return;
            }
            if (PlayerNum == 0)
            {
                logger.Error("房间人数为0！");
                return;
            }
            players[p.pos]=null;
            playerNum --;
        }

        public RoomInfo()
        {
            //players = new Player[7];
            //RoomMaxPlayer = 2;
            //PlayerNum = 0;
        }
        /// <summary>
        /// 创建房间 构造函数
        /// </summary>
        /// <param name="name">房间名</param>
        /// <param name="Psw">密码，为""表示无密码</param>
        /// <param name="maxPlayer">最大人数</param>
        public RoomInfo(String name, String Psw, int maxPlayer)
        {
            roomName = name;
            roomPsw = "";
            if (Psw=="")
                hasPsw = -1;
            else
            {
                hasPsw = 1;
                roomPsw = Psw;
            }
            roomMaxPlayer = maxPlayer;
            playerNum = 0;
        }

        /// <summary>
        /// 加入房间 构造函数
        /// </summary>
        /// <param name="id">房间号</param>
        /// <param name="psw">密码</param>
        public RoomInfo(int id, String psw)
        {
            this.roomId = id;
            roomPsw = psw;
        }
        
        /// <summary>
        /// 显示房间信息 构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="maxPlayer"></param>
        /// <param name="players"></param>
        /// <param name="hasPsw"></param>
        /// <param name="state"></param>
        public RoomInfo(int id, String name, int maxPlayer, int players, int hasPsw, int state)
        {
            this.roomName = name;
            this.hasPsw = hasPsw;
            this.roomMaxPlayer = maxPlayer;
            this.roomId = id;
            this.playerNum = players;
            this.roomState = state;
        }

        /// <summary>
        /// 房间是否满人
        /// </summary>
        /// <returns></returns>
        public bool isFull()
        {
            return playerNum == roomMaxPlayer;
        }
        
        /// <summary>
        /// 房间是否无人
        /// </summary>
        /// <returns></returns>
        public bool isEmpty()
        {
            return playerNum == 0;
        }


        //生成报文
        public byte[] getRoomInfo(int way)
        {
            byte[] temp = null;
            /**
            if (way == 1)
            {//chuangeiliebiaode1
                temp = new byte[20];
                System.arraycopy(ByteTransUtil.intToByteArray(roomId), 0, temp, 0, 4);
                System.arraycopy(ByteTransUtil.intToByteArray(roomMaxPlayer), 0, temp, 4, 4);
                System.arraycopy(ByteTransUtil.intToByteArray(roomStatus), 0, temp, 8, 4);
                System.arraycopy(ByteTransUtil.intToByteArray(players.size()), 0, temp, 12, 4);
                System.arraycopy(ByteTransUtil.intToByteArray(hasPsw), 0, temp, 16, 4);

            }
            else if (way == 2)
            {
                temp = new byte[8 + players.size() * 32];
                System.arraycopy(ByteTransUtil.intToByteArray(roomStatus), 0, temp, 0, 4);
                System.arraycopy(ByteTransUtil.intToByteArray(players.size()), 0, temp, 4, 4);

                for (int i = 0; i < players.size(); i++)
                {
                    byte[] s = players.get(i).getUserId().getBytes(StandardCharsets.UTF_8);
                    if (s.length > 20 || s.length == 0)
                        return null;
                    int base = 8 + 32 * i;
                    System.arraycopy(s, 0, temp, base, s.length);
                    System.arraycopy(ByteTransUtil.intToByteArray(players.get(i).prepare), 0, temp, base + 4, 4);
                    System.arraycopy(ByteTransUtil.intToByteArray(players.get(i).steps), 0, temp, base + 8, 4);
                    System.arraycopy(ByteTransUtil.intToByteArray(players.get(i).roomId), 0, temp, base + 12, 4);
                }
            }
            else if (way == 3)
            {
                temp = new byte[28];
                System.arraycopy(ByteTransUtil.intToByteArray(roomId), 0, temp, 0, 4);
                System.arraycopy(ByteTransUtil.intToByteArray(roomMaxPlayer), 0, temp, 4, 4);
                System.arraycopy(ByteTransUtil.intToByteArray(hasPsw), 0, temp, 8, 4);
                byte[] s = roomPsw.getBytes(StandardCharsets.UTF_8);
                if (s.length > 20 || s.length == 0)
                    return null;
                System.arraycopy(s, 0, temp, 12, s.length);
            }
            */
            return temp;
        }

        /**
         * 从byte中解析info
         * @param msg
         * @param way
         */
        public void praseRoomInfo(byte[] msg, int way)
        {
            /**
            if (way == 1 && msg.length == 20)
            {//chuangeiliebiaode1
                byte[] temp = new byte[4];

                System.arraycopy(msg, 0, temp, 0, 4);
                roomId = ByteTransUtil.byteArrayToInt(temp);
                System.arraycopy(msg, 4, temp, 0, 4);
                roomMaxPlayer = ByteTransUtil.byteArrayToInt(temp);
                System.arraycopy(msg, 8, temp, 0, 4);
                roomStatus = ByteTransUtil.byteArrayToInt(temp);
                System.arraycopy(msg, 12, temp, 0, 4);
                playerNum = ByteTransUtil.byteArrayToInt(temp);
                System.arraycopy(msg, 16, temp, 0, 4);
                hasPsw = ByteTransUtil.byteArrayToInt(temp);
            }
            else if (way == 2 && msg.length >= 8)
            {
                byte[] temp = new byte[4];
                System.arraycopy(msg, 0, temp, 0, 4);
                roomStatus = ByteTransUtil.byteArrayToInt(temp);
                System.arraycopy(msg, 4, temp, 0, 4);
                playerNum = ByteTransUtil.byteArrayToInt(temp);

                if (playerNum < 0 || msg.length != 8 + playerNum * 32)
                    return;

                players.clear();

                for (int i = 0; i < playerNum; i++)
                {
                    int base = 8 + 32 * i;
                    byte[] s = new byte[20];
                    System.arraycopy(msg, base, s, 0, 20);
                    Player p = new Player(new String(s));
                    System.arraycopy(msg, base + 20, temp, 0, 4);
                    p.prepare = ByteTransUtil.byteArrayToInt(temp);
                    System.arraycopy(msg, base + 24, temp, 0, 4);
                    p.steps = ByteTransUtil.byteArrayToInt(temp);
                    System.arraycopy(msg, base + 28, temp, 0, 4);
                    p.roomId = ByteTransUtil.byteArrayToInt(temp);

                }
            }
            else if (way == 3 && msg.length == 28)
            {
                byte[] temp = new byte[4];
                System.arraycopy(msg, 0, temp, 0, 4);
                roomId = ByteTransUtil.byteArrayToInt(temp);
                System.arraycopy(msg, 4, temp, 0, 4);
                roomMaxPlayer = ByteTransUtil.byteArrayToInt(temp);
                System.arraycopy(msg, 8, temp, 0, 4);
                hasPsw = ByteTransUtil.byteArrayToInt(temp);

                byte[] s = new byte[20];
                System.arraycopy(msg, 8, s, 0, 20);
                roomPsw = new String(s);
            }
            */
        }


        //绑定更新事件
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string _property)
        {
            PropertyChangedEventHandler eventhandler = this.PropertyChanged;
            if (null == eventhandler)
                return;
            eventhandler(this, new PropertyChangedEventArgs(_property));
        }
    }
}
