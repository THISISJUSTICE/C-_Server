using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        static Listener listener_ = new Listener();

        static void OnAcceptHander(Socket clientSocket) {

            try
            {
                Session session = new Session();
                session.Start(clientSocket);

                //송신
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server");
                session.Send(sendBuff);

                Thread.Sleep(1000);

                session.DisConnect();
                session.DisConnect();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }
        static void Main(string[] args)
        {
            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener_.Init(endPoint, OnAcceptHander);

            while (true) {
                
            }
        }
    }
}
