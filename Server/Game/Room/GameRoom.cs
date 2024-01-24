using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class GameRoom : JobSerializer
    {
        public int RoomID { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; private set; } = new Map();

        public void Init(int mapID)
        {
            Map.LoadMap(mapID);

            //TEMP
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.CellPos = new Vector2Int(5, 5);
            EnterGame(monster);
        }

        public void Update() {
            foreach (Monster monster in _monsters.Values) {
                monster.Update();
            }
            foreach (Projectile projectile in _projectiles.Values) {
                projectile.Update();
            }

            Flush();
        }

        public void EnterGame(GameObject gameObject) {
            if (gameObject == null) return;

            GameObjectType type = ObjectManager.GetObjectTypeByID(gameObject.id);

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;

                _players.Add(gameObject.id, player);
                player.Room = this;

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

                //본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                        {
                            spawnPacket.Objects.Add(p.Info);
                        }
                    }
                    foreach (Monster m in _monsters.Values) {
                        spawnPacket.Objects.Add(m.Info);
                    }
                    foreach (Projectile p in _projectiles.Values)
                    {
                        spawnPacket.Objects.Add(p.Info);
                    }

                    player.Session.Send(spawnPacket);
                }
            }

            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.id, monster);
                monster.Room = this;

                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));
            }

            else if (type == GameObjectType.Projectile) {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.id, projectile);
                projectile.Room = this;
            } 

            //타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach (Player p in _players.Values) {
                    if (p.id != gameObject.id)
                        p.Session.Send(spawnPacket);
                }
            }
            
        }

        public void LeaveGame(int objectID) {
            GameObjectType type = ObjectManager.GetObjectTypeByID(objectID);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectID, out player) == false)
                {
                    return;
                }

                Map.ApplyLeave(player);
                player.Room = null;

                //본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }

            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.Remove(objectID, out monster) == false)
                {
                    return;
                }
                
                Map.ApplyLeave(monster);
                monster.Room = null;
            }

            else if (type == GameObjectType.Projectile) {
                Projectile projectile = null;
                if (_projectiles.Remove(objectID, out projectile) == false)
                {
                    return;
                }
                projectile.Room = null;
            }

            //타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIDs.Add(objectID);
                foreach (Player p in _players.Values)
                {
                    if(p.id != objectID)
                        p.Session.Send(despawnPacket);
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        { 
            if (player == null) return;

            //TODO : 검증

            //일단 서버에서 좌표 이동
            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            //다른 좌표로 이동할 경우, 갈 수 있는지 체크
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY) {
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false) {
                    return;
                }
            }

            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            //다른 플레이어에게 브로드캐스팅
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectID = player.Info.ObjectID;
            resMovePacket.PosInfo = info.PosInfo;

            BroadCast(resMovePacket);
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null) return;

            ObjectInfo info = player.Info;
            if (info.PosInfo.State != CreatureState.Idle) return;

            //TODO: 스킬 사용 가능 여부 체크

            //통과
            info.PosInfo.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectID = info.ObjectID;
            skill.Info.SkillID = skillPacket.Info.SkillID;
            BroadCast(skill);

            Data.Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillID, out skillData) == false) return;

            switch(skillData.skillType){
                case SkillType.SkillAuto:
                    //TODO: 데미지 판정
                    Vector2Int skillPos = player.GetFrontPos(info.PosInfo.MoveDir);
                    GameObject target = Map.Find(skillPos);
                    if (target != null)
                    {
                        Console.WriteLine($"{target.id} Hit GameObject");
                    }
                    break;
                case SkillType.SkillProjectile:
                    Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                    if (arrow == null) return;

                    arrow.Owner = player;
                    arrow.Data = skillData;

                    arrow.PosInfo.State = CreatureState.Moving;
                    arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                    arrow.PosInfo.PosX = player.PosInfo.PosX;
                    arrow.PosInfo.PosY = player.PosInfo.PosY;
                    arrow.Speed = skillData.projectile.speed;
                    Push(EnterGame, arrow);
                    break;
            }
            
        }

        public Player FindPlayer(Func<GameObject, bool> condition) {
            foreach (Player player in _players.Values) {
                if (condition(player)) return player;
            }
            return null;
        }

        public void BroadCast(IMessage packet) {
            foreach (Player p in _players.Values) {
                p.Session.Send(packet);
            }
        }

    }
}
