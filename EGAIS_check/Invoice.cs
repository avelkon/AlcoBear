using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AlcoBear
{
    /// <summary>
    /// (ЕГАИС) Класс ТТН
    /// </summary>
    public sealed class Invoice
    {
        /// <summary>
        /// ФСРАР ID
        /// </summary>
        public string OwnerFSRAR_ID { get; set; }

        /// <summary>
        /// Номер ТТН
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Дата создания ТТН (yyyy-mm-dd)
        /// </summary>
        public string CreateDate {get; set;}

        /// <summary>
        /// Identity накладной
        /// </summary>
        public string Identity { get; set;}

        /// <summary>
        /// Дата отгрузки товара
        /// </summary>
        public string ShippingDate { get; set; }

        /// <summary>
        /// ID накладной в системе ЕГАИС
        /// </summary>
        public string WBRegId { get; set; }

        /// <summary>
        /// URL файла ТТН
        /// </summary>
        public string WayBillUrl { get; set; }

        /// <summary>
        /// URL файла справки по форме Б
        /// </summary>
        public string FormBRegIdUrl { get; set; }

        public string WayBillType { get; set; }

        /// <summary>
        /// Отправитель ТТН
        /// </summary>
        public Contractor Shipper { get; set; }

        /// <summary>
        /// Получатель ТТН
        /// </summary>
        public Contractor Consignee { get; set; }

        /// <summary>
        /// Список позиций в ТТН
        /// </summary>
        public List<WayBillPosition> Positions;

        public Invoice()
        {
            this.Shipper = new Contractor();
            this.Consignee = new Contractor();
            this.Positions = new List<WayBillPosition>();
        }

        /// <summary>
        /// Загрузка накладной из файла
        /// </summary>
        /// <param name="url">адрес накладной</param>
        public void ParseXML(string url)
        {
            try
            {
                this.WayBillUrl = url;
                XElement invoice_root = XElement.Load(url);
                XNamespace ns = invoice_root.GetNamespaceOfPrefix("ns");
                XNamespace wb = invoice_root.GetNamespaceOfPrefix("wb");
                XNamespace pref = invoice_root.GetNamespaceOfPrefix("pref");
                XNamespace oref = invoice_root.GetNamespaceOfPrefix("oref");
                this.OwnerFSRAR_ID = invoice_root.Element(ns + "Owner").Element(ns + "FSRAR_ID").Value;
                try { this.Identity = invoice_root.Element(ns + "Document").Element(ns + "WayBill").Element(wb + "Identity").Value.Trim(); }
                catch (NullReferenceException) { this.Identity = ""; }
                XElement node = invoice_root.Element(ns + "Document").Element(ns + "WayBill").Element(wb + "Header");
                try { this.WayBillType = node.Element(wb + "Type").Value; }
                catch (NullReferenceException) { this.WayBillType = null; }
                this.Number = node.Element(wb + "NUMBER").Value;
                this.CreateDate = node.Element(wb + "Date").Value;
                this.ShippingDate = node.Element(wb + "ShippingDate").Value;
                this.Shipper.ParseXML(node.Element(wb + "Shipper"));
                if (this.Shipper != null && this.Shipper.IsValid())
                {
                    DataBaseEntry.AddContractor(this.Shipper);
                }
                this.Consignee.ParseXML(node.Element(wb + "Consignee"));
                //загрузка позиций из накладной
                node = invoice_root.Element(ns + "Document").Element(ns + "WayBill").Element(wb + "Content");
                foreach (XElement pos_node in node.Elements(wb + "Position"))
                {
                    this.Positions.Add(new WayBillPosition(pos_node));
                }
                //-----------------------------
            }
            catch
            {
                throw;
            }
        }
            
    }
}
