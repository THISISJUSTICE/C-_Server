using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    struct JoTimerElem : IComparable<JoTimerElem>
    {
        public int execTick; //실행 시간
        public Action action;
        
        //CompareTo 함수를 호출할 때 비교
        public int CompareTo(JoTimerElem other)
        {
            return other.execTick - execTick;
        }
    }
    class JobTimer
    {
        PriorityQueue<JoTimerElem> pq_ = new PriorityQueue<JoTimerElem>();
        object lock_ = new object();

        public static JobTimer Inst { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0) {
            JoTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter;
            job.action = action;

            lock (lock_) {
                pq_.Push(job);
            }
        }

        public void Flush() {
            while (true) { 
                int now = System.Environment.TickCount;

                JoTimerElem job;

                lock (lock_) {
                    if (pq_.Count == 0) break;

                    job = pq_.Peek();
                    if (job.execTick > now) break;

                    pq_.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
