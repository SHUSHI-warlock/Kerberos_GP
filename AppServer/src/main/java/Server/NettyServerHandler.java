package Server;

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

import org.apache.log4j.Logger;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * ����� Channel ʵ���࣬�ṩ�Կͻ��� Channel �������ӡ��Ͽ����ӡ��쳣ʱ�Ĵ���
 */

@ChannelHandler.Sharable
public class NettyServerHandler extends ChannelInboundHandlerAdapter {
	protected static Logger logger = Logger.getLogger(NettyServerHandler.class);
	private static Gson gson = new Gson();
	AtomicInteger num=new AtomicInteger(0);
    private NettyChannelManager channelManager=new NettyChannelManager();
    //���������
    //NettyRoomChannelManager roomManager=new NettyRoomChannelManager();

	//�����б�
	private ConcurrentHashMap<Integer,CheckerRoom> rooms = new ConcurrentHashMap<Integer, CheckerRoom>();
	//����б�
	private ConcurrentHashMap<String, Player>players = new ConcurrentHashMap<>();

	@Override
    public void channelActive(ChannelHandlerContext ctx) {
    	
    	System.out.println("�������ӽ���,������������"+num.incrementAndGet());
        // �ӹ����������
        channelManager.add(ctx.channel());
        //channelManager.sendAll("�������˽���");
    }
    
