using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;

namespace FileDedupe.Database
{
    internal class Database : IDisposable
    {
        private readonly string _filename;
        private readonly bool _usingTempFile;
        private SQLiteConnection _connection;
        public Database()
        {
            _filename = Path.GetTempFileName();
            _usingTempFile = true;

            Init();
        }

        public Database(string filename)
        {
            _filename = filename;
            _usingTempFile = false;

            if (!File.Exists(_filename))
            {
                Init();
            }
            else
            {
                Connect();
            }
        }

        private void Init()
        {
            SQLiteConnection.CreateFile(_filename);

            Connect();

            ExecuteSql(@"CREATE TABLE FileDetails(Hash TEXT, Filename TEXT, Directory TEXT);");

            ExecuteSql(@"CREATE INDEX IX_FileDetails_Hash ON FileDetails(Hash);");
            ExecuteSql(@"CREATE INDEX IX_FileDetails_Filename ON FileDetails(Filename);");
            ExecuteSql(@"CREATE INDEX IX_FileDetails_Directory ON FileDetails(Directory);");
        }

        private void Connect()
        {
            _connection = new SQLiteConnection($"Data Source={_filename};Version=3;");
            _connection.Open();
        }

        private void ExecuteSql(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            command.ExecuteNonQuery();
        }

        private string Escape(string input)
        {
            return input.Replace("'", "''");
        }

        public void SaveFileInfo(ExtendedFileInfo fileInfo)
        {
            var filename = Escape(fileInfo.FullName);
            var directory = Escape(fileInfo.DirectoryName);
            ExecuteSql($"INSERT INTO FileDetails(Hash, Filename, Directory) VALUES ('{fileInfo.Hash}', '{filename}', '{directory}')");
        }

        public bool IsFileHashSaved(FileInfo fileInfo)
        {
            var escapedFilename = Escape(fileInfo.FullName);
            var sql = $"SELECT Hash FROM FileDetails WHERE Filename = '{escapedFilename}'";
            using (var command = new SQLiteCommand(sql, _connection))
            {
                var reader = command.ExecuteReader();

                return reader.Read();
            }
        }

        public IEnumerable<ExtendedFileInfo> ReadAllFileInfo()
        {
            var sql = "SELECT Hash, Filename FROM FileDetails ORDER BY Hash";
            using (var command = new SQLiteCommand(sql, _connection))
            {
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    yield return new ExtendedFileInfo(new FileInfo(reader["Filename"].ToString()), reader["Hash"].ToString());
                }
            }
        }

        public IEnumerable<DuplicateDirectoryInfo> GetDuplicateDirectoryInfo()
        {
            var duplicates = GetDirectoriesWithDuplicateFiles().ToList();
            Enhance(duplicates);
            return duplicates;
        }

        private IEnumerable<DuplicateDirectoryInfo> GetDirectoriesWithDuplicateFiles()
        {
            var sql = @"SELECT DISTINCT fd1.Directory AS Directory1, fd2.Directory AS Directory2
                        FROM FileDetails fd1
                        INNER JOIN FileDetails fd2
                            ON fd2.Hash = fd1.Hash
                            AND fd2.Directory > fd1.Directory
                        ORDER BY fd1.Directory";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    yield return new DuplicateDirectoryInfo(reader["Directory1"].ToString(), reader["Directory2"].ToString());
                }
            }
        }

        public void Enhance(IEnumerable<DuplicateDirectoryInfo> dirs)
        {
            foreach (var dir in dirs)
            {
                GetFileInOneDirectoryButNotTheOther(dir.Directory1, dir.Directory2, dir.FilesInDirectory1ButNot2);
                GetFileInOneDirectoryButNotTheOther(dir.Directory2, dir.Directory1, dir.FilesInDirectory2ButNot1);
            }
        }

        private void GetFileInOneDirectoryButNotTheOther(string Directory1, string Directory2, List<ExtendedFileInfo> list)
        {
            var sql = $@"SELECT fd1.Filename, fd2.Hash
                                FROM FileDetails fd1
                                LEFT OUTER JOIN FileDetails fd2
                                    ON fd2.Hash = fd1.Hash
                                    AND fd2.Directory = '{Directory2}'
                                WHERE fd1.Directory = '{Directory1}'
                                AND fd2.Hash IS NULL";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var filename = reader["Filename"].ToString();
                    var hash = reader["Hash"].ToString();
                    list.Add(new ExtendedFileInfo(new FileInfo(filename), hash));
                }
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();

            if (_usingTempFile)
            {
                File.Delete(_filename);
            }
        }
    }
}
