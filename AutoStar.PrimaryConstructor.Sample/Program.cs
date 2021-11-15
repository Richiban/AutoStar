using System;

namespace Sample
{
    [PrimaryConstructor]
    partial class Service
    {
        private readonly Guid _data;
        private readonly string _name;

        public override string ToString()
        {
            return $"{nameof(Service)}: {_name}\n{_data}";
        }
    }

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

