using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static int number = 0;
        static object obj = new object();

        static void Thread1() {
            for (int i = 0; i < 1000000; i++) {
                /*//상호배제 Mutual Exclusive
                Monitor.Enter(obj); //문을 잠그는 행위

                number++;

                Monitor.Exit(obj); //잠금 해제 (이 부분이 실행되지 않는 경우: 데드락 DeadLock)*/

                //데드락 방지의 한 예시
                /*try
                {
                    Monitor.Enter(obj);

                    number++;

                    return;
                }

                finally {
                    Monitor.Exit(obj);
                }*/

                //try-finally 구문의 단순화
                lock (obj) {
                    number++;
                }
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 1000000; i++) {
                /*Monitor.Enter(obj);
                number--;
                Monitor.Exit(obj);*/

                lock (obj)
                {
                    number--;
                }
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread1);
            Task t2 = new Task(Thread2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);

        }
    }
}
