using System.Net;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener listener_ = new Listener();

        static void Main(string[] args)
        {
            PacketManager.Inst.Register();

            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener_.Init(endPoint, () => { return new ClientSession(); });

            while (true)
            {

            }
        }
    }
}
