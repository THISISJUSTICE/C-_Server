using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using Server.DB;
using ServerCore;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
		public int AccountDbId { get; private set; }
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket) {
			// TODO : 보안 체크
			if (ServerState != PlayerServerState.ServerStateLogin) 
				return;

			LobbyPlayers.Clear();

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
					// AccountDbId 메모리에 할당
					AccountDbId = findAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					foreach (PlayerDb playerDb in findAccount.Players) {
						LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo() {
							PlayerDbId = playerDb.PlayerDbId,
							Name = playerDb.PlayerName,
							StatInfo = new StatInfo() { 
								Level = playerDb.Level,
								Hp = playerDb.Hp,
								MaxHP = playerDb.MaxHp,
								Attack = playerDb.Attack,
								Speed = playerDb.Speed,
								TotalExp = playerDb.TotalExp
							}
						};

						// 메모리에 할당
						LobbyPlayers.Add(lobbyPlayer);

						// 패킷화
						loginOk.Players.Add(lobbyPlayer);
					}
					Send(loginOk);

					ServerState = PlayerServerState.ServerStateLobby;
				}
				else
				{
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueID };
					db.Accounts.Add(newAccount);

					bool success = db.SaveChangesEx();
					if (success == false)
					{
						return;
					}

					// AccountDbId 메모리에 할당
					AccountDbId = findAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
					ServerState = PlayerServerState.ServerStateLobby;
				}
			}
		}

		public void HandleEnterGame(C_EnterGame enterGamePacket) {
			if (ServerState != PlayerServerState.ServerStateLobby) return;

			LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
			if (playerInfo == null) return;

			MyPlayer = ObjectManager.Instance.Add<Player>();
			{
				MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
				MyPlayer.Info.Name = playerInfo.Name;
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;
				MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
				MyPlayer.Session = this;

				S_ItemList itemListPacket = new S_ItemList();

				using (AppDbContext db = new AppDbContext()) {
					List<ItemDb> items = db.Items
						.Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
						.ToList();

					foreach (ItemDb itemDb in items) {
						Item item = Item.MakeItem(itemDb);
						if (item != null) {
							MyPlayer.Inven.Add(item);
							ItemInfo info = new ItemInfo();
							info.MergeFrom(item.info);
							itemListPacket.Items.Add(info);
						}
						
					}					
				}

				Send(itemListPacket);
			}

			ServerState = PlayerServerState.ServerStateGame;

			// TODO: 입장 요청이 들어오면
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.EnterGame, MyPlayer);
		}

		public void HandleCreatePlayer(C_CreatePlayer createPacket) {
			// TODO : 보안 체크
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			using (AppDbContext db = new AppDbContext()) {
				PlayerDb findPlayer = db.Players
					.Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

				if (findPlayer != null) // 이름 중복
				{
					Send(new S_CreatePlayer());
				}
				else {
					// 1레벨 스탯 정보 추출
					StatInfo stat = null;
					DataManager.StatDict.TryGetValue(1, out stat);

					// DB에 플레이어 데이터 생성
					PlayerDb newPlayerDb = new PlayerDb()
					{
						PlayerName = createPacket.Name,
						Level = stat.Level,
						Hp = stat.Hp,
						MaxHp = stat.MaxHP,
						Attack = stat.Attack,
						Speed = stat.Speed,
						TotalExp = 0,
						AccountDbId = AccountDbId
					};

					db.Players.Add(newPlayerDb);

					bool success = db.SaveChangesEx();
					if (success == false) {
						return;
					}

					// 메모리에 할당
					LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
					{
						PlayerDbId = newPlayerDb.PlayerDbId,
						Name = createPacket.Name,
						StatInfo = new StatInfo()
						{
							Level = stat.Level,
							Hp = stat.Hp,
							MaxHP = stat.MaxHP,
							Attack = stat.Attack,
							Speed = stat.Speed,
							TotalExp = 0
						}
					};

					// 메모리에 할당
					LobbyPlayers.Add(lobbyPlayer);

					S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
					newPlayer.Player.MergeFrom(lobbyPlayer);

					Send(newPlayer);
				}
			}
		}

    }


}
