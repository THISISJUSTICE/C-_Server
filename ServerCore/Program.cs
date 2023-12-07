using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        //메모리 배리어
        //- 코드 재배치 억제
        //- 가시성
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;

        static void Thread1()
        {
            y = 1; //Store y

            //-----------------------------
            Thread.MemoryBarrier(); //멀티 스레드에서만 적용된 값을 커밋하는 역할도 겸함(Store의 커밋)
            r1 = x; //Load x
        }

        static void Thread2()
        {
            x = 1; //Store x

            //-----------------------------
            Thread.MemoryBarrier();
            r2 = y; //Load y
        }

        static void Main(string[] args)
        {
            int count = 0;
            while (true)
            {
                count++;
                x = y = r1 = r2 = 0;

                Task t1 = new Task(Thread1);
                Task t2 = new Task(Thread2);
                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);

                //메모리 배리어를 하지 않으면 코드의 순서가 바뀌는 경우가 있을 수 있어 해당 사항이 나올 수 있음
                if (r1 == 0 && r2 == 0) break;
            }

            Console.WriteLine($"{count}번 만에 빠져나옴");
        }
    }
}
