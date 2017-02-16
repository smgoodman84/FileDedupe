using System.IO;

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
        public string DirectoryName => _fileInfo.DirectoryName;
    }
}
