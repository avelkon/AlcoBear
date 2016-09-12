using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace AlcoBear
{
    /// <summary>
    /// Логика взаимодействия для AddCommentWindow.xaml
    /// </summary>
    public partial class AddCommentWindow : Window
    {
        public string Comment = "";

        public AddCommentWindow(StringCollection commentPatterns)
        {
            InitializeComponent();
            foreach (string item in commentPatterns) cbCommentPatterns.Items.Add(item);
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            this.Comment = tbComment.Text.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void cbCommentPatterns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbComment.Text = (sender as ComboBox).SelectedItem.ToString();
        }

        private void tbComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
