package Server;

import io.netty.buffer.ByteBuf;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.ByteToMessageDecoder;
import org.apache.log4j.Logger;

import java.util.List;

/**
 * <p>
 * 解码，将字节数组转换为NettyMessage对象
 * 
 * @author myumen
 * @date 2017.09.27
 */
public class NettyMessageDecoder extends ByteToMessageDecoder {

	private static Logger logger = Logger.getLogger(NettyMessageDecoder.class);
	
	@Override
	protected void decode(ChannelHandlerContext ctx, ByteBuf in, List<Object> out) throws Exception {
		// 不够报文头长度,返回
		if (in.readableBytes() < NettyMessage.HEAD_LEN) {
			return;
		}



		in.markReaderIndex();
		byte messageP2P = in.readByte();
		byte messageType = in.readByte();
		byte stateCode = in.readByte();
		byte unEncode = in.readByte();
		int length = in.readInt();

		// 如果magicNumber对不上或者length为负数，那有可能是通过telnet随意输入的内容，直接丢弃处理，不需要重置readerIndex
		//if (magicNumber != Constants.MAGIC_NUMBER || length < 0) {
		//	logger.warn("非法输入,丢弃");
		//	return;
		//}
		
		// 长度超过消息头长度,但是剩下的不够一个完整的报文,那么就重置readerIndex，返回等读取更多的数据再处理
		if (in.readableBytes() < length) {
			in.resetReaderIndex();
			return;
		}

		//判断tpye是否要在这里解密

		NettyMessage message = new NettyMessage(messageP2P, messageType,stateCode,unEncode, length);
		byte[] bodyArray = new byte[length];
		in.readBytes(bodyArray);
		message.setMessageBody(bodyArray);
		out.add(message);
	}
}
