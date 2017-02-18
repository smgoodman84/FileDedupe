using System;
using System.Linq;

namespace FileDedupe
{
    class Program
    {
        static void Main(string[] args)
        {
            var directoryReader = new DirectoryReader();
            var database = new Database.Database("FileDedupe.sqlite");

            var files = directoryReader.Read(@"C:\Data\GitHub\FileDedupe\TestData")
                .Where(f => !database.IsFileHashSaved(f))
                .Select(HashCalculator.CalculateHash);

            foreach (var file in files)
            {
                Console.WriteLine($"Saving {file.FullName}");
                database.SaveFileInfo(file);
            }

            //foreach (var file in database.ReadAllFileInfo())
            //{
            //    Console.WriteLine($"{file.FullName} {file.Hash}");
            //}

            Console.WriteLine("Directories With Duplicate Files");
            
            foreach (var file in database.GetDuplicateDirectoryInfo())
            {
                Console.WriteLine($"{file.Directory1} {file.Directory2} {file.GetMatchType()}");
            }

            Console.ReadKey();
        }
    }
}
