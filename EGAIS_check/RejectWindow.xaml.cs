using System;
using System.Windows;

namespace AlcoBear
{
    /// <summary>
    /// Логика взаимодействия для RejectWindow.xaml
    /// </summary>
    public partial class RejectWindow : Window
    {
        public RejectWindow(DateTime TTNDate, bool isReject = true)
        {
            InitializeComponent();
            this.dpActDate.SelectedDate = TTNDate;
            this.dpActDate.DisplayDateStart = TTNDate;
            if (isReject)
            {
                this.Title = "Формирование акта отказа от накладной";
                lbMessage.Visibility = System.Windows.Visibility.Visible;
                lbMessage.Content = "Причина отказа от накладной:";
                tbActNote.Visibility = System.Windows.Visibility.Visible;
                btSendAct.Content = "Отклонить";
            }
            else
            {
                this.Title = "Формирование акта приема накладной";
                lbMessage.Visibility = System.Windows.Visibility.Collapsed;
                tbActNote.Visibility = System.Windows.Visibility.Collapsed;
                btSendAct.Content = "Принять";
            }
        }

        private void btConfirmReject_Click(object sender, RoutedEventArgs e)
        {
            Utils.reject_note = this.tbActNote.Text;
            Utils.reject_date = this.dpActDate.SelectedDate.Value;
            this.Close();
        }
    }
}
