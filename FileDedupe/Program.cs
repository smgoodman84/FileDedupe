using System;
using System.Linq;

namespace FileDedupe
{
    class Program
    {
        static void Main(string[] args)
        {
            var directoryReader = new DirectoryReader();

            var files = directoryReader.Read(@"C:\Data\GitHub\FileDedupe\TestData")
                .Select(f => HashCalculator.CalculateHash(f));

            foreach (var file in files)
            {
                Console.WriteLine($"{file.FullName} {file.Hash}");
            }

            Console.ReadKey();
        }
    }
}
