using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AlcoBear
{
    /// <summary>
    /// Логика взаимодействия для CreateNewDocumentWindow.xaml
    /// </summary>
    public partial class CreateNewDocumentWindow : Window
    {

        private ObservableCollection<StockPosition> posToReturn = new ObservableCollection<StockPosition>();

        public CreateNewDocumentWindow()
        {
            InitializeComponent();
            lbInfoLabel.Visibility = System.Windows.Visibility.Hidden;
            cbInfoComboBox.Visibility = System.Windows.Visibility.Hidden;
            cbInfoComboBox.Items.Clear();
            dgReturn_columnPrice.Visibility = System.Windows.Visibility.Hidden;
            cbRestAlligment.IsChecked = false;
            cbRestAlligment.Visibility = System.Windows.Visibility.Hidden;
            Utils.DownloadDocuments(parseWayBills: false, parseRests: true, parseParthers: true);
        }

        private void dgRestsPos_Loaded(object sender, RoutedEventArgs e)
        {
            dgRestsPos.ItemsSource = Utils.restsList;
            if (dgRestsPos.Items.Count > 0)
            {
                dgRestsPos.IsEnabled = true;
                gbRests.Header = String.Format("Остатки ({0})", Properties.Settings.Default.LastRestsDateTime.ToString("dd.MM.yyyy"));
                dgReturnPos.IsEnabled = true;
            }
        }

        private void btSendRestsQuery_Click(object sender, RoutedEventArgs e)
        {
            tbStatusMessage.Text = "Отправка запроса в УТМ...";
            tbStatusMessage.Text = Utils.Requests.Rests() ? "Запрос отправлен в УТМ" : "Ошибка при формировании запроса";
            Utils.RestsAutoRefresh.Start();
        }

        private void dgReturnPos_Loaded(object sender, RoutedEventArgs e)
        {
            dgReturnPos.ItemsSource = this.posToReturn;
        }

        private void swapDataGridRows(StockPosition position, ObservableCollection<StockPosition> DataGridToDel, ObservableCollection<StockPosition> DataGridToAdd)
        {
            DataGridToDel.Remove(position);
            if (tbtAddAll.IsChecked != true) position.QuantityToReturn = 0;
            else position.QuantityToReturn = position.Quantity;
            DataGridToAdd.Add(position);
        }

        private void dgPositions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((DataGrid)sender).SelectedIndex == -1) return;
            ObservableCollection<StockPosition> source = null, dest = null;
            if(((DataGrid)sender).Equals(this.dgRestsPos)) { source = Utils.restsList; dest = this.posToReturn; }
            else if(((DataGrid)sender).Equals(this.dgReturnPos)) 
            {
                if (MessageBox.Show("Удалить позицию из списка?", "Правка списка позиций", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                source = this.posToReturn; 
                dest = Utils.restsList;
            }
            if(source == null || dest == null) return;
            StockPosition positionToAdd = ((DataGrid)sender).SelectedItem as StockPosition;
            this.swapDataGridRows(positionToAdd, source, dest);
        }

        private void btRefreshRests_Click(object sender, RoutedEventArgs e)
        {
            this.posToReturn.Clear();
            this.tbSearch.Clear();
            Utils.DownloadDocuments(parseWayBills: false, parseRests: true, parseParthers: true);
            tbStatusMessage.Text = "Таблица остатков обновлена";
            dgRestsPos.IsEnabled = dgRestsPos.Items.Count > 0;
        }

        private void btSendDocument_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                //Нажата кнопка формирования документа
                if (cbDocumentType.SelectedIndex == -1) throw new Exception("Не выбран тип документа");
                Utils.DocumentTypes docType = (Utils.DocumentTypes)((ComboBoxItem)cbDocumentType.SelectedItem).Tag;
                //Создание нового акта списания товара
                if (docType == Utils.DocumentTypes.ActWriteOff)
                {
                    if (cbInfoComboBox.SelectedIndex == -1) throw new NullReferenceException("Не указанна причина списания товара");
                    if (dgReturnPos.Items.Count < 1 || !dgReturnPos.IsEnabled) throw new NullReferenceException("Не указанны позиции для списания");
                    //Если отмечено выравнивание остатков
                    if (cbRestAlligment.IsChecked.HasValue && cbRestAlligment.IsChecked.Value)
                    {
                        //Добавляем не выбранные позиции
                        List<StockPosition> posToWriteOff = new List<StockPosition>(Utils.restsList);
                        foreach (StockPosition p in posToWriteOff) p.QuantityToReturn = p.Quantity;
                        //Добавляем оставшееся количество выбранных бутылок
                        List<StockPosition> posToSave = new List<StockPosition>(posToReturn);
                        foreach(StockPosition p in posToSave) 
                        {
                            if (p.QuantityToReturn < p.Quantity)
                            {
                                p.QuantityToReturn = p.Quantity - p.QuantityToReturn;
                                posToWriteOff.Add(p);
                            }
                        }
                        //----------------
                        if (Utils.SendActWriteOff(posToWriteOff, cbInfoComboBox.SelectedItem.ToString()))
                        {
                            tbStatusMessage.Text = "Акт списания успешно отправлен в УТМ";
                        }
                        else
                        {
                            throw new Exception("Ошибка при формировании акта списания");
                        }
                    }
                    else if (cbRestAlligment.IsChecked.HasValue && !cbRestAlligment.IsChecked.Value)
                    {
                        if (Utils.SendActWriteOff(dgReturnPos.ItemsSource as IEnumerable<StockPosition>, cbInfoComboBox.SelectedItem.ToString()))
                        {
                            tbStatusMessage.Text = "Акт списания успешно отправлен в УТМ";
                        }
                        else
                        {
                            throw new Exception("Ошибка при формировании акта списания");
                        }
                    }
                }
                //Создание новой ТТН
                else if (docType == Utils.DocumentTypes.WBInvoiceFromMe || docType == Utils.DocumentTypes.WBReturnFromMe)
                {
                    if (cbInfoComboBox.SelectedIndex == -1) throw new NullReferenceException("Не выбран получатель");
                    if (Utils.SendWayBill(cbInfoComboBox.SelectedItem as Contractor, dgReturnPos.ItemsSource as IEnumerable<StockPosition>, docType))
                    {
                        tbStatusMessage.Text = "Возвратная накладная отправлена в УТМ";
                    }
                    else
                    {
                        throw new Exception("Ошибка при формировании накладной");
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                tbStatusMessage.Text = ex.Message;
            }
            catch (Exception ex)
            {
                tbStatusMessage.Text = ex.Message;
                Utils.WriteLog(ex.Message, Utils.MessageType.ERROR);
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (StockPosition pos in Utils.restsList)
            {
                if (!String.IsNullOrWhiteSpace(tbSearch.Text) && pos.FullName.ToLower().Contains(tbSearch.Text.ToLower()))
                {
                    dgRestsPos.ScrollIntoView(pos);
                    dgRestsPos.SelectedItem = pos;
                    break;
                }
            }
        }

        private void cbDocumentType_Loaded(object sender, RoutedEventArgs e)
        {
            cbItem_Return.Tag = Utils.DocumentTypes.WBReturnFromMe;
            cbItem_TTN.Tag = Utils.DocumentTypes.WBInvoiceFromMe;
            cbItem_ActWriteOff.Tag = Utils.DocumentTypes.ActWriteOff;
        }

        private void LoadOrgList(ComboBox comboBox, Label label)
        {
            label.Visibility = System.Windows.Visibility.Visible;
            label.Content = "Получатель";
            dgReturn_columnPrice.Visibility = System.Windows.Visibility.Visible;
            cbInfoComboBox.Visibility = System.Windows.Visibility.Visible;
            try { comboBox.ItemsSource = DataBaseEntry.GetContractors(); }
            catch { comboBox.ItemsSource = null; }
            comboBox.IsEnabled = comboBox.Items.Count > 0;
        }

        private void LoadWriteOffReasons(ComboBox comboBox, Label label)
        {
            label.Content = "Причина списания";
            label.Visibility = System.Windows.Visibility.Visible;
            dgReturn_columnPrice.Visibility = System.Windows.Visibility.Hidden;
            cbInfoComboBox.Visibility = System.Windows.Visibility.Visible;
            try { comboBox.ItemsSource = Properties.Settings.Default.ActWriteOffReasons; }
            catch { comboBox.ItemsSource = null; }
            comboBox.IsEnabled = comboBox.Items.Count > 0;
        }

        private void cbDocumentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex == -1)
            {
                //Если ничего не выбрано
                lbInfoLabel.Visibility = System.Windows.Visibility.Hidden;
                cbInfoComboBox.Visibility = System.Windows.Visibility.Hidden;
                dgReturn_columnPrice.Visibility = System.Windows.Visibility.Hidden;
                cbRestAlligment.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                Utils.DocumentTypes selectedType = (Utils.DocumentTypes)(((sender as ComboBox).SelectedItem as ComboBoxItem).Tag);
                if (selectedType == Utils.DocumentTypes.WBInvoiceFromMe || selectedType == Utils.DocumentTypes.WBReturnFromMe)
                {
                    //Если выбраны ТТН
                    LoadOrgList(cbInfoComboBox, lbInfoLabel);
                    cbRestAlligment.Visibility = System.Windows.Visibility.Hidden;
                }
                else if (selectedType == Utils.DocumentTypes.ActWriteOff || selectedType == Utils.DocumentTypes.ActWriteOffShop)
                {
                    //Если выбран акт списания или акт постановки на учет
                    LoadWriteOffReasons(cbInfoComboBox, lbInfoLabel);
                    cbRestAlligment.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

    }
}
