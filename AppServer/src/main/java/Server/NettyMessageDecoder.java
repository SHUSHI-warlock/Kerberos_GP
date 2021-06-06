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
 * ���룬���ֽ�����ת��ΪNettyMessage����
 * 
 * @author myumen
 * @date 2017.09.27
 */
public class NettyMessageDecoder extends ByteToMessageDecoder {

	private static Logger logger = Logger.getLogger(NettyMessageDecoder.class);
	private NettyChannelManager channelManager= NettyChannelManager.getInstance();

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

		NettyMessage message = new NettyMessage(messageP2P, messageType,stateCode,unEncode, length);
		byte[] bodyArray = new byte[length];
		in.readBytes(bodyArray);

		//�û�������
		try {
			String userid = channelManager.findUser(ctx.channel());
			if (userid == null)//�û���֤
			{
				logger.info("�յ�δ��¼��������Ϣ");
				message.setMessageBody(bodyArray);
			} else {
				logger.info("�������յ�һ����Ϣ");
				if(message.getLength()!=0)
					message.setMessageBody(channelManager.getUserDes(userid).Decryption(bodyArray));
			}
		}
		catch (Exception e)
		{
			logger.error("���Լӽ��ܳ���");
			e.printStackTrace();
		}

		out.add(message);
	}
}
