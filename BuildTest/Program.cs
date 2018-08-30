using System;

namespace BuildTest
{
    class Program
    {
        static void Main(string[] args)
        {
#if !CUSTOM_DEFINE
#error CUSTOM_DEFINE was not defined
#endif
            Console.WriteLine("Hello World!");
        }
    }
}
