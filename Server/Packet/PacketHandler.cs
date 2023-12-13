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

        clientSession.room.BroadCast(clientSession, chatPacket.chat);

        Console.WriteLine($"[From Client] Player ID: {p.playerID}, {p.name}");

        foreach (C_PlayerInfoReq.Skill skill in p.skills)
        {
            Console.WriteLine($"[From Client] skill: {skill.id}, {skill.level}, {skill.duration}");
        }
    }

    public static void TestHandler(PacketSession session, IPacket packet)
    {
        
    }

}

