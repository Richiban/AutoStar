using System;

namespace AutoStar.EnumClass.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Enum-class Validation ====");

            Service service = new Service.TypeA();

            switch (service)
            {
                case Service.TypeA a: Console.WriteLine("We have an A"); break;
                case Service.TypeB b: Console.WriteLine("We have a B"); break;
                
                default: Console.WriteLine("We have an unknown service"); break;
            }
        }
    }
}

