using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace DummyClient
{
    class Packet
    {
        public ushort size;
        public ushort packetID;
    }

    class PlayerInfoReq : Packet {
        public long playerID;
    }

    class PlayerInfoOK : Packet {
        public int hp;
        public int attack;
    }

    public enum PacketID {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2
    }

    class ServerSession : PacketSession
    {
        //unsafe를 사용하면 C++처럼 포인터를 사용
        /*static unsafe void ToBytes(byte[] array, int offset, ulong value) { 
            
        }*/

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq {packetID = (ushort)PacketID.PlayerInfoReq, playerID = 1001 };

            //송신
            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

                //BitConverter 는 new byte[]을 계속 생성하기에 비효율적임

                bool success = true;
                ushort count = 0;
                //success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), packet.size);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), packet.packetID);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), packet.playerID);
                count += 8;

                //패킷의 사이즈는 마지막에 계산해줘야 알 수 있기에 마지막에 입력
                success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), count);

                /*byte[] size = BitConverter.GetBytes(packet.size); // 2byte
                byte[] packetID = BitConverter.GetBytes(packet.packetID); // 2byte
                byte[] playerID = BitConverter.GetBytes(packet.playerID); //8byte
                Array.Copy(size, 0, openSegment.Array, openSegment.Offset + count, 2);
                count += 2;
                Array.Copy(packetID, 0, openSegment.Array, openSegment.Offset + count, 2);
                count += 2;
                Array.Copy(playerID, 0, openSegment.Array, openSegment.Offset + count, 8);
                count += 8;*/

                ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);
                if(success)
                    Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);

            Console.WriteLine($"[From Server] RecvPacket ID: {id}, size: {size}"); ;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
