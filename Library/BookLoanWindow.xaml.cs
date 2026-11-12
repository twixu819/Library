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
    /// Логика взаимодействия для BookLoanWindow.xaml
    /// </summary>
    public partial class BookLoanWindow : Window
    {
        private int _bookId;
        private string _bookTitle;
        private int _availableCopies;
        private bool _isStudent = true;

        public BookLoanWindow(int bookId, string bookTitle, int availableCopies)
        {
            InitializeComponent();
            _bookId = bookId;
            _bookTitle = bookTitle;
            _availableCopies = availableCopies;

            BookTitleLabel.Text = $"Книга: «{_bookTitle}» ({_availableCopies} прим.)";

            DueDatePicker.SelectedDate = DateTime.Now.AddDays(14);

            UpdateToggleButtons();
        }

        private void UpdateToggleButtons()
        {
            var activeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#166534"));
            var activeFg = Brushes.White;
            var inactiveBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            var inactiveFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));

            if (_isStudent)
            {
                StudentToggleBtn.Background = activeBg;
                StudentToggleBtn.Foreground = activeFg;
                StudentToggleBtn.BorderThickness = new Thickness(0);

                TeacherToggleBtn.Background = inactiveBg;
                TeacherToggleBtn.Foreground = inactiveFg;
                TeacherToggleBtn.BorderThickness = new Thickness(0);

                GroupOrSpecialtyLabel.Text = "Номер групи:";
            }
            else
            {
                TeacherToggleBtn.Background = activeBg;
                TeacherToggleBtn.Foreground = activeFg;
                TeacherToggleBtn.BorderThickness = new Thickness(0);

                StudentToggleBtn.Background = inactiveBg;
                StudentToggleBtn.Foreground = inactiveFg;
                StudentToggleBtn.BorderThickness = new Thickness(0);

                GroupOrSpecialtyLabel.Text = "Номер спеціальності / Кафедра:";
            }
        }

        private void StudentToggle_Click(object sender, RoutedEventArgs e)
        {
            _isStudent = true;
            UpdateToggleButtons();
        }

        private void TeacherToggle_Click(object sender, RoutedEventArgs e)
        {
            _isStudent = false;
            UpdateToggleButtons();
        }

        private void IssueConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_availableCopies <= 0)
            {
                MessageBox.Show("Немає доступних примірників для видачі!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string lastName = LastNameBox.Text.Trim();
            string firstName = FirstNameBox.Text.Trim();
            string middleName = MiddleNameBox.Text.Trim();
            string extraInfo = GroupOrSpecialtyBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
            {
                MessageBox.Show("Будь ласка, вкажіть прізвище та ім'я читача.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DueDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Будь ласка, оберіть дату повернення книги.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime selectedDueDate = DueDatePicker.SelectedDate.Value;

            if (selectedDueDate.Date == DateTime.Now.Date)
            {
                MessageBox.Show("Дата повернення не може бути призначена на сьогодні!\nОберіть минулу дату (для фіксації боргу) або дату в майбутньому.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string fullName = $"{lastName} {firstName} {middleName}".Trim();
            string roleType = _isStudent ? $"Студент ({extraInfo})" : $"Викладач (Спец: {extraInfo})";
            string dueDateFormatted = selectedDueDate.ToString("yyyy-MM-dd");

            using (var connection = new SQLiteConnection(Database.ConnectionString))
            {
                connection.Open();

                string updateBookSql = "UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE Id = @bookId";
                using (var updateCmd = new SQLiteCommand(updateBookSql, connection))
                {
                    updateCmd.Parameters.AddWithValue("@bookId", _bookId);
                    updateCmd.ExecuteNonQuery();
                }

                string insertDebtSql = @"INSERT INTO Debtors (StudentName, GroupName, BookTitle, DueDate) 
                                         VALUES (@name, @group, @book, @dueDate)";
                using (var insertCmd = new SQLiteCommand(insertDebtSql, connection))
                {
                    insertCmd.Parameters.AddWithValue("@name", fullName);
                    insertCmd.Parameters.AddWithValue("@group", $"{roleType}, тел: {phone}");
                    insertCmd.Parameters.AddWithValue("@book", _bookTitle);
                    insertCmd.Parameters.AddWithValue("@dueDate", dueDateFormatted);
                    insertCmd.ExecuteNonQuery();
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