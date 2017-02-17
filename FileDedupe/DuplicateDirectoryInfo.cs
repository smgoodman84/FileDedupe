using System.Collections.Generic;
using System.Linq;

namespace FileDedupe
{
    class DuplicateDirectoryInfo
    {
        public string Directory1 { get; }
        public string Directory2 { get; }

        public DuplicateDirectoryInfo(string directory1, string directory2)
        {
            Directory1 = directory1;
            Directory2 = directory2;

            FilesInDirectory1ButNot2 = new List<ExtendedFileInfo>();
            FilesInDirectory2ButNot1 = new List<ExtendedFileInfo>();
        }

        public string GetMatchType()
        {
            if (FilesInDirectory1ButNot2.Any())
            {
                if (FilesInDirectory2ButNot1.Any())
                {
                    return "Intersection";
                }

                return "Directory1 > Directory2";
            }

            if (FilesInDirectory2ButNot1.Any())
            {
                return "Directory2 > Directory1";
            }

            return "Identical";
        }

        public List<ExtendedFileInfo> FilesInDirectory1ButNot2 { get; set; }
        public List<ExtendedFileInfo> FilesInDirectory2ButNot1 { get; set; }
    }
}
