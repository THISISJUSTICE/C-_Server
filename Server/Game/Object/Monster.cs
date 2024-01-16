using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    class Monster : GameObject
    {
        public Monster() {
            ObjectType = GameObjectType.Monster;

            //TEMP
            Stat.Level = 1;
            Stat.MaxHP = 100;
            Stat.Hp = Stat.MaxHP;
            Stat.Speed = 5.0f;

            State = CreatureState.Idle;
        }

        //FSM(Finite State Machine)
        public override void Update() {
            switch (State) {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }
        }

        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;

        long _nextSearchTick = 0;
        protected virtual void UpdateIdle() {
            if (_nextSearchTick > Environment.TickCount64) return;

            _nextSearchTick = Environment.TickCount64 + 1000;

            Player target = Room.FindPlayer(p =>
            {
                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero < _searchCellDist;
            });

            if (target == null) return;

            _target = target;
            State = CreatureState.Moving;
        }

        long _nextMoveTick = 0;
        protected virtual void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64) return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + 1000;

            if (_target == null || _target.Room != Room) {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            int dist = (_target.CellPos - CellPos).cellDistFromZero;
            if (dist == 0 || dist > _chaseCellDist) {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: false);
            if (path.Count < 2 || path.Count > _chaseCellDist) {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            //이동
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);

            //다른 플레이어에게 브로드캐스팅
            S_Move movePacket = new S_Move();
            movePacket.ObjectID = id;
            movePacket.PosInfo = PosInfo;
            Room.BroadCast(movePacket);
        }

        protected virtual void UpdateSkill()
        {

        }

        protected virtual void UpdateDead()
        {

        }

    }
}
