package Server;

import io.netty.channel.Channel;
import io.netty.channel.ChannelId;
import io.netty.util.AttributeKey;
import org.apache.log4j.Logger;

import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

/**
 * 客户端 Channel 管理器。提供两种功能：
 * 1. 客户端 Channel 的管理
 * 2. 向客户端 Channel 发送消息
 */

public class NettyChannelManager {
    protected static Logger logger = Logger.getLogger(NettyChannelManager.class);

    /**
     * 表示 Channel 对应的用户
     */
    private static final AttributeKey<String> CHANNEL_ATTR_KEY_USER = AttributeKey.newInstance("user");
    /**
     * Channel 映射
     */
    private ConcurrentMap<ChannelId, Channel> channels = new ConcurrentHashMap<ChannelId, Channel>();
    
    /**
     * 用户与 Channel 的映射。
     *
     * 通过它，可以获取用户对应的 Channel。这样，我们可以向指定用户发送消息。
     */
    
    
    //大厅		
    private ConcurrentMap<String, Channel> userChannels = new ConcurrentHashMap<String, Channel>();

    /**
     * 添加 Channel
     *
     * @param channel Channel
     */
    public void add(Channel channel) {
    	System.out.println("链接的id"+channel.id());
        channels.put(channel.id(), channel);
    }

    /**
     * 添加指定用户
     *
     * @param channel Channel
     * @param user 用户
     */
    public void addUser(Channel channel, String user) {
        Channel existChannel = channels.get(channel.id());
        if (existChannel == null) {
            return;
        }
        // 设置属性
        channel.attr(CHANNEL_ATTR_KEY_USER).set(user);
        // 添加到 userChannels
        userChannels.put(user, channel);
        System.out.println(channel);
    }

    /**
     * 将 Channel 移除
     *
     * @param channel Channel
     */
    public void remove(Channel channel) {
        // 移除 channels
        channels.remove(channel.id());
        // 移除 userChannels
        if (channel.hasAttr(CHANNEL_ATTR_KEY_USER)) {
            userChannels.remove(channel.attr(CHANNEL_ATTR_KEY_USER).get());
        }
    }

    public String findUser(Channel channel)
    {
        return channel.attr(CHANNEL_ATTR_KEY_USER).get();
    }

    /**
     * 向指定用户发送消息
     *
     * @param user 用户
     * @param invocation 消息体
     */
    public void send(String user, NettyMessage invocation) {
        // 获得用户对应的 Channel
        Channel channel = userChannels.get(user);
        if (channel == null) {

            return;
        }
        if (!channel.isActive()) {

            return;
        }
        try {
            // 发送消息
            channel.pipeline().writeAndFlush(invocation);
            logger.debug(String.format( "发送报文：%s",invocation));
        }
        catch (Exception e)
        {
            e.printStackTrace();
            logger.error(String.format( "发送报文失败!"));

        }

    }

    /**
     * 向所有用户发送消息
     *
     * @param invocation 消息体
     */
    public void sendAll(NettyMessage invocation) {
        try {
            for (Channel channel : channels.values()) {
                if (!channel.isActive()) {
                    return;
                }
                // 发送消息
                channel.pipeline().writeAndFlush(invocation);
            }
            logger.debug(String.format("发送报文：%s", invocation));
        }
        catch (Exception e){
            e.printStackTrace();
            logger.error("群发失败！");
        }
    }

    /**
     * 向其他人发送
     * @param user
     * @param invocation
     */
    public void sendOther(String user,NettyMessage invocation)
    {
        try {
            for (Channel channel : channels.values()) {
                if (!channel.isActive()) {
                    return;
                }
                if(user.equals(findUser(channel))) {
                    return;
                }
                // 发送消息
                channel.pipeline().writeAndFlush(invocation);
            }
            logger.debug(String.format("发送报文：%s", invocation));
        }
        catch (Exception e){
            e.printStackTrace();
            logger.error("群发失败！");
        }
    }


}