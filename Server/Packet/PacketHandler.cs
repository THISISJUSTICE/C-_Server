using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;


class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        C_PlayerInfoReq p = packet as C_PlayerInfoReq;

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

