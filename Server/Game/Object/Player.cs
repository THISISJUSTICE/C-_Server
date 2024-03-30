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
            // - 비동기 방법
            // - 다른 쓰레드로 DB 처리
            // -- 결과를 받아서 이어서 처리를 해야하는 경우가 많음 ( Ex)아이템 생성 )
            //DBTransaction.SavePlayerStatus_AllInOne(this, Room);
            DBTransaction.SavePlayerStatus_Step1(this, Room);
        }

    }
}
