using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

public enum PacketID
{
    PlayerInfoReq = 1,
	Test = 2,
	
}

interface IPacket {
	ushort Protocol { get; }
	public void Read(ArraySegment<byte> segment);
	public ArraySegment<byte> Write();
}

class PlayerInfoReq : IPacket {
    public byte testByte;
	public long playerID;
	public string name;
	
	public class Skill
	{
	    public int id;
		public short level;
		public float duration;
		
		public class Attribute
		{
		    public int att;
		
		    public void Read(ReadOnlySpan<byte> s, ref ushort count)
		    {
		        this.att = BitConverter.ToInt32(s.Slice(count, s.Length-count));
				count += sizeof(int);
				
		    }
		
		    public bool Write(Span<byte> s, ref ushort count)
		    {
		        bool success = true;
		        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), att);
				count += sizeof(int);
				
		        return success;
		    }
		}
		
		public List<Attribute> attributes = new List<Attribute>();
		
	
	    public void Read(ReadOnlySpan<byte> s, ref ushort count)
	    {
	        this.id = BitConverter.ToInt32(s.Slice(count, s.Length-count));
			count += sizeof(int);
			
			this.level = BitConverter.ToInt16(s.Slice(count, s.Length-count));
			count += sizeof(short);
			
			this.duration = BitConverter.ToSingle(s.Slice(count, s.Length-count));
			count += sizeof(float);
			
			this.attributes.Clear();
			ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			for (int i = 0; i < attributeLen; i++)
			{
			    Attribute attribute = new Attribute();
			    attribute.Read(s, ref count);
			    attributes.Add(attribute);
			}
			
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
			
			success &= BitConverter.TryWriteBytes(s, count);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)attributes.Count);
			count += sizeof(ushort);
			
			foreach (Attribute attribute in attributes)
			{
			    success &= attribute.Write(s, ref count);
			}
			
	        return success;
	    }
	}
	
	public List<Skill> skills = new List<Skill>();
	

    public ushort Protocol { get { return (ushort)PacketID.PlayerInfoReq; } }

    public void Read(ArraySegment<byte> segment)
    {
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        testByte = (byte)segment.Array[segment.Offset + count];
		count += sizeof(byte);
		
		this.playerID = BitConverter.ToInt32(s.Slice(count, s.Length-count));
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

        segment.Array[segment.Offset + count] = (byte)testByte;
		count += sizeof(byte);
		
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
class Test : IPacket {
    public int testInt;

    public ushort Protocol { get { return (ushort)PacketID.Test; } }

    public void Read(ArraySegment<byte> segment)
    {
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.testInt = BitConverter.ToInt32(s.Slice(count, s.Length-count));
		count += sizeof(int);
		
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.Test);
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), testInt);
		count += sizeof(int);
		
        success &= BitConverter.TryWriteBytes(s, count);
        if (!success) return null;
        return SendBufferHelper.Close(count);
    }
}

