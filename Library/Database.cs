using System;
using System.IO;
using System.Data.SQLite;

namespace Library
{
    public static class Database
    {
        private const string DbFileName = "library.db";
        public static string ConnectionString => $"Data Source={DbFileName};Version=3;";

        public static void InitializeDatabase()
        {
            bool dbExists = File.Exists(DbFileName);

            if (!dbExists)
            {
                SQLiteConnection.CreateFile(DbFileName);
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string createTablesQuery = @"
                    CREATE TABLE IF NOT EXISTS Books (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Author TEXT NOT NULL,
                        Discipline TEXT NOT NULL,
                        AvailableCopies INTEGER NOT NULL DEFAULT 1,
                        TotalCopies INTEGER NOT NULL DEFAULT 1,
                        IsIssued INTEGER NOT NULL DEFAULT 0
                    );

                    CREATE TABLE IF NOT EXISTS Debtors (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StudentName TEXT NOT NULL,
                        GroupName TEXT NOT NULL,
                        BookTitle TEXT NOT NULL,
                        DueDate TEXT NOT NULL
                    );";

                using (var command = new SQLiteCommand(createTablesQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                if (!dbExists)
                {
                    string insertSeedData = @"
                        INSERT INTO Books (Title, Author, Discipline, AvailableCopies, TotalCopies, IsIssued) VALUES
                        ('Обробка зображень та мультимедіа', 'Ю.С. Матвієнко', '121', 30, 30, 0),
                        ('English for Information Technology and Computing', 'І. Ю.Сковронська', '121', 35, 35, 0),
                        ('Комп''ютерна дискретна математика', 'І. Ю.Сковронська', '121', 20, 20, 0);";

                    using (var command = new SQLiteCommand(insertSeedData, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}