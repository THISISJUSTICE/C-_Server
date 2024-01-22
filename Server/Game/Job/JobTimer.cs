using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server.Game
{
    struct JoTimerElem : IComparable<JoTimerElem>
    {
        public int execTick; //실행 시간
        public IJob job;
        
        //CompareTo 함수를 호출할 때 비교
        public int CompareTo(JoTimerElem other)
        {
            return other.execTick - execTick;
        }
    }
    public class JobTimer
    {
        PriorityQueue<JoTimerElem> _pq = new PriorityQueue<JoTimerElem>();
        object _lock = new object();

        public void Push(IJob action, int tickAfter = 0) {
            JoTimerElem joTimerElem;
            joTimerElem.execTick = System.Environment.TickCount + tickAfter;
            joTimerElem.job = action;

            lock (_lock) {
                _pq.Push(joTimerElem);
            }
        }

        public void Flush() {
            while (true) { 
                int now = System.Environment.TickCount;

                JoTimerElem joTimerElem;

                lock (_lock) {
                    if (_pq.Count == 0) break;

                    joTimerElem = _pq.Peek();
                    if (joTimerElem.execTick > now) break;

                    _pq.Pop();
                }

                joTimerElem.job.Execute();
            }
        }
    }
}
