using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;
using ServerCore;


class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.room == null) {
            return;
        }
        GameRoom room = clientSession.room;
        room.Push(() => room.BroadCast(clientSession, chatPacket.chat));

    }

    public static void TestHandler(PacketSession session, IPacket packet)
    {
        
    }

}

