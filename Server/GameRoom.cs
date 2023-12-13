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

        public void Push(Action job) {
            jobQ_.Push(job);
        }

        public void BroadCast(ClientSession session, string chat) {
            S_Chat packet = new S_Chat();

            packet.playerID = session.sessionID;
            packet.chat = $"{chat}, I am {packet.playerID}!";

            ArraySegment<byte> segment = packet.Write();
            foreach (ClientSession cs in sessions_)
            {
                cs.Send(segment);
            }
        }

        public void Enter(ClientSession session) {
            sessions_.Add(session);
            session.room = this;
        }

        public void Leave(ClientSession session)
        {
            sessions_.Remove(session);
        }

    }
}
