﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;


class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}," +
			$" Dir: {movePacket.PosInfo.MoveDir}, State: {movePacket.PosInfo.State})");

		Player player = clientSession.MyPlayer;
		if (player == null) return;
		GameRoom room = player.Room;
		if (room == null) return;

		room.HandleMove(player, movePacket);
	}

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
		C_Skill skillPacket = packet as C_Skill;
		ClientSession clientSession = session as ClientSession;

		Player player = clientSession.MyPlayer;
		if (player == null) return;
		GameRoom room = player.Room;
		if (room == null) return;

		room.HandleSkill(player, skillPacket);
    }

}

