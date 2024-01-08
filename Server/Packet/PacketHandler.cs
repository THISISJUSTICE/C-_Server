using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;


class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}," +
			$" Dir: {movePacket.PosInfo.MoveDir}, State: {movePacket.PosInfo.State})");

		if (clientSession.MyPlayer == null || clientSession.MyPlayer.Room == null) return;

		//TODO : 검증

		PlayerInfo info = clientSession.MyPlayer.Info;
		info.PosInfo = movePacket.PosInfo;

		//다른 플레이어에게 브로드캐스팅
		S_Move resMovePacket = new S_Move();
		resMovePacket.PlayerID = clientSession.MyPlayer.Info.PlayerID;
		resMovePacket.PosInfo = movePacket.PosInfo;

		clientSession.MyPlayer.Room.BroadCast(resMovePacket, id:clientSession.MyPlayer.Info.PlayerID);
	}

}

