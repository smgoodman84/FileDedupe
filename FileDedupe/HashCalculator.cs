using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FileDedupe
{
    class HashCalculator
    {
        public static ExtendedFileInfo CalculateHash(FileInfo file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file.FullName))
                {
                    var hash = md5.ComputeHash(stream);

                    var hashstring = string.Join("", hash.Select(b => b.ToString("x2")));

                    return new ExtendedFileInfo(file, hashstring);
                }
            }
        }
    }
}
