using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.Game;
using ServerCore;


class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		/*Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}," +
			$" Dir: {movePacket.PosInfo.MoveDir}, State: {movePacket.PosInfo.State})");*/

		Player player = clientSession.MyPlayer;
		if (player == null) return;
		GameRoom room = player.Room;
		if (room == null) return;

		room.Push(room.HandleMove, player, movePacket);
	}

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
		C_Skill skillPacket = packet as C_Skill;
		ClientSession clientSession = session as ClientSession;

		Player player = clientSession.MyPlayer;
		if (player == null) return;
		GameRoom room = player.Room;
		if (room == null) return;

		room.Push(room.HandleSkill, player, skillPacket);
    }

	public static void C_LoginHandler(PacketSession session, IMessage packet)
	{
		C_Login loginPacket = packet as C_Login;
		ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"Unique Id({loginPacket.UniqueID})");

		// TODO
		using (AppDbContext db = new AppDbContext()) {
			AccountDb findAccount = db.Accounts
				.Where(a => a.AccountName == loginPacket.UniqueID).FirstOrDefault();

			if (findAccount != null)
			{
				S_Login loginOk = new S_Login() { LoginOk = 1 };
				clientSession.Send(loginOk);
			}
			else {
				AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueID };
				db.Accounts.Add(newAccount);
				db.SaveChanges();

				S_Login loginOk = new S_Login() { LoginOk = 1 };
				clientSession.Send(loginOk);
			}

		}
	}
	

}

