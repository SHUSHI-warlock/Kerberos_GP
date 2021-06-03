package messages;

import myutil.ByteTransUtil;

import java.io.Serializable;
import java.nio.charset.StandardCharsets;
import java.text.MessageFormat;

/**
 * <p>
 * 消息对象，根据协议定义消息格式。消息格式为：messageP2P+messageType+stateCode+unEncode+Length+消息体byte[]，
 * messageP2P 表示通信双端
 * messageType 指明该消息是某两方通信中的哪一种报文
 * stateCode 附带消息状态，一般用于返回结果错误码
 * unEncode 暂时无含义，属于冗余部分，通常置为-1
 * Length 为消息体的byte大小
 * @date 2021.05.10
 */
public class NettyMessage implements Serializable {
	private static final long serialVersionUID = 3201210212398130551L;

	/**
	 * 消息头字节长度
	 */
	public static final int HEAD_LEN = 8;
	/**
	 * 通信双端
	 */
	private byte messageP2P = 1;
	/**
	 * 消息类型
	 */
	private byte messageType = 1;
	/**
	 * 状态码
	 */
	private byte stateCode = 1;
	/**
	 * 冗余码
	 */
	private byte unEncode = -1;    //全1
	/**
	 * 消息体长度，即messageBody.length
	 */
	private int length = 0;

	/**
	 * 消息头，8字节长度，依次由四个数字组成：messageP2P|messageType|stateCode|UnEncode|length
	 * ，数字按大端存取
	 */
	private byte[] messageHead;

	/**
	 * 消息体，默认为UTF-8编码
	 */
	private byte[] messageBody;

	/**
	 * 默认心跳报文
	 */
	public static final NettyMessage HEATBEAT_MSG = buildHeartBeatMsg();

	public static NettyMessage buildHeartBeatMsg() {
		NettyMessage hb = new NettyMessage();
		hb.setMessageType((byte) 0);

		hb.setMessageBody("HB".getBytes()); // 默认编码即可,英文字符在所有编码结果都是一样的
		return hb;
	}

	public NettyMessage() {

	}

	/**
	 * 报文头初始化
	 */
	public NettyMessage(int messageP2P, int messageType, int stateCode) {
		super();
		this.messageP2P = (byte)messageP2P;
		this.messageType = (byte)messageType;
		this.stateCode = (byte)stateCode;
		this.length = 0;
	}
	public NettyMessage(int messageP2P, int messageType, int stateCode, int unEncode, int length) {
		this(messageP2P,messageType,stateCode);
		this.unEncode = (byte)unEncode;
		this.length = length;
	}

	/**
	 * 用报文内容初始化
	 *
	 * @param msg
	 */
	public NettyMessage(String msg) {
		if (msg == null || msg.length() == 0) {
			return;
		}
		this.messageBody = msg.getBytes(StandardCharsets.UTF_8);
		this.length = this.messageBody.length;
	}

	/**
	 * 用接收到的报文初始化
	 *
	 * @param fullMsg
	 */
	public NettyMessage(byte[] fullMsg) {
		if (fullMsg == null || fullMsg.length < HEAD_LEN) {
			return;
		}

		this.messageHead = new byte[HEAD_LEN];
		System.arraycopy(fullMsg, 0, messageHead, 0, HEAD_LEN);
		this.parseHead();
		if (fullMsg.length > HEAD_LEN) {
			this.messageBody = new byte[this.length];
			System.arraycopy(fullMsg, HEAD_LEN, this.messageBody, 0, this.length);
		}
	}

	public int getLength() {
		return length;
	}
	public void setLength(int length) {
		this.length = length;
	}
	public byte getMessageType() {
		return messageType;
	}
	public void setMessageType(int messageType) {
		this.messageType = (byte)messageType;
	}
	public byte getMessageP2P() {
		return messageP2P;
	}
	public void setMessageP2P(int messageP2P) {
		this.messageP2P = (byte)messageP2P;
	}
	public byte getStateCode() {
		return stateCode;
	}
	public void setStateCode(int stateCode) {
		this.stateCode = (byte)stateCode;
	}
	public byte getUnEncode() {
		return unEncode;
	}


	public byte[] getMessageBody() {
		return messageBody;
	}

	public void setMessageBody(byte[] messageBody) {
		this.messageBody = messageBody;
		if (this.messageBody != null) {
			this.length = this.messageBody.length;
		}
	}

	public void setMessageBody(String mb) {
		if (mb == null || mb.length() == 0) {
			return;
		}
		this.messageBody = mb.getBytes();

		this.length = this.messageBody.length;
	}

	public byte[] getMessageHead() {
		if (this.messageHead == null) {
			this.composeHead();
		}
		return messageHead;
	}

	public void setMessageHead(byte[] messageHead) {
		this.messageHead = messageHead;
		this.parseHead();
	}

	/* 解析报文头 */
	private void parseHead() {
		if (messageHead == null || messageHead.length != HEAD_LEN) {
			return;
		}
		this.messageP2P = messageHead[0];
		this.messageType = messageHead[1];
		this.stateCode = messageHead[2];
		this.unEncode = messageHead[3];
		byte[] tmps = new byte[4];
		System.arraycopy(messageHead, 4, tmps, 0, 4);
		this.length = ByteTransUtil.byteArrayToInt(tmps, false);
	}

	/* 生成报文头 */
	private void composeHead() {
		this.messageHead = new byte[HEAD_LEN];
		messageHead[0] = this.messageP2P;
		messageHead[1] = this.messageType;
		messageHead[2] = this.stateCode;
		messageHead[3] = this.unEncode;
		System.arraycopy(ByteTransUtil.intToByteArray(this.length, false), 0, messageHead, 4, 4);
	}

	@Override
	public String toString() {
		return MessageFormat.format(
				"Msg[messageP2P={0,number,###},messageType={1,number,###},stateCode={2,number,###},UnEncode={3,number,###},length={4,number,###}][{5}]",
				messageP2P, messageType, stateCode, unEncode, length, bodyToString());
	}

	public String bodyToString() {
		String body = null;
		if (this.messageBody != null && this.messageBody.length > 0) {
			body = new String(messageBody);
		}
		return body;
	}

	/**
	 * 生成完整消息对应的字节数组，如果没有消息体，就只有头部
	 *
	 * @return
	 */
	public byte[] composeFull() {
		if (this.messageBody != null) {
			this.length = this.messageBody.length;
		}

		byte[] data = new byte[this.length + HEAD_LEN];
		composeHead();
		System.arraycopy(messageHead,0,data,0,HEAD_LEN);

		if (this.messageBody != null) {
			System.arraycopy(this.messageBody, 0, data, HEAD_LEN, this.length);
		}
		return data;
	}

}

