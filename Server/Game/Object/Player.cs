using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }

        public Player() {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
                       
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }

        public void OnLeaveGame() {
            // TODO
            // DB 연동
            // 부하를 줄여야 함
            // 1) 서버가 다운되면 저장되지 않은 정보 소실
            // 2) 코드 흐름을 다 막아버림 (멀티 스레드 환경에서 시간 소모)
            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb playerDb = new PlayerDb();
                playerDb.PlayerDbId = PlayerDbId;
                playerDb.Hp = Stat.Hp;

                // Find로 찾는 것 보다 DB 접근 횟수가 줄어듦
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                db.SaveChangesEx();
            }
        }

    }
}
