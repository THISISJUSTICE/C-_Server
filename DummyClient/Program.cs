using System;
using System.Net;
using System.Threading;
using ServerCore;

namespace DummyClient
{

    class Program
    {
        static void Main(string[] args)
        {
            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();

            connector.Connect(endPoint, () => { return SessionManager.Inst.Generate(); }, 10);

            while(true){

                try
                {
                    SessionManager.Inst.SendForEach();
                }

                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
                Thread.Sleep(250);
            }
            
        }
    }
}
