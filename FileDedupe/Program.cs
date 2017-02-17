using System;
using System.Linq;

namespace FileDedupe
{
    class Program
    {
        static void Main(string[] args)
        {
            var directoryReader = new DirectoryReader();
            var database = new Database.Database();

            var files = directoryReader.Read(@"C:\Data\GitHub\FileDedupe\TestData")
                .Select(f => HashCalculator.CalculateHash(f));

            foreach (var file in files)
            {
                database.SaveFileInfo(file);
            }

            foreach (var file in database.ReadAllFileInfo())
            {
                Console.WriteLine($"{file.FullName} {file.Hash}");
            }

            Console.WriteLine("Directories With Duplicate Files");
            
            foreach (var file in database.GetDuplicateDirectoryInfo())
            {
                Console.WriteLine($"{file.Directory1} {file.Directory2} {file.GetMatchType()}");
            }

            Console.ReadKey();
        }
    }
}
