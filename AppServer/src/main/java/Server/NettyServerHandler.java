package Server;

import Message.AuthenticationMessage;
import Service.ChatMsg;
import Service.Constant;
import Service.GameMsg;
import Service.RoomInfo;
import Service.User.Player;
import Service.User.User;
import com.google.gson.Gson;
import io.netty.channel.Channel;
import io.netty.channel.ChannelHandler;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.ChannelInboundHandlerAdapter;

import myutil.DESUtil.DesKey;
import org.apache.log4j.Logger;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * 服务端 Channel 实现类，提供对客户端 Channel 建立连接、断开连接、异常时的处理
 */

@ChannelHandler.Sharable
public class NettyServerHandler extends ChannelInboundHandlerAdapter {
	protected static Logger logger = Logger.getLogger(NettyServerHandler.class);
	private static Gson gson = new Gson();
	AtomicInteger num=new AtomicInteger(0);
    private NettyChannelManager channelManager= NettyChannelManager.getInstance();
    //房间管理类
    //NettyRoomChannelManager roomManager=new NettyRoomChannelManager();

	//房间列表
	private ConcurrentHashMap<Integer,CheckerRoom> rooms = new ConcurrentHashMap<Integer, CheckerRoom>();
	//玩家列表
	private ConcurrentHashMap<String, Player>players = new ConcurrentHashMap<>();


	private DesKey TGSKeyV;

	public NettyServerHandler(){
		TGSKeyV = new DesKey();

	}

	@Override
    public void channelActive(ChannelHandlerContext ctx) {
    	
    	System.out.println("有人链接进来,链接总人数："+num.incrementAndGet());
        // 从管理器中添加
        channelManager.add(ctx.channel());
        //channelManager.sendAll("大厅有人进入");
    }
    
    @Override
    public void channelRead(ChannelHandlerContext ctx, Object msg) throws Exception {

		NettyMessage message = (NettyMessage)msg;
		logger.info(String.format("接收到报文Head:[P2P:%d Type:%d State:%d UnCode:%d Length:%d]",
				message.getMessageP2P(),message.getMessageType(),message.getStateCode(),message.getUnEncode(),message.getLength()));

		if(message.getMessageP2P()!=4)
		{
			logger.warn(String.format("收到预期外的发送方报文"));
			//丢弃报文
			return;
		}

		if(message.getMessageType()==0)//用户认证
		{
			// TODO: 2021/6/2 添加登录验证
			User user = gson.fromJson(message.bodyToString(), User.class);
			logger.debug(String.format("用户%s请求登录大厅", user.getUserId()));

			//测试：假定秘钥唯一
			DesKey key = new DesKey(new byte[]{1, 1, 1, 1, 1, 1, 1, 1});
			enterLobby(ctx.channel(), user, key);
			return;
		}

		Player player = players.get(channelManager.findUser(ctx.channel()));

		switch (message.getMessageType())
		{
			case 2://用户请求房间列表
				logger.debug(String.format( "用户%s请求房间消息",player.getUserId()));
				roomInfos(player);
				break;
			case 3://用户创建房间
				logger.debug(String.format( "用户%s请求创建房间",player.getUserId()));
				createRoom(player,gson.fromJson(message.bodyToString(),RoomInfo.class));
				break;
			case 4://用户进入房间
				enterRoom(player,gson.fromJson(message.bodyToString(),RoomInfo.class));
				logger.debug(String.format( "用户%s请求进入房间",player.getUserId()));
				break;
			case 5://用户退出房间
				logger.debug(String.format( "用户%s请求退出房间",player.getUserId()));
				exitRoom(player);
				break;
			case 6://用户准备
				logger.debug(String.format( "用户%s请求准备",player.getUserId()));
				prepare(player);
				break;
			case 7://用户取消准备
				logger.debug(String.format( "用户%s请求取消准备",player.getUserId()));
				unprepare(player);
				break;
			case 8://聊天消息
				logger.debug(String.format( "用户%s发送聊天消息",player.getUserId()));
				chat(player,gson.fromJson(message.bodyToString(),ChatMsg.class));
				break;
			case 9://用户移动
				logger.debug(String.format( "用户%s提交移动结果",player.getUserId()));
				playerMove(player,gson.fromJson(message.bodyToString(),GameMsg.class));
				break;
			case 10://用户退出大厅
				logger.debug(String.format( "用户%s请求退出房间",player.getUserId()));
				exitLobby(player);
				break;
			default:
				//
				break;
		}

    //    ctx.channel().writeAndFlush("i am server !");

//        ctx.writeAndFlush("i am server !").addListener(ChannelFutureListener.CLOSE);
    }

