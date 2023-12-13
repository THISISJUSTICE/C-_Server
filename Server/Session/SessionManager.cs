using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class SessionManager
    {
        static SessionManager session_ = new SessionManager();
        public static SessionManager Inst { get { return session_; } }

        int sessionID_ = 0;

        Dictionary<int, ClientSession> sessions_ = new Dictionary<int, ClientSession>();

        object lock_ = new object();

        public ClientSession Generate() {
            lock (lock_)
            {
                int sessionID = ++sessionID_;
                ClientSession session = new ClientSession();
                session.sessionID = sessionID;
                sessions_.Add(sessionID, session);

                Console.WriteLine($"Connected : {sessionID}");
                return session;
            }
        }

        
        public ClientSession Find(int id) {
            lock (lock_) {
                ClientSession session = null;
                sessions_.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(ClientSession session) {
            lock (lock_) {
                sessions_.Remove(session.sessionID);
            }
        }

    }
}
