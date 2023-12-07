using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program {
        static void Main(string[] args)
        {
            int[,] arr = new int[10000, 10000];

            {
                for (int i = 0; i < 10000; i++)
                    for (int j = 0; j < 10000; j++) arr[i, j] = 1;
            }

            // 캐시의 공간 지역성으로 인해 위의 경우가 더 빠름

            {
                for (int i = 0; i < 10000; i++)
                    for (int j = 0; j < 10000; j++) arr[j, i] = 1;
            }
        }
    }
    
}
