package Server;

import io.netty.buffer.ByteBuf;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.ByteToMessageDecoder;
import org.apache.log4j.Logger;

import java.util.List;

/**
 * <p>
 * ���룬���ֽ�����ת��ΪNettyMessage����
 * 
 * @author myumen
 * @date 2017.09.27
 */
public class NettyMessageDecoder extends ByteToMessageDecoder {

	private static Logger logger = Logger.getLogger(NettyMessageDecoder.class);
	
	@Override
	protected void decode(ChannelHandlerContext ctx, ByteBuf in, List<Object> out) throws Exception {
		// ��������ͷ����,����
		if (in.readableBytes() < NettyMessage.HEAD_LEN) {
			return;
		}



		in.markReaderIndex();
		byte messageP2P = in.readByte();
		byte messageType = in.readByte();
		byte stateCode = in.readByte();
		byte unEncode = in.readByte();
		int length = in.readInt();

		// ���magicNumber�Բ��ϻ���lengthΪ���������п�����ͨ��telnet������������ݣ�ֱ�Ӷ�����������Ҫ����readerIndex
		//if (magicNumber != Constants.MAGIC_NUMBER || length < 0) {
		//	logger.warn("�Ƿ�����,����");
		//	return;
		//}
		
		// ���ȳ�����Ϣͷ����,����ʣ�µĲ���һ�������ı���,��ô������readerIndex�����صȶ�ȡ����������ٴ���
		if (in.readableBytes() < length) {
			in.resetReaderIndex();
			return;
		}

		//�ж�tpye�Ƿ�Ҫ���������

		NettyMessage message = new NettyMessage(messageP2P, messageType,stateCode,unEncode, length);
		byte[] bodyArray = new byte[length];
		in.readBytes(bodyArray);
		message.setMessageBody(bodyArray);
		out.add(message);
	}
}
