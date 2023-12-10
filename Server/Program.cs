using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;

namespace ServerCore
{
    class Knight {
        public int hp;
        public int attack;
        public string name;
        public List<int> skiils = new List<int>();
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");

            Knight knight = new Knight { hp = 100, attack = 10 };
            
            //송신 버퍼를 외부에 위치시키는 이유는 내부에 있으면 같은 내용의 송신 버퍼를 여러 클라에 전송할 경우 복사 시간 발생함
            /*byte[] sendBuff = new byte[4096];
            byte[] buffer = BitConverter.GetBytes(knight.hp);
            byte[] buffer2 = BitConverter.GetBytes(knight.attack);
            Array.Copy(buffer, 0, sendBuff, 0, buffer.Length);
            Array.Copy(buffer2, 0, sendBuff, buffer.Length, buffer2.Length);*/

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            /*byte[] buffer = BitConverter.GetBytes(knight.hp);
            byte[] buffer2 = BitConverter.GetBytes(knight.attack);*/
            byte[] buffer = Encoding.UTF8.GetBytes("Welcome to MMORPG Server");
            Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length);

            Send(sendBuff);
            Thread.Sleep(1000);
            DisConnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
    class Program
    {
        static Listener listener_ = new Listener();

        static void Main(string[] args)
        {
            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener_.Init(endPoint, () => { return new GameSession(); });

            while (true)
            {

            }
        }
    }
}
