package Server;

import Service.User.User;
import io.netty.buffer.ByteBuf;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.ByteToMessageDecoder;
import myutil.DESUtil.DesKey;
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
	private NettyChannelManager channelManager= NettyChannelManager.getInstance();

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

		NettyMessage message = new NettyMessage(messageP2P, messageType,stateCode,unEncode, length);
		byte[] bodyArray = new byte[length];
		in.readBytes(bodyArray);

		//用户不在线
		try {
			String userid = channelManager.findUser(ctx.channel());
			if (userid == null)//用户认证
			{
				logger.info("收到未记录的连接消息");
				message.setMessageBody(bodyArray);
			} else {
				logger.info("服务器收到一条消息");
				if(message.getLength()!=0)
					message.setMessageBody(channelManager.getUserDes(userid).Decryption(bodyArray));
			}
		}
		catch (Exception e)
		{
			logger.error("尝试加解密出错！");
			e.printStackTrace();
		}

		out.add(message);
	}
}
