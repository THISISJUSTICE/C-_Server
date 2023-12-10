using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    //비동기 방식의 리스닝
    public class Listener
    {
        Socket listenSocket;
        Func<Session> sessionFactory_; //Socket 매개변수 하나를 받는 함수

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory) { 
            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sessionFactory_ += sessionFactory;

            //Bind
            listenSocket.Bind(endPoint);

            //back log: 최대 대기수
            listenSocket.Listen(10);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            //소켓을 수신받으면 이벤트를 발생시켜 해당 함수 실행
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            //최초 리스닝 시작
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args) {
            //이전에 수신한 소켓 정보 초기화
            args.AcceptSocket = null;

            Console.WriteLine("Listening...");
            
            //해당 함수를 호출하자마자 소켓을 수신받을 경우 실행됨
            bool pending = listenSocket.AcceptAsync(args);
            if (!pending) {
                OnAcceptCompleted(null, args);
            }

        }

        //멀티 스레드로 실행될 수 있다는 점을 염두
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.SocketError == SocketError.Success)
            {
                Session session = sessionFactory_.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else {
                Console.WriteLine(args.SocketError.ToString());
            }

            //소켓 수신 후 다시 리스닝 시작
            RegisterAccept(args);
        }

        //동기 방식(사용하지 않음)
        public Socket Accept() {
            return listenSocket.Accept();
        }
    }
}
