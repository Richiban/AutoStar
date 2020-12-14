using System;

namespace Sample
{
    [PrimaryConstructor]
    partial class Service
    {
        private readonly Guid _data;

        public string PrintData() => _data.ToString();
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Auto-constructor Validation ====");

            //var myDataClass = new Service(Guid.NewGuid());

            //Console.WriteLine(myDataClass.PrintData());
        }
    }
}

