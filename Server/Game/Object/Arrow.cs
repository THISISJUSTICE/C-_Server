using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    class Arrow : Projectile
    {
        public GameObject Owner { get; set; }
        long _nextMoveTick = 0;

        public override void Update()
        {
            if (Data == null || Data.projectile == null || Owner == null || Room == null) return;

            if (_nextMoveTick >= Environment.TickCount64) return;

            long tick = (long)(1000 / Data.projectile.speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            Vector2Int destPos = GetFrontPos();

            if (Room.Map.CanGo(destPos))
            {
                CellPos = destPos;
                S_Move movePacket = new S_Move();
                movePacket.ObjectID = id;
                movePacket.PosInfo = PosInfo;
                Room.BroadCast(movePacket);
            }
            else {
                GameObject target = Room.Map.Find(destPos);
                if (target != null) {
                    //TODO: 피격 판정
                    target.OnDamaged(this, Data.damage + Owner.Stat.Attack);
                    Console.WriteLine($"{target.Info.Name} 피격, damage: {Data.damage}");
                    
                }

                //소멸
                Room.LeaveGame(id);
            }
        }

    }
}
