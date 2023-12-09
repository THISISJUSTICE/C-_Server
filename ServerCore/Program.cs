using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        //이 방식을 사용하면 쓰레드에서 쓰기가 사용되면 값이 변경
        //static string ThreadName;

        //쓰기가 공용되지 않게 됨(쓰레드에서 쓰기가 사용되도 그것은 쓰레드 내부에서만 변경됨)
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My name is {Thread.CurrentThread.ManagedThreadId}"; }); //함수를 매개변수로 주어지면, 해당 부분이 중복되면 실행되지 않음

        static void WhoAmI() {
            /*ThreadName = $"My name is {Thread.CurrentThread.ManagedThreadId}";

            Thread.Sleep(1000);

            Console.WriteLine(ThreadName);*/


           /* ThreadName.Value = $"My name is {Thread.CurrentThread.ManagedThreadId}";

            Thread.Sleep(1000);

            Console.WriteLine(ThreadName.Value);*/

            bool repeat = ThreadName.IsValueCreated;
            if (repeat)
                Console.WriteLine(ThreadName.Value + " (repeat)");
            else 
                Console.WriteLine(ThreadName.Value);
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            ThreadName.Dispose(); //사용이 끝난 이후 해제
        }
    }
}
