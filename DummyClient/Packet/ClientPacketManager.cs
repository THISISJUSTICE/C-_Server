using System;
using System.Collections.Generic;
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
    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> makeFunc_ = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> handler_ = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register() {      
        makeFunc_.Add((ushort)PacketID.S_BroadCastEnterGame, MakePacket<S_BroadCastEnterGame>);
        handler_.Add((ushort)PacketID.S_BroadCastEnterGame, PacketHandler.S_BroadCastEnterGameHandler);
      
        makeFunc_.Add((ushort)PacketID.S_BroadCastLeaveGame, MakePacket<S_BroadCastLeaveGame>);
        handler_.Add((ushort)PacketID.S_BroadCastLeaveGame, PacketHandler.S_BroadCastLeaveGameHandler);
      
        makeFunc_.Add((ushort)PacketID.S_PlayerList, MakePacket<S_PlayerList>);
        handler_.Add((ushort)PacketID.S_PlayerList, PacketHandler.S_PlayerListHandler);
      
        makeFunc_.Add((ushort)PacketID.S_BroadCastMove, MakePacket<S_BroadCastMove>);
        handler_.Add((ushort)PacketID.S_BroadCastMove, PacketHandler.S_BroadCastMoveHandler);

    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null) {
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (makeFunc_.TryGetValue(id, out func)) {
            IPacket packet = func(session, buffer);

            if(onRecvCallback != null) onRecvCallback(session, packet);
            else HandlePacket(session, packet);
        }
    }

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() {
        T pkt = new T();
        pkt.Read(buffer);
        return pkt;
    }

    public void HandlePacket(PacketSession session, IPacket packet){
        Action<PacketSession, IPacket> action = null;
        if(handler_.TryGetValue(packet.Protocol, out action)){
            action(session, packet);
            
        }
    }
}
