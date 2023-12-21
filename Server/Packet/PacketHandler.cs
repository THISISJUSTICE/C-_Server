﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;
using ServerCore;


class PacketHandler
{
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;

        if (clientSession.room == null) {
            return;
        }
        GameRoom room = clientSession.room;
        room.Push(() => room.Leave(clientSession));

    }

    public static void C_MoveHandler(PacketSession session, IPacket packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.room == null)
        {
            return;
        }

        GameRoom room = clientSession.room;
        room.Push(() => room.Move(clientSession, movePacket));

    }

}

