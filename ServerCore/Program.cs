using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    /*class EventLock
    {
        //bool <- 커널
        AutoResetEvent available_ = new AutoResetEvent(true);
        //ManualResetEvent available_ = new ManualResetEvent(true);

        public void Acquire()
        {
            available_.WaitOne(); //입장 시도
            //available_.Reset();

        }

        public void Release()
        {
            available_.Set();
        }
    }*/

    class Program
    {
        static int num = 0;

        //int, Thread ID
        static Mutex lock_ = new Mutex();

        static void Thread1() {
            for (int i = 0; i < 100000; i++) {
                lock_.WaitOne();
                num++;
                lock_.ReleaseMutex();
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 100000; i++)
            {
                lock_.WaitOne();
                num--;
                lock_.ReleaseMutex();
            }
        }

        static void Main(string[] args)
        {
            DateTime pre = DateTime.Now;
            Task t1 = new Task(Thread1);
            Task t2 = new Task(Thread2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            DateTime now = DateTime.Now;

            TimeSpan executionTime = now - pre;
            Console.WriteLine(num);
            Console.WriteLine($"걸린 시간: {executionTime.TotalMilliseconds}");
        }
    }
}
