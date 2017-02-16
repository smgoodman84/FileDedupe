using System;
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

        public void SaveFileInfo(ExtendedFileInfo fileInfo)
        {
            ExecuteSql($"INSERT INTO FileDetails(Hash, Filename, Directory) VALUES ('{fileInfo.Hash}', '{fileInfo.FullName}', '{fileInfo.DirectoryName}')");
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

        public IEnumerable<DuplicateDirectoryInfo> GetDirectoriesWithDuplicateFiles()
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

        public void Dispose()
        {
            File.Delete(_filename);
        }
    }
}
