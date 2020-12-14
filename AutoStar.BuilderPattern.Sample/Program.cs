using System;

namespace Sample
{
    [BuilderPattern]
    public partial class Person
    {
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime? BirthDate { get; }
        public string? A { get; }
        public string B { get; } = "B val";
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Builder Validation ====");
            try
            {
                var myDataClass = new Person.Builder
                {
                    FirstName = "Alex",
                    LastName = "Bloggs"
                }
                    .Build();

                Console.WriteLine(myDataClass);
            }
            catch (Person.Builder.ValidationException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

