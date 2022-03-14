using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace AutoStar.Tests.Common;

public class CodeDiffer
{
    public static void AssertEqual(string expected, string actual)
    {
        var diff = InlineDiffBuilder.Diff(expected, actual);

        var numDifferences = 0;

        foreach (var line in diff.Lines)
        {
            if (!String.IsNullOrWhiteSpace(line.Text) && line.Type != ChangeType.Unchanged)
            {
                numDifferences++;
            }

            switch (line.Type)
            {
                case ChangeType.Inserted:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("+ ");

                    break;
                case ChangeType.Deleted:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("- ");

                    break;
                case ChangeType.Modified:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("~ ");

                    break;
                default:
                    Console.ForegroundColor =
                        ConsoleColor.Gray; // compromise for dark or light background

                    Console.Write("  ");

                    break;
            }

            Console.WriteLine(line.Text);
        }

        if (numDifferences > 0)
        {
            Assert.Fail($"{numDifferences} differences found");
        }

    }
}