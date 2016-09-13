using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace AlcoBear
{
    public static class Utils
    {
        //<body>
        /// <summary>
        /// Виды сообщений в LOG файле
        /// </summary>
        public enum MessageType : int { MSG, ERROR, WARRING };        

        public enum DocumentTypes { WBInvoiceFromMe, WBReturnFromMe, WBInvoiceToMe, WBReturnToMe, ActWriteOff, ActChargeOn };

        private const string UpdateInfo_login = @"egais_check";

        private const string UpdateInfo_password = @"Cktgnjy";

        public static class URLs
        {
            /// <summary>
            /// URL списка входящих документов
            /// (default: /opt/out)
            /// </summary>
            public static string incomeDocuments = BuildURL("opt/out");

            /// <summary>
            /// URL, на который отправляются акты подтверждения ТТН
            /// (default: /opt/in/WayBillAct)
            /// </summary>
            public static string outcomeWayBillAct = BuildURL("opt/in/WayBillAct");

            /// <summary>
            /// URL, на который отправляются запросы остатков
            /// (default: /opt/in/QueryRests)
            /// </summary>
            public static string restsQuery = BuildURL("opt/in/QueryRests");

            /// <summary>
            /// URL, на который отправляются запросы справочника организаций
            /// (default: /opt/in/QueryPartner)
            /// </summary>
            public static string QuerryPartner = BuildURL("opt/in/QueryPartner");

            /// <summary>
            /// URL, на который отправляются ТТН
            /// (default: /opt/in/WayBill)
            /// </summary>
            public static string OutcomeWayBill = BuildURL("opt/in/WayBill");


            /// <summary>
            /// URL, на который отправляются акты списания
            /// (default: /opt/in/ActWriteOff)
            /// </summary>
            public static string ActWriteOff = BuildURL("opt/in/ActWriteOff");
        }

        /// <summary>
        /// Причина отказа от накладной
        /// </summary>
        public static string reject_note = "";

        /// <summary>
        /// Дата отказа от накладной
        /// </summary>
        public static DateTime reject_date;

        /// <summary>
        /// Список загруженных ТТН
        /// </summary>
        public static List<Invoice> incomeWayBillsList = new List<Invoice>();

        /// <summary>
        /// Список загруженных справок "Б"
        /// </summary>
        public static List<FormBRegInfo> incomeFormBList = new List<FormBRegInfo>();

        /// <summary>
        /// Остатки продукции на складе (ЕГАИС)
        /// </summary>
        public static ObservableCollection<StockPosition> restsList = new ObservableCollection<StockPosition>();

        /// <summary>
        /// Формирует URL из текущих настроек ЕГАИС
        /// </summary>
        public static void BuildURL()
        {
            URLs.incomeDocuments = BuildURL("opt/out");
            URLs.restsQuery = BuildURL("opt/in/QueryRests");
            URLs.outcomeWayBillAct = BuildURL("opt/in/WayBillAct");
        }

        /// <summary>
        /// Формирует URL из текущих настроек ЕГАИС
        /// </summary>
        /// <param name="path">дополнительный путь</param>
        /// <returns>сформированный URL</returns>
        private static string BuildURL(string path)
        {
            UriBuilder urb = new UriBuilder()
            {
                Host = Properties.Settings.Default.UTM_host,
                Port = Properties.Settings.Default.UTM_port,
                Path = path
            };
            return urb.ToString();
        }

        /// <summary>
        /// Пишет сообщение в log-файл
        /// </summary>
        /// <param name="message">Тело сообщения</param>
        /// <param name="type">Тип сообщения [Utils.MessageType]</param>
        public static void WriteLog(string message, MessageType type = MessageType.MSG)
        {
            string prefix = "[" + DateTime.Now.ToString() + "] ";
            prefix += "[" + type.ToString() + "]";
            File.AppendAllText(Properties.Settings.Default.logFilePath, prefix + " " + message + "\r\n");
        }

        /// <summary>
        /// Отправляет XML-файл в ЕГАИС
        /// </summary>
        /// <param name="fileContent">содержимое XML файла</param>
        /// <param name="url">url на который посылается запрос, по умолчанию это "/opt/in/WayBillAct"</param>
        /// <returns>TRUE, если запрос успешно отправлен, иначе FALSE</returns>
        public static bool SendXML(string fileContent, string url = null)
        {
            return SendXmlGetResponse(fileContent.Trim(), url) == null ? false : true;
        }

        /// <summary>
        /// Отправляет XML-файл в ЕГАИС
        /// </summary>
        /// <param name="fileContent">содержимое XML файла</param>
        /// <param name="url">url на который посылается запрос, по умолчанию это "/opt/in/WayBillAct"</param>
        /// <returns>Возвращает поток с ответом от сервера</returns>
        public static string SendXmlGetResponse(string fileContent, string url = null)
        {
            try
            {
                if (!Directory.Exists("Upload")) Directory.CreateDirectory("Upload");
                File.WriteAllText("Upload\\" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".xml", fileContent);
                System.Net.ServicePointManager.Expect100Continue = false;
                string boundary = "xxxxxxxxxxxxxxxxxxxxxxx" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(String.IsNullOrWhiteSpace(url) ? URLs.outcomeWayBillAct : url);
                wr.Method = "POST";
                wr.ContentType = "multipart/form-data; boundary=" + boundary;
                wr.Date = DateTime.Now;
                Stream rs = wr.GetRequestStream();
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string header = "Content-Disposition: form-data; name=\"xml_file\"; filename=\"@wbac.xml\"\r\nContent-Type: application/xml\r\n\r\n";
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);
                byte[] fileContentBytes = Encoding.UTF8.GetBytes(fileContent);
                rs.Write(fileContentBytes, 0, fileContentBytes.Length);
                byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
                rs.Close();
                using (WebResponse wresp = wr.GetResponse())
                {
                    if (wr.HaveResponse)
                    {
                        using (Stream respst = wresp.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(respst))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                    wresp.Close();
                }
                return null;
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, MessageType.ERROR);
                return null;
            }
        }

        /// <summary>
        /// Отправляет "DELETE" запрос в УТМ
        /// </summary>
        /// <param name="fileUrl">URL файла</param>
        /// <returns>TRUE - если файл успешно удален, иначе FALSE</returns>
        public static bool DeleteFile(string fileUrl)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fileUrl);
                request.Method = "DELETE";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    WriteLog("Не удалось удалить файл \"" + fileUrl + "\"", MessageType.WARRING);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, MessageType.ERROR);
                return false;
            }
        }

        /// <summary>
        /// Удаляет все документы из списка [URLListToDelete]
        /// </summary>
        /// <param name="URLListToDelete">Список документов на удаление</param>
        /// <returns>TRUE - если файлы успешно удалены, иначе FALSE</returns>
        public static bool DeleteFile(ICollection<string> URLListToDelete)
        {
            try
            {
                foreach (string file in URLListToDelete) DeleteFile(file);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, MessageType.ERROR);
                return false;
            }
        }

        /// <summary>
        /// Загружает документы из списка входящих документов УТМ
        /// </summary>
        public static void DownloadDocuments(bool parseWayBills = true, bool parseRests = true, bool parseParthers = true )
        {
            try
            {
                incomeWayBillsList.Clear();
                incomeFormBList.Clear();
                XDocument egaisDocument = XDocument.Load(URLs.incomeDocuments);
                List<string> docsToDel = new List<string>();
                foreach (XElement node in egaisDocument.Root.Elements("url"))
                {
                    string documentUrl = node.Value.Trim();
                    //WayBills
                    if (parseWayBills && documentUrl.ToLower().Contains("/waybill/"))
                    {
                        try
                        {
                            Invoice invoice = new Invoice();
                            invoice.ParseXML(documentUrl);
                            if (invoice.WayBillType.Equals(Utils.DocumentTypes.WBReturnFromMe.ToString()))
                            {
                                //Накладная подтверждение возврата
                                //incomeWayBillsList.Add(invoice);
                            }
                            else if (invoice.WayBillType.Equals(Utils.DocumentTypes.WBInvoiceFromMe.ToString()))
                            {
                                //Накладная подтверждения исходящей накладной
                                incomeWayBillsList.Add(invoice);
                            }
                            else
                            {
                                incomeWayBillsList.Add(invoice);
                            }
                        }
                        catch
                        {
                            throw new Exception("Ошибка при парсинге WayBill по адресу " + documentUrl);
                        }
                    }
                    else if (parseWayBills && documentUrl.ToLower().Contains("/formbreginfo/"))
                    {
                        try
                        {
                            FormBRegInfo inv = new FormBRegInfo();
                            inv.ParseXML(documentUrl);
                            incomeFormBList.Add(inv);
                        }
                        catch
                        {
                            throw new Exception("Ошибка при парсинге FormB по адресу " + documentUrl);
                        }
                    }
                    //Rests
                    else if (parseRests && documentUrl.ToLower().Contains("/replyrests/"))
                    {
                        if (node.Attribute("replyId") != null && node.Attribute("replyId").Value.Equals(Properties.Settings.Default.LastRestQueryID))
                        {
                            //Пришел ответ на запрос остатков
                            Parser.RestsXML(documentUrl);
                        }
                        else
                        {
                            docsToDel.Add(documentUrl);
                        }
                    }
                    //ReplyPartner
                    else if (parseParthers && documentUrl.ToLower().Contains("/replypartner/"))
                    {
                        Parser.PatnerXML(documentUrl);
                        docsToDel.Add(documentUrl);
                    }
                    //Ticket
                    else if (documentUrl.ToLower().Contains("/ticket/"))
                    {
                        //DeleteFile(documentUrl);
                    }
                }
                DeleteFile(docsToDel);
                //Поиск соответствий справок 'B' и приходных ТТН
                if (parseWayBills)
                    for (int i = 0; i < incomeWayBillsList.Count; i++)
                    {
                        //if (incomeWayBillsList[i].WayBillType.Equals(Utils.WayBillTypes.WBInvoiceFromMe.ToString()) ||
                        //    incomeWayBillsList[i].WayBillType.Equals(Utils.WayBillTypes.WBReturnFromMe.ToString())) continue;
                        foreach (FormBRegInfo formb in incomeFormBList)
                        {
                            if (formb.Equals(incomeWayBillsList[i]))
                            {
                                incomeWayBillsList[i].WBRegId = formb.wbRegId;
                                incomeWayBillsList[i].FormBRegIdUrl = formb.url;
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                Utils.WriteLog(ex.Message, MessageType.ERROR);
            }
        }

        public static class Parser
        {

            /// <summary>
            /// Парсит XML с остатками
            /// </summary>
            /// <param name="url">URL XML файла</param>
            public static void RestsXML(string url)
            {
                Properties.Settings.Default.LastRestsURL = url;
                Properties.Settings.Default.Save();
                RestsXML(XDocument.Load(url).Root);
            }

            /// <summary>
            /// Парсит XML с остатками
            /// </summary>
            /// <param name="restsXML">XML файл остатков</param>
            private static void RestsXML(XElement restsXML)
            {
                XNamespace ns = restsXML.GetNamespaceOfPrefix("ns");
                //Ищет узел <ns:ReplyRests>
                XElement nodeReplyRests = restsXML.Name.LocalName.Equals("Documents") ?
                     restsXML.Element(ns + "Document").Element(ns + "ReplyRests") : (restsXML.Name.LocalName.Equals("Document") ? restsXML.Element(ns + "ReplyRests") : restsXML);
                XNamespace rst = nodeReplyRests.GetNamespaceOfPrefix("rst");
                Properties.Settings.Default.LastRestsDateTime = Convert.ToDateTime(nodeReplyRests.Element(rst + "RestsDate").Value);
                Properties.Settings.Default.Save();
                restsList.Clear();
                foreach (XElement stockPosNode in nodeReplyRests.Element(rst + "Products").Elements(rst + "StockPosition"))
                {
                    restsList.Add(new StockPosition(stockPosNode));
                }
            }

            public static void PatnerXML(string url)
            {
                PartnerXML(XDocument.Load(url).Root);
            }

            private static void PartnerXML(XElement partnerXML)
            {
                XNamespace ns = partnerXML.GetNamespaceOfPrefix("ns");
                //Ищет узел <ns:ReplyClient>
                XElement nodeReplyClient = partnerXML.Name.LocalName.Equals("Documents") ?
                     partnerXML.Element(ns + "Document").Element(ns + "ReplyClient") : (partnerXML.Name.LocalName.Equals("Document") ? partnerXML.Element(ns + "ReplyClient") : partnerXML);
                XNamespace rc = partnerXML.GetNamespaceOfPrefix("rc");
                foreach (XElement clientNode in nodeReplyClient.Element(rc + "Clients").Elements(rc + "Client"))
                {
                    DataBaseEntry.AddContractor(new Contractor(clientNode), true);
                }
            }
        }

        public static class Requests
        {
            /// <summary>
            /// Отправляет запрос остатков в УТМ
            /// </summary>
            /// <returns>TRUE если запрос успешно отправлен в УТМ, иначе FALSE</returns>
            public static bool Rests()
            {
                string resp = Utils.SendXmlGetResponse(String.Format(Properties.Resources.XMLPattern_QueryRests, Properties.Settings.Default.FSRAR_ID), Utils.URLs.restsQuery);
                try
                {
                    XDocument RestsQueryResponse = XDocument.Parse(resp);
                    Properties.Settings.Default.LastRestQueryID = RestsQueryResponse.Root.Element("url").Value.Trim();
                    Properties.Settings.Default.LastRestQueryDateTime = DateTime.Now;
                    Properties.Settings.Default.Save();
                    restsList.Clear();
                    return true;
                }
                catch (NullReferenceException)
                {
                    Utils.WriteLog("Запрос не был отправлен в УТМ", MessageType.WARRING);
                }
                catch (Exception ex)
                {
                    Utils.WriteLog(ex.Message, MessageType.ERROR);
                }
                return false;
            }

            /// <summary>
            /// Отправляет запрос контрагента в УТМ
            /// </summary>
            /// <returns>TRUE если запрос успешно отправлен в УТМ, иначе FALSE</returns>
            public static bool Contragent(string value, bool useINN = false)
            {
                if (String.IsNullOrWhiteSpace(value)) return false;
                try
                {
                    string paramName = useINN ? "ИНН" : "СИО";
                    return Utils.SendXML(String.Format(Properties.Resources.XMLPattern_QueryPartner, Properties.Settings.Default.FSRAR_ID, paramName, value), Utils.URLs.QuerryPartner);
                }
                catch (NullReferenceException)
                {
                    Utils.WriteLog("Запрос не был отправлен в УТМ", MessageType.WARRING);
                }
                catch (Exception ex)
                {
                    Utils.WriteLog(ex.Message, MessageType.ERROR);
                }
                return false;
            }
        }
        
        /// <summary>
        /// Формирует новый случайный номер накладной
        /// </summary>
        /// <returns>Строка с номером накладной</return>   
        private static string nextInvoiceNumber(string pref)
        {
            return "albr-" + pref + "-" + new Random().Next(1, 10000000).ToString() + "-" + new Random().Next(1, 1000).ToString();
        }

        public static bool SendActWriteOff(IEnumerable<StockPosition> positionsList, string writeOffReason)
        {
            XDocument actWriteOff = XDocument.Parse(String.Format(
                Properties.Resources.XMLPattern_ActWriteOff,
                Properties.Settings.Default.FSRAR_ID,
                nextInvoiceNumber("awo"),
                DateTime.Now.ToString("yyyy-MM-dd"),
                writeOffReason
                ));
            XNamespace ns = actWriteOff.Root.GetNamespaceOfPrefix("ns");
            XNamespace awr = actWriteOff.Root.GetNamespaceOfPrefix("awr");
            XElement contentNode = actWriteOff.Root.Element(ns + "Document").Element(ns + "ActWriteOff").Element(awr + "Content");
            int ident = 1;
            foreach (StockPosition position in positionsList)
            {
                if (position.QuantityToReturn > 0 && !String.IsNullOrWhiteSpace(position.InformBRegID))
                {
                    contentNode.AddFirst(new XElement(awr + "Position"));
                    position.AddToXML_ActWriteOf(contentNode.Element(awr + "Position"), ident++);
                }
            }
            return SendXML(actWriteOff.ToString(), URLs.ActWriteOff);
        }

        public static bool SendWayBill(Contractor consignee, IEnumerable<StockPosition> toWBPositionsList, DocumentTypes type)
        {
            //Сформировать накладную на возврат
            if (!consignee.IsValid() || !DataBaseEntry.ThisCompany.IsValid()) throw Contractor.NotValidContragentException;
            XDocument waybill = XDocument.Parse(String.Format(
                Properties.Resources.XMLPattern_WayBill,
                Properties.Settings.Default.FSRAR_ID,
                nextInvoiceNumber("wb"),
                DateTime.Now.ToString("yyyy-MM-dd"),
                type.ToString()
                ));
            XNamespace ns = waybill.Root.GetNamespaceOfPrefix("ns");
            XNamespace wb = waybill.Root.GetNamespaceOfPrefix("wb");
            //Заполняем тег <Header>
            XElement currentNode = waybill.Root.Element(ns + "Document").Element(ns + "WayBill").Element(wb + "Header");
            //Заполняем отправителя (shipper)
            try
            {
                DataBaseEntry.ThisCompany.AddToXML(currentNode.Element(wb + "Shipper"));
            }
            catch (NullReferenceException) { throw new NullReferenceException("Нет информации об отправителе"); }
            //Заполняем получателя (consignee)
            try
            {
                consignee.AddToXML(currentNode.Element(wb + "Consignee"));
            }
            catch (NullReferenceException) { throw new NullReferenceException("Нет информации о получателе"); }
            //Заполняем тег <Content>
            currentNode = waybill.Root.Element(ns + "Document").Element(ns + "WayBill").Element(wb + "Content");
            int ident = 1;
            foreach (StockPosition position in toWBPositionsList)
            {
                if (position.IsValid() && position.QuantityToReturn > 0)
                {
                    currentNode.AddFirst(new XElement(wb + "Position"));
                    position.AddToXML(currentNode.Element(wb + "Position"), ident++);
                }
                
            }
            return SendXML(waybill.ToString(), URLs.OutcomeWayBill);
        }

        /// <summary>
        /// Проверяет обновление и обновляется до последней версии
        /// </summary>
        /// <returns>TRUE - если требуется обновление, FALSE - если нет</returns>
        public static bool CheckUpdate()
        {
            Version last_version = null;
            Version current_version = Assembly.GetExecutingAssembly().GetName().Version;
            string[] updateFileContent = null;
            try
            {
                using (WebClient wc = new WebClient())
                   updateFileContent = wc.DownloadString(String.Format(Properties.Settings.Default.UpdateInfoFileUrl, UpdateInfo_login, UpdateInfo_password)).Split('\r', '\n');
            }
            catch (WebException)
            {
                throw new WebException("Ошибка при подключении к серверу обновлений");
            }
            //Парсинг параметров обновления
            foreach (string s in updateFileContent)
                if (!String.IsNullOrWhiteSpace(s))
                {
                    string[] setting = s.Trim().Split('=');
                    if (setting[0].Equals("[update]", StringComparison.CurrentCultureIgnoreCase) && !(setting[1].Equals("1") || setting[1].ToLower().Equals("on"))) return false;
                    if (setting[0].Equals("[last_version]", StringComparison.CurrentCultureIgnoreCase)) 
                    {
                        try
                        {
                            last_version = new Version(setting[1]);
                        }
                        catch (FormatException)
                        {
                            return false;
                        }
                    }
                }
            //----------------------------
            if (current_version.CompareTo(last_version) < 0) return true;
            else return false;
        }

        public static void StartUpdate()
        {
            System.Diagnostics.Process updater = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "AlcoBearUpdater.exe",
                    Arguments = "--all",
                    UseShellExecute = true
                }
            };
            updater.Start();
            App.Current.Shutdown();
        }

        //</body>
    }
}
