using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ServerCore;

namespace DummyClient
{

	class PlayerInfoReq
	{
		public long playerID;
		public string name;
		public struct Skill
		{
			public int id;
			public short level;
			public float duration;

			public void Read(ReadOnlySpan<byte> s, ref ushort count)
			{
				this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
				count += sizeof(int);

				this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
				count += sizeof(short);

				this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
				count += sizeof(float);

			}

			public bool Write(Span<byte> s, ref ushort count)
			{
				bool success = true;
				success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
				count += sizeof(int);

				success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
				count += sizeof(short);

				success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
				count += sizeof(float);

				return success;
			}
		}

		public List<Skill> skills = new List<Skill>();


		public void Read(ArraySegment<byte> segment)
		{
			ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

			ushort count = 0;
			count += sizeof(ushort);
			count += sizeof(ushort);
			this.playerID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(long);

			ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
			count += nameLen;

			this.skills.Clear();
			ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			for (int i = 0; i < skillLen; i++)
			{
				Skill skill = new Skill();
				skill.Read(s, ref count);
				skills.Add(skill);
			}

		}

		public ArraySegment<byte> Write()
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
			bool success = true;
			ushort count = 0;

			Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

			count += sizeof(ushort);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
			count += sizeof(ushort);

			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), playerID);
			count += sizeof(long);

			ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
			count += sizeof(ushort);
			count += nameLen;

			success &= BitConverter.TryWriteBytes(s, count);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
			count += sizeof(ushort);

			foreach (Skill skill in skills)
			{
				success &= skill.Write(s, ref count);
			}

			success &= BitConverter.TryWriteBytes(s, count);
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

            PlayerInfoReq packet = new PlayerInfoReq {playerID = 1001, name = "ABCD" };
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 101, level = 1, duration = 3.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 201, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 301, level = 3, duration = 5.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 401, level = 4, duration = 6.0f });

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
