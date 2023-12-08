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

            /*Thread.Sleep(1); //무조건 휴식(1ms) 정도
            Thread.Sleep(0); //조건부 양보 (나보다 우선 순위가 낮은 스레드에게는 양보하지 않음
            Thread.Yield(); //관대한 양보 (실행이 가능한 스레드가 있으면 양보)*/

            Thread.Yield();
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
