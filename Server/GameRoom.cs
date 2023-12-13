using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> sessions_ = new List<ClientSession>();
        object lock_ = new object();

        public void BroadCast(ClientSession session, string chat) {
            S_Chat packet = new S_Chat();

            packet.playerID = session.sessionID;
            packet.chat = $"{chat}, I am {packet.playerID}!";

            ArraySegment<byte> segment = packet.Write();

            lock (lock_) {
                foreach (ClientSession cs in sessions_) {
                    cs.Send(segment);
                }
            }
        }

        public void Enter(ClientSession session) {
            lock (lock_) {
                sessions_.Add(session);
                session.room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (lock_) {
                sessions_.Remove(session);
            }
            
        }
    }
}
