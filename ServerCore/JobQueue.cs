using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public interface IJobQueue {
        public void Push(Action job);
    }
    public class JobQueue: IJobQueue
    {
        Queue<Action> jobQueue_ = new Queue<Action>();
        object lock_ = new object();
        bool flush_ = false;
        public void Push(Action job)
        {
            bool flush = false;
            lock (lock_) {
                jobQueue_.Enqueue(job);
                if (!flush_) flush_ = flush = true;
            }
            if (flush) Flush();
            
        }

        void Flush() {
            while (true) {
                Action action = Pop();
                if (action == null) return;

                action.Invoke();
            }
        }

        Action Pop() {
            lock (lock_) {
                if (jobQueue_.Count == 0) {
                    flush_ = false;
                    return null;
                }
                    
                return jobQueue_.Dequeue();
            }
        }

    }
}
