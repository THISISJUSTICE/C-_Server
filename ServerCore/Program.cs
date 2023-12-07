using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static int number = 0;

        //atiominc = 원자성(어떤 동작이 한 번에 일어나야 함)

        static void Thread1() {
            for (int i = 0; i < 1000000; i++) {
                //All or Nothing
                int afterval = Interlocked.Increment(ref number); //메모리 배리어도 사용됨, 캐시의 개념이 쓸모 없어짐
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 1000000; i++) {
                Interlocked.Decrement(ref number);
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
