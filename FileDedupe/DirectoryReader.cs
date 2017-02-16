using System.Collections.Generic;
using System.IO;

namespace FileDedupe
{
    class DirectoryReader
    {
        public IEnumerable<FileInfo> Read(string directory)
        {
            return Read(new DirectoryInfo(directory));
        }

        public IEnumerable<FileInfo> Read(DirectoryInfo directory)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                yield return file;
            }

            foreach (var subdirectory in directory.EnumerateDirectories())
            {
                foreach (var item in Read(subdirectory))
                {
                    yield return item;
                }
            }
        }
    }
}
