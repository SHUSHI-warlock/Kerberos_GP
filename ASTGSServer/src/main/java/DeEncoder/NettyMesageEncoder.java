package DeEncoder;

import io.netty.buffer.ByteBuf;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.MessageToByteEncoder;
import messages.NettyMessage;

public class NettyMesageEncoder extends MessageToByteEncoder<NettyMessage> {
    @Override
    protected void encode(ChannelHandlerContext channelHandlerContext, NettyMessage nettyMessage, ByteBuf out) throws Exception {
        //判断是否加密

        //写消息头部
        out.writeBytes(nettyMessage.getMessageHead());

        //写消息体
        if(nettyMessage.getLength()!=0)
            out.writeBytes(nettyMessage.getMessageBody());
    }
}
