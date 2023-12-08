using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class SpinLock
    {
        volatile int locked_ = 0;

        public void Acquire()
        {
            while (true)
            {
                /*int original = Interlocked.Exchange(ref locked_, 1);
                if (original == 0) break;*/

                //CAS(Compare and Swap)
                int expected = 0;
                int desired = 1;
                if (Interlocked.CompareExchange(ref locked_, desired, expected) == expected)
                    break;
            }
        }

        //별도 처리를 하지 않아도 됨
        public void Release()
        {
            locked_ = 0;
        }
    }

    class Program
    {
        static int num = 0;
        static SpinLock lock_ = new SpinLock();

        static void Thread1() {
            for (int i = 0; i < 1000000; i++) {
                lock_.Acquire();
                num++;
                lock_.Release();
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 1000000; i++)
            {
                lock_.Acquire();
                num--;
                lock_.Release();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread1);
            Task t2 = new Task(Thread2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(num);

        }
    }
}
