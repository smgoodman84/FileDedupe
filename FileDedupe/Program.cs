using System;

namespace FileDedupe
{
    class Program
    {
        static void Main(string[] args)
        {
            var directoryReader = new DirectoryReader();

            foreach (var file in directoryReader.Read(@"C:\Data\GitHub\FileDedupe\TestData"))
            {
                Console.WriteLine(file.FullName);
            }

            Console.ReadKey();
        }
    }
}
