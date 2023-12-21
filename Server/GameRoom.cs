using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> sessions_ = new List<ClientSession>();
        JobQueue jobQ_ = new JobQueue();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job) {
            jobQ_.Push(job);
        }

        public void Flush() {
            foreach (ClientSession cs in sessions_)
            {
                cs.Send(pendingList);
            }
            //Console.WriteLine($"Flushed {pendingList.Count} itmes");
            pendingList.Clear();
        }

        public void BroadCast(ArraySegment<byte> segment) {
            pendingList.Add(segment);
        }

        //플레이어 추가
        public void Enter(ClientSession session) {
            
            sessions_.Add(session);
            session.room = this;

            //플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();
            foreach (ClientSession s in sessions_) {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerID = s.sessionID,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,

                }) ;
            }
            session.Send(players.Write());

            //플레이어 입장을 브로드캐스트
            S_BroadCastEnterGame enter = new S_BroadCastEnterGame();
            enter.playerID = session.sessionID;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            BroadCast(enter.Write());
        }

        //플레이어 제거
        public void Leave(ClientSession session)
        {
            sessions_.Remove(session);

            //브로드 캐스트
            S_BroadCastLeaveGame leave = new S_BroadCastLeaveGame();
            leave.playerID = session.sessionID;
            BroadCast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet) {
            //좌표를 바꾸기
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            //브로드 캐스팅
            S_BroadCastMove move = new S_BroadCastMove();
            move.playerID = session.sessionID;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            BroadCast(move.Write());
        }

    }
}
