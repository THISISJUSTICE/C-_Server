using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;

namespace Server
{
	class ClientSession : PacketSession
    {
        public int sessionID { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");

            S_Chat chat = new S_Chat()
            {
                Context = "안녕하세요"
            };

            ushort size = (ushort)chat.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes(size + 4), 0, sendBuffer, 0, sizeof(ushort));
            ushort protocolID = (ushort)MsgId.SChat;
            Array.Copy(BitConverter.GetBytes(protocolID), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(chat.ToByteArray(), 0, sendBuffer, 4, size);
            
            Send(new ArraySegment<byte>(sendBuffer));
            //Program.room.Push(() => Program.room.Enter(this));

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Inst.Remove(this);

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            Console.WriteLine("OnRecvPacket");
            PacketManager.Inst.OnRecvPacket(this, buffer);            
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
