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
        public Database()
        {
            _filename = Path.GetTempFileName();
            SQLiteConnection.CreateFile(_filename);

            ExecuteSql(@"CREATE TABLE FileDetails(Hash TEXT, Filename TEXT, Directory TEXT);");
        }

        private SQLiteConnection Connect()
        {
            var connection = new SQLiteConnection($"Data Source={_filename};Version=3;");
            connection.Open();

            return connection;
        }

        private void ExecuteSql(string sql)
        {
            using (var connection = Connect())
            {
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
            }
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

        public IEnumerable<ExtendedFileInfo> ReadAllFileInfo()
        {
            using (var connection = Connect())
            {
                var sql = "SELECT Hash, Filename FROM FileDetails ORDER BY Hash";
                var command = new SQLiteCommand(sql, connection);
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
            using (var connection = Connect())
            {
                var sql = @"SELECT DISTINCT fd1.Directory AS Directory1, fd2.Directory AS Directory2
                            FROM FileDetails fd1
                            INNER JOIN FileDetails fd2
                                ON fd2.Hash = fd1.Hash
                                AND fd2.Directory > fd1.Directory
                            ORDER BY fd1.Directory";

                var command = new SQLiteCommand(sql, connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    yield return new DuplicateDirectoryInfo(reader["Directory1"].ToString(), reader["Directory2"].ToString());
                }
            }
        }

        public void Enhance(IEnumerable<DuplicateDirectoryInfo> dirs)
        {
            using (var connection = Connect())
            {
                foreach (var dir in dirs)
                {
                    GetFileInOneDirectoryButNotTheOther(connection, dir.Directory1, dir.Directory2, dir.FilesInDirectory1ButNot2);
                    GetFileInOneDirectoryButNotTheOther(connection, dir.Directory2, dir.Directory1, dir.FilesInDirectory2ButNot1);
                }
            }
        }

        private static void GetFileInOneDirectoryButNotTheOther(SQLiteConnection connection, string Directory1, string Directory2, List<ExtendedFileInfo> list)
        {
            var sql = $@"SELECT fd1.Filename, fd2.Hash
                                FROM FileDetails fd1
                                LEFT OUTER JOIN FileDetails fd2
                                    ON fd2.Hash = fd1.Hash
                                    AND fd2.Directory = '{Directory2}'
                                WHERE fd1.Directory = '{Directory1}'
                                AND fd2.Hash IS NULL";

            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var filename = reader["Filename"].ToString();
                var hash = reader["Hash"].ToString();
                list.Add(new ExtendedFileInfo(new FileInfo(filename), hash));
            }
        }

        public void Dispose()
        {
            File.Delete(_filename);
        }
    }
}
