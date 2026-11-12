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
    /// <summary>
    /// Логика взаимодействия для AddBookWindow.xaml
    /// </summary>
    public partial class AddBookWindow : Window
    {
        public AddBookWindow()
        {
            InitializeComponent();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text.Trim();
            string author = AuthorBox.Text.Trim();
            string discipline = DisciplineBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author))
            {
                MessageBox.Show("Будь ласка, введіть назву книги та автора.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CopiesBox.Text.Trim(), out int copies) || copies <= 0)
            {
                MessageBox.Show("Введіть коректну кількість примірників (число більше 0).", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var connection = new SQLiteConnection(Database.ConnectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Books (Title, Author, Discipline, AvailableCopies, TotalCopies, IsIssued) 
                                 VALUES (@title, @author, @discipline, @copies, @copies, 0)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@author", author);
                    cmd.Parameters.AddWithValue("@discipline", discipline);
                    cmd.Parameters.AddWithValue("@copies", copies);
                    cmd.ExecuteNonQuery();
                }
            }

            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
