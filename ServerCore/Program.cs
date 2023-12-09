using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        //상호배제
        //SpinLock (SpinLock과 Context Switching이 혼합된 형태
        //Mutex

        //불러오기를 할 땐 굳이 락을 하지 않아도 되고, 쓰기가 있을 때 락을 사용

        //읽기를 할 땐 상호배제 없이 획득, 쓰기를 할 땐 상호배제
        static ReaderWriterLockSlim lock3 = new ReaderWriterLockSlim();

        class Reward { 

        }

        static Reward GetReward(int id) {
            lock3.EnterReadLock();
            lock3.ExitReadLock();
            return null;
        }

        static void AddReward(Reward reward) {

            lock3.EnterWriteLock();
            lock3.ExitWriteLock();

        }

        static volatile int count;
        static Lock lock_ = new Lock();

        static void Main(string[] args)
        {
            Task t1 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++) {
                    lock_.WriteLock();
                    lock_.WriteLock();
                    count++;
                    lock_.WriteUnlock();
                    lock_.WriteUnlock();
                }
            });

            Task t2 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++)
                {
                    lock_.WriteLock();
                    count--;
                    lock_.WriteUnlock();
                }
            });

            DateTime pre = DateTime.Now;

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            DateTime now = DateTime.Now;

            Console.WriteLine(count);
            TimeSpan executionTime = now - pre;
            Console.WriteLine($"걸린 시간: {executionTime.TotalMilliseconds}");
        }
    }
}
