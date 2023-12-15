using System.Net;
using System.Threading;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener listener_ = new Listener();
        public static GameRoom room = new GameRoom();

        static void FlushRoom() {
            room.Push(() => room.Flush());
            JobTimer.Inst.Push(FlushRoom, 250);
        }
        static void Main(string[] args)
        {
            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener_.Init(endPoint, () => { return SessionManager.Inst.Generate(); });

            JobTimer.Inst.Push(FlushRoom);

            while (true)
            {
                JobTimer.Inst.Flush();
            }
        }

    }

}
