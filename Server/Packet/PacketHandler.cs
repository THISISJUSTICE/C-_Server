using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class PacketHandler
    {
        public static void PlayerInfoHandler(PacketSession session, IPacket packet)
        {
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"[From Client] Player ID: {p.playerID}, {p.name}");

            foreach (PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"[From Client] skill: {skill.id}, {skill.level}, {skill.duration}");
            }
        }
    }
}
