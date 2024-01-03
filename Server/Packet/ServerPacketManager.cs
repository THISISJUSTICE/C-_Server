using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using ServerCore;

public class PacketManager
{
    #region Singleton
    static PacketManager instance_ = new PacketManager();
    public static PacketManager Inst { get {return instance_;} }
    #endregion

    PacketManager() {
        Register();
    }
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
    Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();

    public void Register() {
        _onRecv.Add((ushort)MsgID.CChat, MakePacket<C_Chat>);
        _handler.Add((ushort)MsgID.CChat, PacketHandler.C_ChatHandler);
    }

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			action(session, pkt);
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id) {
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action)) return action;
		return null;
	}

}
