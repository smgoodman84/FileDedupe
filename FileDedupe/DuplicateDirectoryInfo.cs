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
        }
    }
}
