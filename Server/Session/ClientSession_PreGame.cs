using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using Server.DB;
using ServerCore;
using Microsoft.EntityFrameworkCore;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public void HandleLogin(C_Login loginPacket) {
			// TODO : 보안 체크
			if (ServerState != PlayerServerState.ServerStateLogin) 
				return;


			// - 동시에 다른 사람이 같은 UniqueID를 보내는 경우
			// - 같은 패킷을 여러 번 보내는 경우
			// - 맞지 않는 타이밍에 패킷을 보내는 경우
			using (AppDbContext db = new AppDbContext())
			{
				AccountDb findAccount = db.Accounts
					.Include(a => a.Players)
					.Where(a => a.AccountName == loginPacket.UniqueID).FirstOrDefault();

				if (findAccount != null)
				{
					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
				}
				else
				{
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueID };
					db.Accounts.Add(newAccount);
					db.SaveChanges(); // TODO: Exception

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
				}
			}
		}

    }


}
