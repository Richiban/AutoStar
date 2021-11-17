using System;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Enum-class Validation ====");

            AutoStar.EnumClass.Sample.Service service = new AutoStar.EnumClass.Sample.Service.TypeA();

            switch (service)
            {
                case AutoStar.EnumClass.Sample.Service.TypeA a: Console.WriteLine("We have an A"); break;
                case AutoStar.EnumClass.Sample.Service.TypeB b: Console.WriteLine("We have a B"); break;
                
                default: Console.WriteLine("We have an unknown service"); break;
            }
        }
    }
}

