using System.Net;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener listener_ = new Listener();
        public static GameRoom room = new GameRoom();

        static void Main(string[] args)
        {
            PacketManager.Inst.Register();

            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener_.Init(endPoint, () => { return SessionManager.Inst.Generate(); });

            while (true)
            {

            }
        }
    }
}