	/**
	 * 进入大厅请求
	 * @param channel
	 * @param user
	 */
	public void enterLobby(Channel channel, User user, DesKey key){
		}


	public void enterLobby(ChannelHandlerContext ctx,NettyMessage message){
		byte[] mes=message.getMessageBody();
		AuthenticationMessage fromClient=new AuthenticationMessage();
		fromClient.CVMEssage(mes,TGSKeyV);

		server.ticket=fromClient.ticket;
		logger.info(String.format("用户%s开始进行验证",fromClient.IDc));
		server.verify(fromClient);

		server.ADc="127.0.0.1";
		if(server.ADc.length()<16){
			for(int i=server.ADc.length();i<16;i++){
				server.ADc+=" ";
			}
		}

		if(server.status==0){
			logger.info(String.format("用户%s验证成功", fromClient.IDc));
			byte[] mess=server.generateBack(fromClient.TS, server.Kcv);
			NettyMessage back=new NettyMessage(5,0,0);
			back.setMessageBody(mess);
			System.out.println("正在发送消息");
			ctx.writeAndFlush(back);

			//
			channelManager.addUser(channel, user.getUserId(),key);
			user.userState = Constant.online;
			players.put(user.getUserId(),new Player(user));

			logger.info(String.format("用户%s进入大厅 || 大厅总人数%d",user.getUserId(),players.size()));



		}
		else{
			//System.out.println(tgs.status);
			NettyMessage back=new NettyMessage(3,0,server.status);
			ctx.writeAndFlush(back);
			logger.info(String.format("用户%s验证失败",fromClient.IDc));
		}

	}


	/**
	 * 返回房间信息请求
	 * @param user
	 */
	public void roomInfos(User user){
		NettyMessage roomInfoMessage = new NettyMessage(5,2,0);

		//json转list
		//List<RoomInfo> list= gson.fromJson(msg, new TypeToken<List<RoomInfo>>() {}.getType());
		LobbyMsg msg = new LobbyMsg();
		List<RoomInfo> list  = new ArrayList<>();
		for(Integer roomId : rooms.keySet()){
			list.add(rooms.get(roomId).getRefreashInfo());
		}
		msg.setRoomInfos(list);
		List<User> ulist = new ArrayList<>();
		for(String u : players.keySet()){
			ulist.add(players.get(u));
		}
		msg.setUsers(ulist);
		roomInfoMessage.setMessageBody(gson.toJson(msg));
		channelManager.send(user.getUserId(), roomInfoMessage);
	}

	/**
	 * 进入房间请求
	 * @param p
	 * @param roomInfo
	 */
	public void enterRoom(Player p, RoomInfo roomInfo){

		NettyMessage response = new NettyMessage(5,4,0);

		CheckerRoom room = null;
		if(!rooms.containsKey(roomInfo.getRoomId())){
			//房间已经不存在
			response.setStateCode(1);
		}
		else{
			room = rooms.get(roomInfo.getRoomId());
			if(room.getRoomInfo().isPlayerFull()){
				//房间人满了
				response.setStateCode(2);
			}
			else if(room.getRoomInfo().getRoomState()==1)
			{
				//房间已经开始了
				response.setStateCode(3);
			}
			else{
				//channelManager.send(user.getUserId(), response);
				//上锁
				p.userState = Constant.enter_room;
				room.enterRoom(p);
				logger.debug(String.format("用户%s进入房间%d成功",p.getUserId(),roomInfo.getRoomId()));

				return;
			}
		}
		logger.debug(String.format("用户%s进入房间%d失败",p.getUserId(),roomInfo.getRoomId()));

		channelManager.send(p.getUserId(), response);

	}

