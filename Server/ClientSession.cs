﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class Packet
    {
        public ushort size;
        public ushort packetID;
    }

    class PlayerInfoReq : Packet
    {
        public long playerID;
    }

    class PlayerInfoOK : Packet
    {
        public int hp;
        public int attack;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2
    }

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");

            Packet packet = new Packet { size = 4, packetID = 10 };

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] buffer = BitConverter.GetBytes(packet.size);
            byte[] buffer2 = BitConverter.GetBytes(packet.packetID);

            Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

            Send(sendBuff);
            Thread.Sleep(5000);
            DisConnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            Console.WriteLine($"[From Server] RecvPacket ID: {id}, size: {size}");

            switch ((PacketID)id) {
                case PacketID.PlayerInfoReq:
                    {
                        long playerID = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
                        count += 2;
                        Console.WriteLine($"[From Server] Player ID: {playerID}");
                    }
                    break;
                case PacketID.PlayerInfoOK:
                    break;
            }

            
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
