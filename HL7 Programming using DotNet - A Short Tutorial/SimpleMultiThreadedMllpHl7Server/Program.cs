using System;

namespace SimpleMultiThreadedMllpHl7Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            var main = new OurSimpleMultiThreadedMllpHl7Server();
            main.StartOurTcpServer(1080); 

            Console.WriteLine("Press any key to exit program...");
            Console.ReadLine();
        }
    }
}