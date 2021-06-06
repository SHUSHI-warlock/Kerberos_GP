package Server;

import io.netty.channel.Channel;
import io.netty.channel.ChannelId;
import io.netty.util.AttributeKey;
import myutil.DESUtil.DESUtils;
import myutil.DESUtil.DesKey;
import org.apache.log4j.Logger;

import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

/**
 * �ͻ��� Channel ���������ṩ���ֹ��ܣ�
 * 1. �ͻ��� Channel �Ĺ���
 * 2. ��ͻ��� Channel ������Ϣ
 */
public class NettyChannelManager {
    protected static Logger logger = Logger.getLogger(NettyChannelManager.class);

    private static NettyChannelManager instance = new NettyChannelManager();

    /**
     * ��ʾ Channel ��Ӧ���û�
     */
    private static final AttributeKey<String> CHANNEL_ATTR_KEY_USER = AttributeKey.newInstance("user");
    /**
     * Channel ӳ��
     */
    private ConcurrentMap<ChannelId, Channel> channels ;

    /**
     * �û��� Channel ��ӳ�䡣
     *
     * ͨ���������Ի�ȡ�û���Ӧ�� Channel�����������ǿ�����ָ���û�������Ϣ��
     */

    //����
    private ConcurrentMap<String, Channel> userChannels;
    private ConcurrentMap<String, DESUtils> userKeys;

    private NettyChannelManager(){
        channels = new ConcurrentHashMap<ChannelId, Channel>();
        userChannels = new ConcurrentHashMap<String, Channel>();
        userKeys = new ConcurrentHashMap<String,DESUtils>();
    }

    public static NettyChannelManager getInstance(){
        return instance;
    }

    /**
     * ��� Channel
     *
     * @param channel Channel
     */
    public void add(Channel channel) {
    	System.out.println("���ӵ�id"+channel.id());
        channels.put(channel.id(), channel);
    }

    /**
     * ���ָ���û�
     *
     * @param channel Channel
     * @param user �û�
     */
    public void addUser(Channel channel, String user,DesKey key) {
        Channel existChannel = channels.get(channel.id());
        if (existChannel == null) {
            return;
        }
        // ��������
        channel.attr(CHANNEL_ATTR_KEY_USER).set(user);
        // ��ӵ� userChannels
        userChannels.put(user, channel);
        userKeys.put(user,new DESUtils(key));
        System.out.println(channel);
    }

    /**
     * �� Channel �Ƴ�
     *
     * @param channel Channel
     */
    public void remove(Channel channel) {
        // �Ƴ� channels
        channels.remove(channel.id());

        // �Ƴ� userChannels
        if (channel.hasAttr(CHANNEL_ATTR_KEY_USER)) {
            userKeys.remove(channel.attr(CHANNEL_ATTR_KEY_USER).get());
            userChannels.remove(channel.attr(CHANNEL_ATTR_KEY_USER).get());
        }
    }

    public String findUser(Channel channel)
    {
        return channel.attr(CHANNEL_ATTR_KEY_USER).get();
    }

    public DESUtils getUserDes(String userid)
    {
        return userKeys.get(userid);
    }

    /**
     * ��ָ���û�������Ϣ
     *
     * @param user �û�
     * @param invocation ��Ϣ��
     */
    public void send(String user, NettyMessage invocation) {
        // ����û���Ӧ�� Channel
        Channel channel = userChannels.get(user);
        if (channel == null) {

            return;
        }
        if (!channel.isActive()) {

            return;
        }
        try {
            // ������Ϣ
            channel.pipeline().writeAndFlush(invocation);
            logger.debug(String.format( "���ͱ��ģ�%s",invocation));
        }
        catch (Exception e)
        {
            e.printStackTrace();
            logger.error(String.format( "���ͱ���ʧ��!"));

        }

    }

    /**
     * �������û�������Ϣ
     *
     * @param invocation ��Ϣ��
     */
    public void sendAll(NettyMessage invocation) {
        logger.debug(String.format("���ͱ��ģ�%s", invocation));

        try {
            for (Channel channel : channels.values()) {
                if (!channel.isActive()) {
                    return;
                }
                // ������Ϣ
                NettyMessage msg = new NettyMessage(invocation);

                channel.pipeline().writeAndFlush(msg);
            }
        }
        catch (Exception e){
            e.printStackTrace();
            logger.error("Ⱥ��ʧ�ܣ�");
        }
    }

    /**
     * �������˷���
     * @param user
     * @param invocation
     */
    public void sendOther(String user,NettyMessage invocation)
    {
        logger.debug(String.format("���ͱ��ģ�%s", invocation));
        try {
            for (Channel channel : channels.values()) {
                if (!channel.isActive()) {
                    return;
                }
                if(user.equals(findUser(channel))) {
                    logger.info("�Լ�����");
                    continue;
                }
                // ������Ϣ
                NettyMessage msg = new NettyMessage(invocation);
                channel.pipeline().writeAndFlush(msg);
            }

        }
        catch (Exception e){
            e.printStackTrace();
            logger.error("Ⱥ��ʧ�ܣ�");
        }
    }


}