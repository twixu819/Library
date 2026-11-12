using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Library
{
    public class IssuedRecord
    {
        public int Id { get; set; }
        public string BookTitle { get; set; }
        public string ReaderName { get; set; }
        public string ReaderDetails { get; set; }
        public string DueDate { get; set; }
    }
    /// <summary>
    /// Логика взаимодействия для IssuedBooksWindow.xaml
    /// </summary>
    public partial class IssuedBooksWindow : Window
    {
        private List<IssuedRecord> _records = new List<IssuedRecord>();

        public IssuedBooksWindow()
        {
            InitializeComponent();
            LoadIssuedBooks();
        }

        private void LoadIssuedBooks()
        {
            _records.Clear();
            string todayStr = DateTime.Now.ToString("yyyy-MM-dd");

            using (var connection = new SQLiteConnection(Database.ConnectionString))
            {
                connection.Open();
                string query = @"SELECT Id, BookTitle, StudentName, GroupName, DueDate 
                         FROM Debtors 
                         WHERE DueDate >= @today 
                         ORDER BY DueDate ASC";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@today", todayStr);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _records.Add(new IssuedRecord
                            {
                                Id = reader.GetInt32(0),
                                BookTitle = reader.GetString(1),
                                ReaderName = reader.GetString(2),
                                ReaderDetails = reader.GetString(3),
                                DueDate = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            IssuedDataGrid.ItemsSource = null;
            IssuedDataGrid.ItemsSource = _records;
        }

        private void ReturnBookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IssuedDataGrid.SelectedItem is IssuedRecord selected)
            {
                var result = MessageBox.Show(
                    $"Підтвердити повернення книги «{selected.BookTitle}» від читача {selected.ReaderName}?",
                    "Повернення книги",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new SQLiteConnection(Database.ConnectionString))
                    {
                        connection.Open();

                        string deleteQuery = "DELETE FROM Debtors WHERE Id = @id";
                        using (var delCmd = new SQLiteCommand(deleteQuery, connection))
                        {
                            delCmd.Parameters.AddWithValue("@id", selected.Id);
                            delCmd.ExecuteNonQuery();
                        }

                        string updateBookQuery = "UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE Title = @title";
                        using (var upCmd = new SQLiteCommand(updateBookQuery, connection))
                        {
                            upCmd.Parameters.AddWithValue("@title", selected.BookTitle);
                            upCmd.ExecuteNonQuery();
                        }
                    }

                   
                    LoadIssuedBooks(); 
                }
            }
            else
            {
                MessageBox.Show("Оберіть запис у таблиці.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
