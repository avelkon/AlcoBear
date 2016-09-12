using System;
using System.Windows;

namespace AlcoBear
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void loadSettings()
        {
            this.tbFSRAR_ID.Text = Properties.Settings.Default.FSRAR_ID;
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
                Properties.Settings.Default.UTM_host = "localhost";
                Properties.Settings.Default.UTM_port = 8080;
                Properties.Settings.Default.FSRAR_ID = "";
                Properties.Settings.Default.Save();
                Utils.BuildURL();
                if (MessageBox.Show("Очистить справочник контрагентов?", "Сброс настроек", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No).Equals(MessageBoxResult.Yes))
                {
                    DataBaseEntry.DeleteContragents();
                }
            }
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FSRAR_ID = tbFSRAR_ID.Text;
            UriBuilder utm_uri = new UriBuilder(tbUTMUrl.Text);
            Properties.Settings.Default.UTM_host = utm_uri.Host;
            Properties.Settings.Default.UTM_port = utm_uri.Port == 80 ? 8080 : utm_uri.Port;
            Properties.Settings.Default.Save();
            Utils.BuildURL();
            this.Close();
        }

        private void btGetOrgInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(this.tbFSRAR_ID.Text)) Utils.Requests.Contragent(this.tbFSRAR_ID.Text);
            else if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.FSRAR_ID)) Utils.Requests.Contragent(Properties.Settings.Default.FSRAR_ID);
        }
    }
}
