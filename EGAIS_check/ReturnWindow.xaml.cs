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
    /// Логика взаимодействия для CreateNewDocument.xaml
    /// </summary>
    public partial class CreateNewDocument : Window
    {

        private ObservableCollection<StockPosition> posToReturn = new ObservableCollection<StockPosition>();

        public CreateNewDocument()
        {
            InitializeComponent();
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
            //Thread req_tr = new Thread(Utils.Requests.Rests);
            tbStatusMessage.Text = "Отправка запроса в УТМ...";
            tbStatusMessage.Text = Utils.Requests.Rests() ? "Запрос отправлен в УТМ" : "Ошибка при формировании запроса";
        }

        private void dgReturnPos_Loaded(object sender, RoutedEventArgs e)
        {
            dgReturnPos.ItemsSource = this.posToReturn;
        }

        private void swapDataGridRows(StockPosition position, ObservableCollection<StockPosition> DataGridToDel, ObservableCollection<StockPosition> DataGridToAdd)
        {
            DataGridToDel.Remove(position);
            position.QuantityToReturn = 0;
            DataGridToAdd.Add(position);
        }

        private void dgPositions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((DataGrid)sender).SelectedIndex == -1) return;
            ObservableCollection<StockPosition> source = null, dest = null;
            if(((DataGrid)sender).Equals(this.dgRestsPos)) { source = Utils.restsList; dest = this.posToReturn; }
            else if(((DataGrid)sender).Equals(this.dgReturnPos)) 
            {
                if (MessageBox.Show("Удалить позицию из возвратной накладной?", "Правка возвратной накладной", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
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
            if (dgRestsPos.Items.Count > 0)
            {
                dgRestsPos.IsEnabled = true;
                dgReturnPos.IsEnabled = true;
            }
            cbOrgList.ItemsSource = DataBaseEntry.GetContractors();
            cbOrgList.IsEnabled = cbOrgList.Items.Count > 0;
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
                    AddCommentWindow commentWindow = new AddCommentWindow(Properties.Settings.Default.Pattern_ActWriteOffReasons);
                    commentWindow.Owner = this;
                    bool? dialogResult = commentWindow.ShowDialog();
                    if (!dialogResult.HasValue || !dialogResult.Value)
                    {
                        tbStatusMessage.Text = "Не указанна причина списания товара";
                        throw new NullReferenceException("Не указанна причина списания товара");
                    }
                    if (Utils.SendActWriteOff(dgReturnPos.ItemsSource as IEnumerable<StockPosition>, commentWindow.Comment))
                    {
                        tbStatusMessage.Text = "Акт списания успешно отправлен в УТМ";
                    }
                    else
                    {
                        tbStatusMessage.Text = "Ошибка при формировании акта списания";
                        throw new Exception("Ошибка при формировании акта списания");
                    }
                }
                //Создание нового акта постановки на баланс
                else if(docType == Utils.DocumentTypes.ActChargeOn)
                {
                    return;
                }
                //Создание новой ТТН
                else if (docType == Utils.DocumentTypes.WBInvoiceFromMe || docType == Utils.DocumentTypes.WBReturnFromMe)
                {
                    if (cbOrgList.SelectedIndex == -1) throw new NullReferenceException("Не выбран получатель");
                    if (Utils.SendWayBill(cbOrgList.SelectedItem as Contractor, dgReturnPos.ItemsSource as IEnumerable<StockPosition>, docType))
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

        private void cbOrgList_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            cb.ItemsSource = DataBaseEntry.GetContractors();
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
            cbItem_ActWriteOf.Tag = Utils.DocumentTypes.ActWriteOff;
            cbItem_ActChargeOn.Tag = Utils.DocumentTypes.ActChargeOn;
        }

        private void cbDocumentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Utils.DocumentTypes selectedType = (Utils.DocumentTypes)(((sender as ComboBox).SelectedItem as ComboBoxItem).Tag);
            if (selectedType == Utils.DocumentTypes.WBInvoiceFromMe || selectedType == Utils.DocumentTypes.WBReturnFromMe)
            {
                cbOrgList.IsEnabled = true;
            }
            else if (selectedType == Utils.DocumentTypes.ActWriteOff || selectedType == Utils.DocumentTypes.ActChargeOn)
            {
                cbOrgList.IsEnabled = false;
            }
        }

    }
}
