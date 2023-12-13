using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> sessionFactory_;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1) {
            for (int i = 0; i < count; i++)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sessionFactory_ = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnConnectedCompleted;
                args.RemoteEndPoint = endPoint;
                args.UserToken = socket;

                RegisterConnect(args);
            }
        }

        void RegisterConnect(SocketAsyncEventArgs args) {
             Socket socket = args.UserToken as Socket;
            if (socket == null) return;

            bool panding = socket.ConnectAsync(args);
            if (!panding) {
                OnConnectedCompleted(null, args);
            }
        }

        void OnConnectedCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.SocketError == SocketError.Success)
            {
                Session session = sessionFactory_.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else {
                Console.WriteLine($"OnConnectedCompleted Failed : {args.SocketError}");
            }
        }
    }
}