using System;

namespace AutoStar.PrimaryConstructor.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Auto-constructor Validation ====");

            var myDataClass = new Service(Guid.NewGuid(), "srtras");
            
            Console.WriteLine(myDataClass);
        }
    }
}

