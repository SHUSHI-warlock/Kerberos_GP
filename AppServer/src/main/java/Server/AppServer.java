package Server;

import Service.User.Player;
import com.google.gson.Gson;
import io.netty.bootstrap.ServerBootstrap;
import io.netty.channel.ChannelFuture;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelOption;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.ServerSocketChannel;
import io.netty.channel.socket.SocketChannel;
import io.netty.channel.socket.nio.NioServerSocketChannel;
import io.netty.handler.logging.LogLevel;
import io.netty.handler.logging.LoggingHandler;
import io.netty.util.concurrent.EventExecutorGroup;
import org.apache.log4j.Logger;

import java.util.concurrent.TimeUnit;

/**
 * Discards any incoming data.
 */
public class AppServer {
    private static Logger logger = Logger.getLogger(AppServer.class);

    private int port = 8080;

    private ChannelFuture future;

    private ServerBootstrap bootstrap;

    private EventExecutorGroup bizGroup = null;

    private NettyServerHandler serverHandler;

    public AppServer(int port) {
        this.port = port;
        serverHandler = new NettyServerHandler();
    }

    public void run() throws Exception {

        EventLoopGroup bossGroup = new NioEventLoopGroup(1); // (1)
        EventLoopGroup workerGroup = new NioEventLoopGroup(2);
        try {
            ServerBootstrap b = new ServerBootstrap();
            b.group(bossGroup, workerGroup).channel(NioServerSocketChannel.class);
            // 设置用于ServerSocketChannel的属性和handler
            b.handler(new ChannelInitializer<ServerSocketChannel>() {

                protected void initChannel(ServerSocketChannel ch) throws Exception {
                    ch.pipeline().addLast(new LoggingHandler(LogLevel.DEBUG));
                }
            });
            b.option(ChannelOption.SO_BACKLOG, 128);
            b.option(ChannelOption.SO_REUSEADDR, true);

            // 设置用于SocketChannel的属性和handler
            b.childHandler(new ChannelInitializer<SocketChannel>() {
                @Override
                public void initChannel(SocketChannel ch) throws Exception {
                    ch.pipeline().addLast(new LoggingHandler(LogLevel.DEBUG));
                    //ch.pipeline().addLast(new IdleStateHandler(60, 20, 0, TimeUnit.SECONDS));
                    //ch.pipeline().addLast(new NettyHeartBeatDuplexHandler());
                    ch.pipeline().addFirst(new NettyMesageEncoder());
                    ch.pipeline().addLast(new NettyMessageDecoder());
                    ch.pipeline().addLast(serverHandler);
                }
            });

            b.childOption(ChannelOption.SO_KEEPALIVE, true);
            b.childOption(ChannelOption.TCP_NODELAY, true);
            b.childOption(ChannelOption.SO_REUSEADDR, true);

            // Bind and start to accept incoming connections.
            future = b.bind(port).sync();
            bootstrap = b;
            logger.info("server started sucessfully.");
        } catch (Throwable t) {
            logger.error("异常", t);
        }
    }

    public static void main(String[] args) throws Exception {
        int port;
        if (args.length > 0) {
            port = Integer.parseInt(args[0]);
        } else {
            port = 8080;
        }

        new AppServer(port).run();
    }
}