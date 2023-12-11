using System;
using System.Net;
using ServerCore;

namespace DummyClient
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetID;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> segment);

    }

    class PlayerInfoReq : Packet {
        public long playerID;

        public PlayerInfoReq() {
            packetID = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;
            //ushort size = BitConverter.ToUInt16(segment.Array, segment.Offset);
            count += 2;
            //ushort id = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
            count += 2;
            //패킷의 사이즈를 틀리게 입력해도 데이터를 받는 일이 생기지 않게 함
            playerID = BitConverter.ToUInt16(new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count - count));
            count += 8;
            
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);


            bool success = true;
            ushort count = 0;
            
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), (ushort)PacketID.PlayerInfoReq);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), playerID);
            count += 8;

            //패킷의 사이즈는 마지막에 계산해줘야 알 수 있기에 마지막에 입력
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), count);

            if (!success) return null;

            return SendBufferHelper.Close(count);
        }
    }

    public enum PacketID {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2
    }

    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq {playerID = 1001 };

            //송신
            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> segment = packet.Write();
                if(segment != null)
                    Send(segment);
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
