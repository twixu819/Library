using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Library
{
    public class BookItem
    {
        public int Id { get; set; }
        public int RowNumber { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Discipline { get; set; }
        public int AvailableCopies { get; set; }
        public string CopiesString => $"{AvailableCopies} прим";
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<BookItem> _allBooks = new List<BookItem>();

        public MainWindow()
        {
            InitializeComponent();
            LoadBooksFromDatabase();
        }

        public void LoadBooksFromDatabase()
        {
            _allBooks.Clear();

            using (var connection = new SQLiteConnection(Database.ConnectionString))
            {
                connection.Open();
                string query = "SELECT Id, Title, Author, Discipline, AvailableCopies FROM Books";

                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    int index = 1;
                    while (reader.Read())
                    {
                        _allBooks.Add(new BookItem
                        {
                            RowNumber = index++,
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Author = reader.GetString(2),
                            Discipline = reader.GetString(3),
                            AvailableCopies = reader.GetInt32(4)
                        });
                    }
                }
            }

            BooksDataGrid.ItemsSource = null;
            BooksDataGrid.ItemsSource = _allBooks;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                BooksDataGrid.ItemsSource = _allBooks;
            }
            else
            {
                var filtered = _allBooks.FindAll(b =>
                    b.Title.ToLower().Contains(query) ||
                    b.Author.ToLower().Contains(query) ||
                    b.Discipline.ToLower().Contains(query));

                BooksDataGrid.ItemsSource = filtered;
            }
        }

        private void ClearSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
        }

        private void AllBooks_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            LoadBooksFromDatabase();
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddBookWindow();
            addWindow.Owner = this;

            if (addWindow.ShowDialog() == true)
            {
                LoadBooksFromDatabase();
            }
        }

        private void IssuedBooks_Click(object sender, RoutedEventArgs e)
        {
            var window = new IssuedBooksWindow();
            window.Owner = this;

            window.ShowDialog();
            LoadBooksFromDatabase();
        }

        private void BooksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is BookItem selectedBook)
            {
                var detailsWindow = new BookDetailsWindow(selectedBook);
                detailsWindow.Owner = this;

                if (detailsWindow.ShowDialog() == true)
                {
                    LoadBooksFromDatabase();
                }
            }
        }

        private void Debtors_Click(object sender, RoutedEventArgs e)
        {
            var debtorsWindow = new DebtorsWindow();
            debtorsWindow.Owner = this;
            debtorsWindow.ShowDialog();

            LoadBooksFromDatabase();
        }
    }
}