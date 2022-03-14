using System;

namespace Sample
{
    [BuilderPattern]
    public partial class Person
    {
        public Person(string firstName, string lastName, DateTime? birthDate, string? a, string b)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            A = a;
            B = b;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public DateTime? BirthDate { get; }
        public string? A { get; }
        public string B { get; } = "B val";
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("==== Builder Validation ====");

            try
            {
                var myDataClass = new Person.Builder
                {
                    FirstName = "Alex"
                }.Build();

                Console.WriteLine(myDataClass);
            }
            catch (Person.BuilderException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}