using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AlcoBear
{
    /// <summary>
    /// Логика взаимодействия для ContragentsWindow.xaml
    /// </summary>
    public partial class ContragentsWindow : Window
    {
        public ContragentsWindow()
        {
            InitializeComponent();
        }

        private void btContragentsListAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tbQueryOrgRegID.Text)) Utils.Requests.Contragent(tbQueryOrgRegID.Text.Trim());
             else if (!String.IsNullOrWhiteSpace(tbQueryOrgINN.Text)) Utils.Requests.Contragent(tbQueryOrgINN.Text.Trim(), true);
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.DownloadDocuments(parseWayBills: false, parseRests: false, parseParthers: true);
            dgContragents.ItemsSource = new List<Contractor>(DataBaseEntry.GetContractors());
            if (dgContragents.Items.Count == 0) dgContragents.IsEnabled = false;
        }

        private void btContragentsListRefresh_Click(object sender, RoutedEventArgs e)
        {
            dgContragents.ItemsSource = null;
            dgContragents.Items.Clear();
            Utils.DownloadDocuments(parseWayBills: false, parseRests: false, parseParthers: true);
            dgContragents.ItemsSource = new List<Contractor>(DataBaseEntry.GetContractors());
            if (dgContragents.Items.Count == 0) dgContragents.IsEnabled = false;
        }       
    }
}
