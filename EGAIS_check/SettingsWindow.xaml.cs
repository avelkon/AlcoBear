using System;
using System.Windows;

namespace AlcoBear
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly System.Windows.Media.Brush color_TextBoxBadValue = System.Windows.Media.Brushes.LightCoral;
        private readonly System.Windows.Media.Brush color_TextBoxGoodValue = System.Windows.Media.Brushes.LightGreen;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void loadSettings()
        {
            this.tbFSRAR_ID.Text = Properties.Settings.Default.FSRAR_ID;
            if (String.IsNullOrWhiteSpace(tbFSRAR_ID.Text)) tbFSRAR_ID.Background = color_TextBoxBadValue;
            this.tbUTMUrl.Text = Properties.Settings.Default.UTM_host;
            if (DataBaseEntry.ThisCompany != null)
            {
                this.tbOrgShortName.Text = DataBaseEntry.ThisCompany.ShortName;
                this.tbOrgFullName.Text = DataBaseEntry.ThisCompany.FullName;
                this.tbOrgAddress.Text = DataBaseEntry.ThisCompany.Address.Description;
                this.tbOrgINN.Text = DataBaseEntry.ThisCompany.INN;
                this.tbOrgKPP.Text = DataBaseEntry.ThisCompany.KPP;
            }
            else
            {
                this.tbOrgShortName.Clear();
                this.tbOrgFullName.Clear();
                this.tbOrgAddress.Clear();
                this.tbOrgINN.Clear();
                this.tbOrgKPP.Clear();
            }
        }

        private void settingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loadSettings();
        }

        private void btToDefault_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вернуть настройки по умолчанию?", "Сброс настроек", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No).Equals(MessageBoxResult.Yes))
            {
                this.tbOrgShortName.Clear();
                this.tbOrgFullName.Clear();
                this.tbOrgAddress.Clear();
                this.tbOrgINN.Clear();
                this.tbOrgKPP.Clear();
                DataBaseEntry.ThisCompany = null;
                Properties.Settings.Default.Reset();
                if (MessageBox.Show("Очистить справочник контрагентов?", "Сброс настроек", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No).Equals(MessageBoxResult.Yes))
                {
                    DataBaseEntry.DeleteContragents();
                }
            }
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            UriBuilder utm_uri = new UriBuilder(tbUTMUrl.Text);
            Properties.Settings.Default.UTM_host = utm_uri.Host;
            Properties.Settings.Default.UTM_port = utm_uri.Port == 80 ? 8080 : utm_uri.Port;
            if (!Properties.Settings.Default.UTM_hosts_list.Contains(utm_uri.Host)) Properties.Settings.Default.UTM_hosts_list.Add(utm_uri.Host);
            Properties.Settings.Default.Save();
            if (!Utils.GetFSRAR())
            {
                MessageBox.Show("Не удалось подключиться к УТМ", "Подключение к УТМ", MessageBoxButton.OK, MessageBoxImage.Information);
                tbFSRAR_ID.Clear();
                tbFSRAR_ID.Background = color_TextBoxBadValue;
            }
            else
            {
                tbFSRAR_ID.Text = Properties.Settings.Default.FSRAR_ID;
                tbFSRAR_ID.Background = color_TextBoxGoodValue;
            }
        }

        private void btGetOrgInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(this.tbFSRAR_ID.Text)) Utils.Requests.Contragent(this.tbFSRAR_ID.Text);
            else if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.FSRAR_ID)) Utils.Requests.Contragent(Properties.Settings.Default.FSRAR_ID);
        }

        private void settingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}
