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

        public void SendForEach() {
            lock (lock_) {
                foreach (ServerSession s in sessions_) {
                    C_Chat chatPacket = new C_Chat();
                    chatPacket.chat = $"Hello Server!";
                    ArraySegment<byte> segment = chatPacket.Write();

                    s.Send(segment);
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