	/**
	 * 创建房间请求
	 * @param p
	 * @param roomInfo
	 */
	public void createRoom(Player p, RoomInfo roomInfo){
		int i;
		for ( i = 0; i < 9999; i++) {
			if(!rooms.containsKey(i)){
				//已设置psw,hasPsw,maxPlayer,playerNum
				roomInfo.setRoomId(i);
				roomInfo.setRoomState(0);

				rooms.put(i,new CheckerRoom(channelManager,roomInfo));
				break;
			}
		}
		//回复创建结果
		NettyMessage response = new NettyMessage(5,3,0);
		channelManager.send(p.getUserId(), response);

		try {
			p.userState = Constant.enter_room;
			//调用加入房间
			rooms.get(i).enterRoom(p);
		}
		catch (Exception e)
		{
			e.printStackTrace();
			logger.error("无法加入！");
		}
	}

	/**
	 * 尝试删除房间 无人删除
	 * @param roomId
	 */
	private void deleteRoom(int roomId)
	{
		logger.debug(String.format( "房间%d正在删除",roomId));
		rooms.remove(roomId);
	}

	public void exitRoom(Player p){
		if(p==null||p.roomId==-1) {
			//退出请求有误！
			logger.error(String.format( "用户%s退出请求有误！",p.getUserId()));
			return;
		}
		//要先保存房间号
		int roomId = p.roomId;
		//用户退出
		rooms.get(roomId).exitRoom(p);
		p.userState = Constant.online;

		//检测房间是否为空
		if(rooms.get(roomId).getRoomInfo().getPlayerNum() ==0)
		{
			logger.info(String.format( "房间%d为空,准备删除",roomId));
			//删除房间
			deleteRoom(roomId);
		}
	}

	public void unprepare(Player p){
		if(p==null||p.roomId==-1) {
			//退出请求有误！
			logger.error(String.format( "用户%s取消准备请求有误！",p.getUserId()));
			return;
		}
		rooms.get(p.roomId).unPrepare(p);
	}

	public void prepare(Player p){
		if(p==null||p.roomId==-1) {
			//退出请求有误！
			logger.error(String.format( "用户%s准备请求有误！",p.getUserId()));
			return;
		}
		rooms.get(p.roomId).prepare(p);
	}

	/**
	 * 聊天
	 * @param user
	 * @param msg
	 */
	public void chat(Player user, ChatMsg msg)
	{
		NettyMessage message = new NettyMessage(5,5,0);
		message.setMessageBody(gson.toJson(msg));
		if(msg.getType()==1)
		{//大厅聊天
			logger.info(String.format("用户%s大厅消息发送！",user.getUserId()));
			channelManager.sendOther(user.getUserId(),message);
		}
		else if(msg.getType()==2)
		{//房间聊天
			rooms.get(user.roomId).msgHandle(user.getUserId(),message);
		}
	}

	public void playerMove(Player p, GameMsg msg)
	{
		if(msg.gameState==3)
		{
			rooms.get(p.roomId).playerMove(p,msg);
		}
		else if(msg.gameState==4)
		{
			rooms.get(p.roomId).playerSkip(p,msg);

		}
		else{
			logger.error("用户移动报文出错！");
		}
	}

	public void exitLobby(Player p){

	}

	@Override
    public void channelUnregistered(ChannelHandlerContext ctx) {
		String uid = channelManager.findUser(ctx.channel());

		if(players.get(uid).userState== Constant.in_room)
		{//在房间中
			// TODO: 2021/5/13 在房间中退出
		}

		// 从管理器中移除
		channelManager.remove(ctx.channel());
		players.remove(uid);

		logger.info(String.format( "用户%s退出大厅 || 当前大厅总人数:%d", uid, players.size()));
	}

    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
        // 断开连接
        ctx.channel().close();
    }

}
