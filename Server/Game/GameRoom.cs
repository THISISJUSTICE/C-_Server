﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomID { get; set; }

        List<Player> _players = new List<Player>();

        public void EnterGame(Player newPlayer) {
            if (newPlayer == null) return;

            lock(_lock){
                _players.Add(newPlayer);
                newPlayer.Room = this;

                //본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players) {
                        if (newPlayer != p) {
                            spawnPacket.Player.Add(p.Info);
                        }
                    }

                    newPlayer.Session.Send(spawnPacket);
                }

                //타인한테 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Player.Add(newPlayer.Info);
                    foreach (Player p in _players) {
                        if (newPlayer != p)
                            p.Session.Send(spawnPacket);
                    }
                }
            }
            
        }

        public void LeaveGame(int playerID) {
            lock (_lock) {
                Player player = _players.Find(p => p.Info.PlayerID == playerID);
                if (player == null) return;

                _players.Remove(player);
                player.Room = null;

                //본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                //타인한테 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerID.Add(player.Info.PlayerID);
                    foreach (Player p in _players)
                    {
                        if(player != p)
                            p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        { 
            if (player == null) return;

            lock (_lock){
                //TODO : 검증

                PlayerInfo info = player.Info;
                info.PosInfo = movePacket.PosInfo;

                //다른 플레이어에게 브로드캐스팅
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerID = player.Info.PlayerID;
                resMovePacket.PosInfo = movePacket.PosInfo;

                BroadCast(resMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null) return;

            lock (_lock) {
                PlayerInfo info = player.Info;
                if (info.PosInfo.State != CreatureState.Idle) return;

                //TODO: 스킬 사용 가능 여부 체크

                //통과
                info.PosInfo.State = CreatureState.Skill;
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.PlayerID = info.PlayerID;
                skill.Info.SkillID = 1;
                BroadCast(skill);

                //TODO: 데미지 판정
            }
        }

        public void BroadCast(IMessage packet) {
            lock (_lock) {
                foreach (Player p in _players) {
                    p.Session.Send(packet);
                }
            }
        }

    }
}
