package Server;

import io.netty.buffer.ByteBuf;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.MessageToByteEncoder;
import org.apache.log4j.Logger;

public class NettyMesageEncoder extends MessageToByteEncoder<NettyMessage> {
    private static Logger logger = Logger.getLogger(NettyMesageEncoder.class);

    private NettyChannelManager channelManager= NettyChannelManager.getInstance();


    @Override
    protected void encode(ChannelHandlerContext channelHandlerContext, NettyMessage nettyMessage, ByteBuf out) throws Exception {
        //写消息体
        if(nettyMessage.getLength()!=0) {
            if (nettyMessage.getMessageType() != 0) {
                //加密操作
                nettyMessage.setMessageBody(channelManager.
                        getUserDes(channelManager.
                        findUser(channelHandlerContext.channel())).
                        Encryption(nettyMessage.getMessageBody()));

                logger.info(String.format("加密后报文长度：%d",nettyMessage.getLength()));
            }
        }
        out.writeBytes(nettyMessage.composeFull());
    }
}
