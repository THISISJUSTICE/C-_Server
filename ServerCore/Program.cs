using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{

    class SessionManager { 
        static object lock_ = new object();

        public static void TestSession()
        {
            
            lock (lock_)
            {

            }
        }

        public static void Test()
        {
            lock (lock_)
            {
                UserManager.TestUser();
            }
        }
    }

    class UserManager
    {
        static object lock_ = new object();

        public static void Test() {
            lock (lock_) {
                SessionManager.TestSession();
            }
        }

        public static void TestUser()
        {
            lock (lock_)
            {
                
            }
        }
    }

    class Program
    {
        static int number = 0;
        static object obj = new object();

        static void Thread1() {
            for (int i = 0; i < 10000; i++) {
                SessionManager.Test();
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 100; i++) {
                UserManager.Test();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread1);
            Task t2 = new Task(Thread2);

            t1.Start();

            Thread.Sleep(100);

            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);

        }
    }
}
