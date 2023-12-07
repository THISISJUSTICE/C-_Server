using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program {
        static bool stop = false;
        static void Thread1()
        {
            Console.WriteLine("쓰레드 시작");
            while(!stop){

            }
            Console.WriteLine("쓰레드 끝");
        }

        static void Main(string[] args)
        {
            Task t = new Task(Thread1);
            t.Start();
            stop = true;
            //Thread1의 무한 반복이 끝나지 않게 됨
        }
    }
    
}
