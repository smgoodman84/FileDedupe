using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDedupe
{
    class ExtendedFileInfo
    {
        private readonly FileInfo _fileInfo;
        private readonly string _hash;

        public ExtendedFileInfo(FileInfo fileInfo, string hash)
        {
            _fileInfo = fileInfo;
            _hash = hash;
        }

        public string FullName => _fileInfo.FullName;
        public string Hash => _hash;
    }
}
