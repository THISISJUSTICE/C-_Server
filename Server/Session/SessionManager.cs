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

        int _sessionID = 0;

        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

        object lock_ = new object();

        public ClientSession Generate() {
            lock (lock_)
            {
                int sessionID = ++_sessionID;
                ClientSession session = new ClientSession();
                session.sessionID = sessionID;
                _sessions.Add(sessionID, session);

                Console.WriteLine($"Connected : {sessionID}");
                return session;
            }
        }

        
        public ClientSession Find(int id) {
            lock (lock_) {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(ClientSession session) {
            lock (lock_) {
                _sessions.Remove(session.sessionID);
            }
        }

    }
}
