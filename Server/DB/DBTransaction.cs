using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Game;

namespace Server.DB
{
    public class DBTransaction : JobSerializer
    {
        public static DBTransaction Instance { get; } = new DBTransaction();

        // Me (GameRoom) -> You (Db) -> Me (GameRoom)
        public static void SavePlayerStatus_AllInOne(Player player, GameRoom room) {
            if (player == null || room == null) return;

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success) {
                        room.Push(() => Console.WriteLine($"Hp Saved({playerDb.Hp})"));
                    }
                }
            });
        }

        // Me (GameRoom)
        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null) return;

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);

        }

        // You(Db)
        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room){
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        room.Push(SavePlayerStatus_Step3, playerDb.Hp);
                    }
                }
            });
        }

        // Me (GameRoom)
        public static void SavePlayerStatus_Step3(int hp) {
            Console.WriteLine($"Hp Saved({hp})");
        }

    }
}
