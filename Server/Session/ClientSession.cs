using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{
	class ClientSession : PacketSession
    {
        public int sessionID { get; set; }
        public GameRoom room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");

            Program.room.Push(() => Program.room.Enter(this));
            
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Inst.Remove(this);
            if (room != null) {
                GameRoom room_ = room;
                room_.Push(() => room_.Leave(this));
                room = null;
            }

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Inst.OnRecvPacket(this, buffer);            
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
