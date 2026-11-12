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
    /// Логика взаимодействия для BookDetailsWindow.xaml
    /// </summary>
    public partial class BookDetailsWindow : Window
    {
        private int _bookId;

        public BookDetailsWindow(BookItem book)
        {
            InitializeComponent();
            _bookId = book.Id;

            BookIdText.Text = $"Інвентарний №: {book.Id}";
            TitleBox.Text = book.Title;
            AuthorBox.Text = book.Author;
            DisciplineBox.Text = book.Discipline;
            CopiesBox.Text = book.AvailableCopies.ToString();
        }

        private void IssueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CopiesBox.Text.Trim(), out int copies) || copies <= 0)
            {
                MessageBox.Show("Усі примірники цієї книги вже відсутні", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loanWindow = new BookLoanWindow(_bookId, TitleBox.Text.Trim(), copies);
            loanWindow.Owner = this;

            if (loanWindow.ShowDialog() == true)
            {
                DialogResult = true; 
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text.Trim();
            string author = AuthorBox.Text.Trim();
            string discipline = DisciplineBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author))
            {
                MessageBox.Show("Назва видання та автор не можуть бути порожніми.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CopiesBox.Text.Trim(), out int copies) || copies < 0)
            {
                MessageBox.Show("Введіть коректну кількість примірників (число від 0).", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var connection = new SQLiteConnection(Database.ConnectionString))
            {
                connection.Open();
                string query = @"UPDATE Books 
                                 SET Title = @title, Author = @author, Discipline = @discipline, AvailableCopies = @copies 
                                 WHERE Id = @id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@author", author);
                    cmd.Parameters.AddWithValue("@discipline", discipline);
                    cmd.Parameters.AddWithValue("@copies", copies);
                    cmd.Parameters.AddWithValue("@id", _bookId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Зміни збережено", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Видалити книгу без змоги відновлення?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                using (var connection = new SQLiteConnection(Database.ConnectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Books WHERE Id = @id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", _bookId);
                        cmd.ExecuteNonQuery();
                    }
                }

                DialogResult = true;
            }
        }
    }
}
