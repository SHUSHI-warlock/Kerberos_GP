﻿using Client.Utils.LogHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.MsgTrans
{
    public class MyTcpClient
    {

        private static Logger logger = Logger.GetLogger();
        private Socket _clientSocket;

        private string _server;
        private int _port;

       
        public MyTcpClient(String ip, int port)
        {
            _server = ip;
            _port = port;
        }

        public void Connect()
        {
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse(_server), _port);
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.BeginConnect(ip, new AsyncCallback(ConnectCallBack), _clientSocket);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void ConnectCallBack(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            try
            {
                client.EndConnect(iar);
                logger.Debug(String.Format("已连接——{0}:{1}", _server, _port));
                //OnConnect("已连接");
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10061)
                {
                    logger.Error("服务器程序未运行或服务器端口未开放");
                    //OnErr("服务器程序未运行或服务器端口未开放");
                }
                else
                {
                    //OnErr(e.Message);
                    logger.Error(e.Message);
                    logger.Error(e.StackTrace);
                }
            }
            finally
            {
            }
        }

        public void Send(Message msg)
        {
            if (_clientSocket == null)
                return;

            //msg += "\r\n";

            byte[] data = msg.composeFull();
            try
            {
                _clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    int length = _clientSocket.EndSend(asyncResult);

                    logger.Debug(string.Format("发送报文 Head:[P2P:{0} Type:{1} State:{2} Length:{3}]",
                msg.MessageP2P, msg.MessageType, msg.StateCode, msg.Length));
                    logger.Debug(msg.bodyToString());

                    //OnSend(string.Format("客户端发送消息:{0}", msg));
                }, null);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                logger.Error(e.StackTrace);
                //OnErr(e.Message);
            }
        }

        public Message Recive()
        {
            byte[] head = new byte[8];
            _clientSocket.Receive(head, Message.HEAD_LEN, SocketFlags.None);
            try
            {
                //_clientSocket.BeginReceive(head, 0, head.Length, SocketFlags.None,
                //asyncResult =>
                //{
                //try
                //{
                //int length = _clientSocket.EndReceive(asyncResult);

                Message message = new Message(head);

                if (message.MessageP2P == 0 && message.MessageType == 0 && message.Length == 0 && message.UnDefineCode == 0)
                {//服务器断开连接
                    logger.Error("服务器断开连接！");
                    throw new Exception("服务器断开连接！");
                }

                logger.Debug(string.Format("收到服务器消息:P2P:{0},Type:{1},State:{2},Length:{3}", message.MessageP2P, message.MessageType, message.StateCode, message.Length));

                if (message.Length != 0)
                {
                    byte[] data = new byte[message.Length];
                    _clientSocket.Receive(data, message.Length, SocketFlags.None);
                    //OnReceive(string.Format("收到服务器消息:长度：{1},{0}", Encoding.UTF8.GetString(data), length));
                    //logger.Debug(string.Format("收到服务器消息:长度：{1},{0}", Encoding.UTF8.GetString(data), length));
                    message.SetBody(data);
                    logger.Debug(message.bodyToString());

                }
                //OnReceive.Invoke(message);


                //Recive();
                return message;
            }
            catch (Exception e)
            {
                if((e is SocketException)&&(e as SocketException).ErrorCode==10054)
                {
                    //OnServerDown("服务器已断线");
                    logger.Error("服务器已断线");
                }
                else
                {
                    logger.Error(e.Message);
                    //OnErr(e.Message);
                }
            }
            return null;
        }
    
        public void Close()
        {
            try
            {
                //_clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch(Exception e)
            {
                logger.Error(e.Message);
            }
        }


    }
}