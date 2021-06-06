package TGSserver;

import Message.AuthenticationMessage;
import Message.MessageInfo;
import Message.TGS;
import com.google.gson.Gson;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.ChannelInboundHandlerAdapter;
import messages.Message;
import messages.NettyMessage;
import org.apache.log4j.Logger;

import java.util.Arrays;

import static Message.byteManage.generateTime;

public class ServerHandler extends ChannelInboundHandlerAdapter{

    protected static Logger logger = Logger.getLogger(ServerHandler.class);
    private TGS tgs=new TGS();
    private static Gson gson = new Gson();
    //����client���͵���Ϣ
    @Override
    public void channelRead(ChannelHandlerContext ctx, Object msg) throws Exception {
        System.out.println("���յ��ͻ�����Ϣ");
        NettyMessage message=(NettyMessage)msg;
        logger.info(String.format("���յ�����Head:[P2P:%d Type:%d State:%d UnCode:%d Length:%d]",
                message.getMessageP2P(),message.getMessageType(),message.getStateCode(),message.getUnEncode(),message.getLength()));

        if(message.getMessageP2P()==2){
            enterTGS(ctx, message);
        }

        //System.out.println("���յ��Ŀͻ�����Ϣ��"+message.getStateCode());
        //logger.info(String.format("���յ�����Head:[P2P:%d Type:%d State:%d UnCode:%d Length:%d]",
        //        message.getMessageP2P(),message.getMessageType(),message.getStateCode(),message.getUnDefineCode(),message.getLength()));


        /*
        RpcRequest request = (RpcRequest) msg;
        System.out.println("���յ��ͻ�����Ϣ:" + request.toString());
        //���ص����ݽṹ
        RpcResponse response = new RpcResponse();
        response.setId(UUID.randomUUID().toString());
        response.setData("server��Ӧ���");
        response.setStatus(1);
        ctx.writeAndFlush(response);


         */

        //Message mes=new Message(0,0,0,-1);
        //ctx.writeAndFlush(mes);

    }

    //֪ͨ����������channelRead()�ǵ�ǰ�������е����һ����Ϣʱ����
    @Override
    public void channelReadComplete(ChannelHandlerContext ctx) throws Exception {
        System.out.println("����˽����������..");
        ctx.flush();
    }

    //������ʱ�����쳣ʱ����
    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
        ctx.close();
    }

    //�ͻ���ȥ�ͷ�������ӳɹ�ʱ����
    @Override
    public void channelActive(ChannelHandlerContext ctx) throws Exception {
        //ctx.writeAndFlush("hello client");
    }


    public void enterTGS(ChannelHandlerContext channel,NettyMessage message){
        //MessageInfo msg=gson.fromJson(message,MessageInfo.class);
        byte[] mes=message.getMessageBody();
        //byte[] mes=msg.getMessage();
        //byte[] mes=message.getBytes();
        AuthenticationMessage fromClient=new AuthenticationMessage();

        System.out.println(Arrays.toString(tgs.TGSKeyAS.getKeyBytes()));
        fromClient.CTGSMessage(mes,tgs.TGSKeyAS);
        //System.out.println("f"+fromClient.IDv);


        logger.info(String.format("�û�%s��ʼ������֤",fromClient.IDc));
        tgs.verify(fromClient);
        System.out.println(tgs.IDv);
        tgs.ADc="127.0.0.1";
        if(tgs.ADc.length()<16){
            for(int i=tgs.ADc.length();i<16;i++){
                tgs.ADc+=" ";
            }
        }
        if(tgs.status==0){
            String ts=generateTime();
            tgs.TS=ts;
            tgs.generateVkey();
            logger.info(String.format("�û�%s��֤�ɹ�", fromClient.IDc));
            if(tgs.IDc.length()<20){
                for(int i=tgs.IDc.length();i<20;i++){
                    tgs.IDc+=" ";
                }
            }

            byte[] ticket=tgs.generateTicket();

            byte[] mess=tgs.generateBack(tgs.IDv,ticket,tgs.sKey,ts,tgs.Kcv);
            //MessageInfo back=new MessageInfo(3,0,0,-1);
            //back.setMessage(mess);
            NettyMessage back=new NettyMessage(3,0,0);
            back.setMessageBody(mess);
            System.out.println("���ڷ�����Ϣ");
            channel.writeAndFlush(back);
        }
        else{
            //System.out.println(tgs.status);
            NettyMessage back=new NettyMessage(3,0,tgs.status);
            channel.writeAndFlush(back);
            logger.info(String.format("�û�%s��֤ʧ��",fromClient.IDc));
        }
    }
}
