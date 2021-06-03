package TGSserver;

import DeEncoder.NettyMesageEncoder;
import DeEncoder.NettyMessageDecoder;
import io.netty.bootstrap.ServerBootstrap;
import io.netty.channel.ChannelFuture;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelOption;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.SocketChannel;
import io.netty.channel.socket.nio.NioServerSocketChannel;
import io.netty.handler.codec.string.StringDecoder;
import io.netty.handler.codec.string.StringEncoder;

public class serverTGS {
    public void bind(int port) throws Exception {

        EventLoopGroup bossGroup = new NioEventLoopGroup(); //bossGroup����parentGroup���Ǹ�����TCP/IP���ӵ�
        EventLoopGroup workerGroup = new NioEventLoopGroup(); //workerGroup����childGroup,�Ǹ�����Channel(ͨ��)��I/O�¼�

        ServerBootstrap sb = new ServerBootstrap();
        sb.group(bossGroup, workerGroup)
                .channel(NioServerSocketChannel.class)
                .option(ChannelOption.SO_BACKLOG, 128) //��ʼ������˿����Ӷ���,ָ���˶��еĴ�С128
                .childOption(ChannelOption.SO_KEEPALIVE, true) //���ֳ�����
                .childHandler(new ChannelInitializer<SocketChannel>() {  // �󶨿ͻ�������ʱ�򴥷�����
                    @Override
                    protected void initChannel(SocketChannel sh) throws Exception {
                        sh.pipeline()
                                .addFirst(new NettyMesageEncoder()) //����request
                                .addLast(new NettyMessageDecoder()) //����response
                                .addLast(new ServerHandler()); //ʹ��ServerHandler����������յ�����Ϣ
                    }
                });
        //�󶨼����˿ڣ�����syncͬ�����������ȴ��󶨲�����
        ChannelFuture future = sb.bind(port).sync();

        if (future.isSuccess()) {
            System.out.println("����������ɹ�");
        } else {
            System.out.println("���������ʧ��");
            future.cause().printStackTrace();
            bossGroup.shutdownGracefully(); //�ر��߳���
            workerGroup.shutdownGracefully();
        }

        //�ɹ��󶨵��˿�֮��,��channel����һ�� �ܵ��رյļ�������ͬ������,ֱ��channel�ر�,�̲߳Ż�����ִ��,�������̡�
        future.channel().closeFuture().sync();

    }


}
