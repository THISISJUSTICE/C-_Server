using System;
using System.Net;
using ServerCore;
using Server.Game;
using System.Threading;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();

        static void FlushRoom() {
            JobTimer.Inst.Push(FlushRoom, 250);
        }
        static void Main(string[] args)
        {
            RoomManager.Instance.Add(1);

            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Inst.Generate(); });

            //JobTimer.Inst.Push(FlushRoom);

            while (true)
            {
                //JobTimer.Inst.Flush();
                RoomManager.Instance.Find(1).Update();

                Thread.Sleep(100);
            }
        }

    }

}