    @Override
    public void channelRead(ChannelHandlerContext ctx, Object msg) throws Exception {

		NettyMessage message = (NettyMessage)msg;
		logger.info(String.format("���յ�����Head:[P2P:%d Type:%d State:%d UnCode:%d Length:%d]",
				message.getMessageP2P(),message.getMessageType(),message.getStateCode(),message.getUnEncode(),message.getLength()));

		if(message.getMessageP2P()!=1)
		{
			//��������
			return;
		}

		if(message.getMessageType()==1)//�û���¼����
		{
			User user = gson.fromJson(message.bodyToString(),User.class);
			enterLobby(ctx.channel(), user);
			return;
		}

		Player player = players.get(channelManager.findUser(ctx.channel()));

		switch (message.getMessageType())
		{
			case 2://�û����󷿼��б�
				roomInfos(player);
				break;
			case 3://�û���������
				createRoom(player,gson.fromJson(message.bodyToString(),RoomInfo.class));
				break;
			case 4://�û����뷿��
				enterRoom(player,gson.fromJson(message.bodyToString(),RoomInfo.class));
				break;
			case 5://�û��˳�����
				exitRoom(player);
				break;
			case 6://�û�׼��
				prepare(player);
				break;
			case 7://�û�ȡ��׼��
				unprepare(player);
				break;
			case 8://������Ϣ
				chat(player,gson.fromJson(message.bodyToString(),ChatMsg.class));
				break;
			case 9://�û��ƶ�
				playerMove(player,gson.fromJson(message.bodyToString(),GameMsg.class));
				break;
			case 10://�û��˳�����
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
	 * �����������
	 * @param channel
	 * @param user
	 */
    public void enterLobby(Channel channel, User user){
		channelManager.addUser(channel, user.getUserId());
		user.userState = Constant.online;
		players.put(user.getUserId(),new Player(user));

		logger.info(String.format("�û�%s������� || ����������%d",user.getUserId(),players.size()));

		channelManager.send(user.getUserId(), new NettyMessage(2,1,0));
	}

	/**
	 * ���ط�����Ϣ����
	 * @param user
	 */
	public void roomInfos(User user){
		logger.debug(String.format( "�û�%s���󷿼���Ϣ",user.getUserId()));
		NettyMessage roomInfoMessage = new NettyMessage(2,2,0);

		//jsonתlist
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
	 * ���뷿������
	 * @param p
	 * @param roomInfo
	 */
	public void enterRoom(Player p, RoomInfo roomInfo){

		NettyMessage response = new NettyMessage(2,4,0);

		CheckerRoom room = null;
		if(!rooms.containsKey(roomInfo.getRoomId())){
			//�����Ѿ�������
			response.setStateCode(1);
		}
		else{
			room = rooms.get(roomInfo.getRoomId());
			if(room.getRoomInfo().isPlayerFull()){
				//����������
				response.setStateCode(2);
			}
			else if(room.getRoomInfo().getRoomState()==1)
			{
				//�����Ѿ���ʼ��
				response.setStateCode(3);
			}
			else{
				//channelManager.send(user.getUserId(), response);
				//����
				p.userState = Constant.enter_room;
				room.enterRoom(p);
				logger.debug(String.format("�û�%s���뷿��%d�ɹ�",p.getUserId(),roomInfo.getRoomId()));

				return;
			}
		}
		logger.debug(String.format("�û�%s���뷿��%dʧ��",p.getUserId(),roomInfo.getRoomId()));

		channelManager.send(p.getUserId(), response);

	}

	/**
	 * ������������
	 * @param p
	 * @param roomInfo
	 */
	public void createRoom(Player p, RoomInfo roomInfo){
		int i;
		for ( i = 0; i < 9999; i++) {
			if(!rooms.containsKey(i)){
				//������psw,hasPsw,maxPlayer,playerNum
				roomInfo.setRoomId(i);
				roomInfo.setRoomState(0);

				rooms.put(i,new CheckerRoom(channelManager,roomInfo));
				break;
			}
		}
		//�ظ��������
		NettyMessage response = new NettyMessage(2,3,0);
		channelManager.send(p.getUserId(), response);

		try {
			p.userState = Constant.enter_room;
			//���ü��뷿��
			rooms.get(i).enterRoom(p);
		}
		catch (Exception e)
		{
			e.printStackTrace();
			logger.error("�޷����룡");
		}
	}

	/**
	 * ����ɾ������ ����ɾ��
	 * @param roomId
	 */
	private void deleteRoom(int roomId)
	{
		logger.debug(String.format( "����%d����ɾ��",roomId));
		rooms.remove(roomId);
	}

	public void exitRoom(Player p){
		if(p==null||p.roomId==-1) {
			//�˳���������
			logger.error(String.format( "�û�%s�˳���������",p.getUserId()));
			return;
		}
		//Ҫ�ȱ��淿���
		int roomId = p.roomId;
		//�û��˳�
		rooms.get(roomId).exitRoom(p);
		p.userState = Constant.online;

		//��ⷿ���Ƿ�Ϊ��
		if(rooms.get(roomId).getRoomInfo().getPlayerNum() ==0)
		{
			logger.info(String.format( "����%dΪ��",roomId));
			//ɾ������
			deleteRoom(roomId);
		}
	}

	public void unprepare(Player p){
		if(p==null||p.roomId==-1) {
			//�˳���������
			logger.error(String.format( "�û�%sȡ��׼����������",p.getUserId()));
			return;
		}
		rooms.get(p.roomId).unPrepare(p);
	}

	public void prepare(Player p){
		if(p==null||p.roomId==-1) {
			//�˳���������
			logger.error(String.format( "�û�%s׼����������",p.getUserId()));
			return;
		}
		rooms.get(p.roomId).prepare(p);
	}

	/**
	 * ����
	 * @param user
	 * @param msg
	 */
	public void chat(Player user, ChatMsg msg)
	{
		NettyMessage message = new NettyMessage(2,5,0);
		message.setMessageBody(gson.toJson(msg));
		if(msg.getType()==1)
		{//��������
			logger.info(String.format("�û�%s������Ϣ���ͣ�",user.getUserId()));
			channelManager.sendOther(user.getUserId(),message);
		}
		else if(msg.getType()==2)
		{//��������
			rooms.get(user.roomId).msgHandle(user.getUserId(),message);
		}
	}

	public void playerMove(Player p, GameMsg msg)
	{
		if(msg.gameState==3)
		{
			rooms.get(p.roomId).playerMove(p,msg);
		}
		else{
			logger.error("�û��ƶ����ĳ���");
		}
	}

	public void exitLobby(Player p){

	}


	@Override
    public void channelUnregistered(ChannelHandlerContext ctx) {
		String uid = channelManager.findUser(ctx.channel());

		if(players.get(uid).userState== Constant.in_room)
		{//�ڷ�����
			// TODO: 2021/5/13 �ڷ������˳�
		}

		// �ӹ��������Ƴ�
		channelManager.remove(ctx.channel());
		players.remove(uid);

		logger.info(String.format( "�û�%s�˳����� || ��ǰ����������:%d", uid, players.size()));
	}

    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
        // �Ͽ�����
        ctx.channel().close();
    }

}
