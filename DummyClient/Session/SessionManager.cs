using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class SessionManager
    {
        static SessionManager session_ = new SessionManager();
        public static SessionManager Inst { get { return session_; } }

        List<ServerSession> sessions_ = new List<ServerSession>();
        object lock_ = new object();
        Random rand = new Random();

        public void SendForEach() {
            lock (lock_) {
                foreach (ServerSession s in sessions_) {
                    C_Move movePakcet = new C_Move();
                    movePakcet.posX = rand.Next(-50, 50);
                    movePakcet.posY = 0;
                    movePakcet.posZ = rand.Next(-50, 50);
                    s.Send(movePakcet.Write());
                }
                
            }
        }

        public ServerSession Generate() {
            lock (lock_) {
                ServerSession session = new ServerSession();
                sessions_.Add(session);
                return session;
            }
        }

    }
}
