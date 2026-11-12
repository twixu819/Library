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
    public class DebtorRecord
    {
        public int Id { get; set; }
        public string BookTitle { get; set; }
        public string ReaderName { get; set; }
        public string ReaderDetails { get; set; }
        public string DueDate { get; set; }
        public int OverdueDays { get; set; }
        public string OverdueDaysString => $"{OverdueDays} дн.";
    }
    /// <summary>
    /// Логика взаимодействия для DebtorsWindow.xaml
    /// </summary>
    public partial class DebtorsWindow : Window
    {
        private List<DebtorRecord> _debtors = new List<DebtorRecord>();

        public DebtorsWindow()
        {
            InitializeComponent();
            LoadOverdueDebtors();
        }

        private void LoadOverdueDebtors()
        {
            _debtors.Clear();
            string todayStr = DateTime.Now.ToString("yyyy-MM-dd");

            using (var connection = new SQLiteConnection(Database.ConnectionString))
            {
                connection.Open();
                string query = @"SELECT Id, BookTitle, StudentName, GroupName, DueDate 
                                 FROM Debtors 
                                 WHERE DueDate < @today 
                                 ORDER BY DueDate ASC";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@today", todayStr);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string dueDateVal = reader.GetString(4);
                            int overdueDays = 0;

                            if (DateTime.TryParse(dueDateVal, out DateTime parsedDate))
                            {
                                overdueDays = (DateTime.Now.Date - parsedDate.Date).Days;
                            }

                            _debtors.Add(new DebtorRecord
                            {
                                Id = reader.GetInt32(0),
                                BookTitle = reader.GetString(1),
                                ReaderName = reader.GetString(2),
                                ReaderDetails = reader.GetString(3),
                                DueDate = dueDateVal,
                                OverdueDays = overdueDays
                            });
                        }
                    }
                }
            }

            DebtorsDataGrid.ItemsSource = null;
            DebtorsDataGrid.ItemsSource = _debtors;
        }

        private void ResolveDebtBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DebtorsDataGrid.SelectedItem is DebtorRecord selected)
            {
                var result = MessageBox.Show(
                    $"Повернути книгу «{selected.BookTitle}» від {selected.ReaderName}?",
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

                   
                    LoadOverdueDebtors();
                }
            }
            else
            {
                MessageBox.Show("Оберіть боржника у списку.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
