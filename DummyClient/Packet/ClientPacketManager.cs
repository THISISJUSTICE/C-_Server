using System;
using System.Collections.Generic;
using ServerCore;

class PacketManager
{
    #region Singleton
    static PacketManager instance_;
    public static PacketManager Inst
    {
        get {
            if (instance_ == null) {
                instance_ = new PacketManager();
            }
            return instance_;
        }
    }
    #endregion

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> handler_ = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register() {
      
        onRecv.Add((ushort)PacketID.S_Test, MakePacket<S_Test>);
        handler_.Add((ushort)PacketID.S_Test, PacketHandler.S_TestHandler);

    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer) {
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (onRecv.TryGetValue(id, out action)) {
            action.Invoke(session, buffer);
        }

    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() {
        T pkt = new T();
        pkt.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (handler_.TryGetValue(pkt.Protocol, out action)) {
            action.Invoke(session, pkt);
        }
    }
}
