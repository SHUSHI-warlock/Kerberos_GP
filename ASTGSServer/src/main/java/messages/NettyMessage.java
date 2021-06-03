package messages;

import myutil.ByteTransUtil;

import java.io.Serializable;
import java.nio.charset.StandardCharsets;
import java.text.MessageFormat;

/**
 * <p>
 * ��Ϣ���󣬸���Э�鶨����Ϣ��ʽ����Ϣ��ʽΪ��messageP2P+messageType+stateCode+unEncode+Length+��Ϣ��byte[]��
 * messageP2P ��ʾͨ��˫��
 * messageType ָ������Ϣ��ĳ����ͨ���е���һ�ֱ���
 * stateCode ������Ϣ״̬��һ�����ڷ��ؽ��������
 * unEncode ��ʱ�޺��壬�������ಿ�֣�ͨ����Ϊ-1
 * Length Ϊ��Ϣ���byte��С
 * @date 2021.05.10
 */
public class NettyMessage implements Serializable {
	private static final long serialVersionUID = 3201210212398130551L;

	/**
	 * ��Ϣͷ�ֽڳ���
	 */
	public static final int HEAD_LEN = 8;
	/**
	 * ͨ��˫��
	 */
	private byte messageP2P = 1;
	/**
	 * ��Ϣ����
	 */
	private byte messageType = 1;
	/**
	 * ״̬��
	 */
	private byte stateCode = 1;
	/**
	 * ������
	 */
	private byte unEncode = -1;    //ȫ1
	/**
	 * ��Ϣ�峤�ȣ���messageBody.length
	 */
	private int length = 0;

	/**
	 * ��Ϣͷ��8�ֽڳ��ȣ��������ĸ�������ɣ�messageP2P|messageType|stateCode|UnEncode|length
	 * �����ְ���˴�ȡ
	 */
	private byte[] messageHead;

	/**
	 * ��Ϣ�壬Ĭ��ΪUTF-8����
	 */
	private byte[] messageBody;

	/**
	 * Ĭ����������
	 */
	public static final NettyMessage HEATBEAT_MSG = buildHeartBeatMsg();

	public static NettyMessage buildHeartBeatMsg() {
		NettyMessage hb = new NettyMessage();
		hb.setMessageType((byte) 0);

		hb.setMessageBody("HB".getBytes()); // Ĭ�ϱ��뼴��,Ӣ���ַ������б���������һ����
		return hb;
	}

	public NettyMessage() {

	}

	/**
	 * ����ͷ��ʼ��
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
	 * �ñ������ݳ�ʼ��
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
	 * �ý��յ��ı��ĳ�ʼ��
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

	/* ��������ͷ */
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

	/* ���ɱ���ͷ */
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
	 * ����������Ϣ��Ӧ���ֽ����飬���û����Ϣ�壬��ֻ��ͷ��
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

