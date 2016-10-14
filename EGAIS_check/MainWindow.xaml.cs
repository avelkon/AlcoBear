using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace AlcoBear
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "AlcoBear v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            //Проверка новой версии
            try
            {
                int updateLevel = Utils.CheckUpdate();
                if (updateLevel > 0)
                {
                    if (MessageBox.Show("Необходимо обновить программу\nСделать это сейчас?", "Обновление", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) 
                    {
                        //Обновляем
                        Utils.StartUpdate(updateLevel);
                        this.Close();
                    }
                    else
                    {
                        //Отказались от обновления
                        MessageBox.Show("Необходимо обновить программу до последней версии", "Обновление", MessageBoxButton.OK, MessageBoxImage.Warning);
                        this.Close();
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                //Ошибка при подключении к серверу обновлений
                MessageBox.Show(ex.Message, "Обновление", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            //-----------
        }

        private void btDownloadInvoices_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            dgInvoices.Items.Clear();
            try
            {
                Utils.DownloadDocuments(parseRests: false, parseParthers: false);
                if (Utils.incomeWayBillsList.Count == 0)
                {
                    //Нету ТТН
                    dgInvoices.IsEnabled = false;
                }
                else
                {
                    dgInvoices.IsEnabled = true;
                    this.dgInvoices.ItemsSource = Utils.incomeWayBillsList;
                }
            }
            catch (Exception ex)
            {
                Utils.WriteLog(ex.Message, Utils.MessageType.ERROR);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btCreateWayBillAct_Click(object sender, RoutedEventArgs e)
        {
            //Одобрить ТТН
            DataGridRow row = (DataGridRow)((FrameworkElement)sender).Tag;
            Invoice waybill = (Invoice)row.Item;
            try
            {
                //Определяем какая из кнопок нажата
                bool? isReject = null;
                if ((sender as Button).Name.Equals("btAccept")) isReject = false;
                else if ((sender as Button).Name.Equals("btReject")) isReject = true;
                if (!isReject.HasValue) throw(new ArgumentNullException());
                //Открываем окно для ввода информации об акте
                RejectWindow rjwindow = new RejectWindow(Convert.ToDateTime(waybill.CreateDate), isReject.Value);
                rjwindow.Owner = this;
                rjwindow.ShowDialog();
                if (Utils.SendXML(String.Format(Properties.Resources.XMLPattern_WayBillAct,
                                                Properties.Settings.Default.FSRAR_ID,
                                                isReject.Value ? "Rejected" : "Accepted",
                                                waybill.Number,
                                                Utils.reject_date.ToString("yyyy-MM-dd"),
                                                waybill.WBRegId,
                                                Utils.reject_note
                                               ),
                                  Utils.URLs.outcomeWayBillAct,
                                  Utils.DocumentTypes.WayBillAct.ToString()
                                 )
                   )
                {
                    //Удаление ТТН из ЕГАИС
                    Utils.DeleteFile(waybill.WayBillUrl);
                    Utils.DeleteFile(waybill.FormBRegIdUrl);
                    //---------------------
                    Utils.incomeWayBillsList.Remove(waybill);
                    this.dgInvoices.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                Utils.WriteLog(ex.Message, Utils.MessageType.ERROR);
            }
        }

        private static readonly string groupFieldName = "ProductVCode";

        private void dgInvoices_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            try
            {
                DataGrid dgDetails = (DataGrid)e.DetailsElement.FindName("dgdShipper");
                Invoice waybill = e.Row.Item as Invoice;
                dgDetails.ItemsSource = new List<Contractor>(1) { waybill.Shipper };
                dgDetails = (DataGrid)e.DetailsElement.FindName("dgdPositions");
                ICollectionView coll = CollectionViewSource.GetDefaultView(waybill.Positions);
                //Поле, по которому групируются позиции
                coll.GroupDescriptions.Add(new PropertyGroupDescription(groupFieldName));
                dgDetails.ItemsSource = coll;
                TextBlock lbTotalSum = (TextBlock)e.DetailsElement.FindName("lbSumPrice");
                TextBlock lbTotalDal = (TextBlock)e.DetailsElement.FindName("lbSumDal");
                double sumPrice = 0;
                double dal = 0;
                foreach (WayBillPosition wbpos in waybill.Positions)
                {
                    sumPrice += wbpos.Price * wbpos.Quantity;
                    dal += (wbpos.Capacity * wbpos.Quantity);
                }
                lbTotalSum.Text = "Итого:\t" + sumPrice.ToString("F2") + " руб.";
                lbTotalDal.Text = "\t" + dal.ToString("F2") + " dal";
            }
            catch (Exception ex)
            {
                Utils.WriteLog(ex.Message, Utils.MessageType.ERROR);
            }
        }

        private void btSettings_Click(object sender, RoutedEventArgs e)
        {
            Window settings_window = new SettingsWindow();
            settings_window.Owner = this;
            settings_window.ShowDialog();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)((FrameworkElement)sender).Tag;
            row.DetailsVisibility = Visibility.Visible;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)((FrameworkElement)sender).Tag;
            row.DetailsVisibility = Visibility.Collapsed;
        }

        private void refreshTbInfoOrg()
        {
            if (DataBaseEntry.ThisCompany != null && !String.IsNullOrWhiteSpace(DataBaseEntry.ThisCompany.ShortName))
            {
                this.tbInfoOrg.Text = String.Format("{0} (ИНН {1} КПП {2})",
                    DataBaseEntry.ThisCompany.ShortName,
                    DataBaseEntry.ThisCompany.INN,
                    DataBaseEntry.ThisCompany.KPP);
            }
            else if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.FSRAR_ID))
            {
                this.tbInfoOrg.Text = String.Format("ФСРАР ID {0}", Properties.Settings.Default.FSRAR_ID);
            }
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.FSRAR_ID))
            {
                Window settings_window = new SettingsWindow();
                settings_window.Owner = this;
                settings_window.ShowDialog();
            }
        }

        private void btCreateNewDocument_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            Window return_window = new CreateNewDocumentWindow();
            return_window.Owner = this;
            this.Cursor = Cursors.Arrow;
            return_window.ShowDialog();
        }

        private void btContragentsListShow_Click(object sender, RoutedEventArgs e)
        {
            Window contragents_window = new ContragentsWindow();
            contragents_window.Owner = this;
            contragents_window.Show();
        }

    }
}