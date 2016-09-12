using System;
using System.Globalization;
using System.Xml.Linq;

namespace AlcoBear
{
    /// <summary>
    /// Класс позиции продукта в УТМ. Абстрактный.
    /// Используйте [WayBillPosition] и [StockPosition]
    /// </summary>
    public abstract class Position : IEquatable<Position>
    {
        /// <summary>
        /// Полное название товара в позиции
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Объем бутылки товара в позиции
        /// </summary>
        public double Capacity { get; set; }

        /// <summary>
        /// Количество товара в позиции
        /// </summary>
        public double Quantity { get; set; }

        private double price = 0;
        public double Price
        {
            get { return price; }
            set { price = value < 0 ? 0 : value; }
        }

        /// <summary>
        /// Код алкогольной продукции
        /// </summary>
        public string ProductVCode { get; set; }

        /// <summary>
        /// Код алкогольной продукции в ЕГАИС
        /// </summary>
        public string AlcCode { get; set; }

        /// <summary>
        /// Крепость алкогольной продукции
        /// </summary>
        public double AlcVolume { get; set; }

        /// <summary>
        /// RegID справки 'A'
        /// </summary>
        public string InformARegID { get; set; }

        /// <summary>
        /// RegID справки 'B'
        /// </summary>
        public string InformBRegID { get; set; }

        /// <summary>
        /// TRUE если позиция относится к пиву, иначе FALSE
        /// </summary>
        public bool IsBeer;

        /// <summary>
        /// Компания-производитель продукта
        /// </summary>
        public Contractor Producer { get; set; }
        /// <summary>
        /// Компания-импортер продукта
        /// </summary>
        public Contractor Importer { get; set; }

        public abstract bool ParseXML(XElement node);

        public abstract void AddToXML(XElement XMLNode, int identity);

        public bool Equals(Position position)
        {
            return this.FullName.Equals(position.FullName) &&
                this.Capacity == position.Capacity &&
                this.InformARegID.Equals(position.InformARegID) &&
                this.InformBRegID.Equals(position.InformBRegID);
        }

        public static Exception NotValidPositionException = new Exception("Не заполнена обязательная информация о позиции");

        public bool IsValid()
        {
            return !(String.IsNullOrWhiteSpace(this.FullName) ||
                     String.IsNullOrWhiteSpace(this.AlcCode) ||
                     String.IsNullOrWhiteSpace(this.ProductVCode) ||
                     String.IsNullOrWhiteSpace(this.InformARegID) ||
                     String.IsNullOrWhiteSpace(this.InformBRegID));
        }

        

    }

    /// <summary>
    /// Класс позиции во входящей ТТН
    /// </summary>
    public sealed class WayBillPosition : Position
    {
        /// <summary>
        /// Идентификатор позиции
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// Партия товара
        /// </summary>
        public string Party { get; set; }
        
        /// <summary>
        /// Создает новый экземпляр позиции
        /// </summary>
        /// <param name="node">XML узел позиции</param>
        public WayBillPosition(XElement node)
        {
            this.ParseXML(node);
        }

        /// <summary>
        /// Парсит узел, достает из него описание позиции
        /// </summary>
        /// <param name="node">XML узел позиции</param>
        /// <returns>TRUE при успешном парсинге XML, иначе FALSE</returns>
        public override bool ParseXML(XElement node)
        {
            try
            {
                XNamespace wb = node.GetNamespaceOfPrefix("wb");
                XNamespace pref = node.GetNamespaceOfPrefix("pref");
                //Обязательные поля
                this.FullName = node.Element(wb + "Product").Element(pref + "FullName").Value;
                //Не обязательные поля
                try { this.AlcCode = node.Element(wb + "Product").Element(pref + "AlcCode").Value; }
                catch (NullReferenceException) { this.AlcCode = null; }

                try { this.Identity = node.Element(wb + "Identity").Value; }
                catch (NullReferenceException) { this.Identity = null; }

                try { this.ProductVCode = node.Element(wb + "Product").Element(pref + "ProductVCode").Value; }
                catch (NullReferenceException) { this.ProductVCode = null; }

                try { this.Party = node.Element(wb + "Party").Value; }
                catch (NullReferenceException) { this.Party = null; }

                try { this.InformARegID = node.Element(wb + "InformA").Element(pref + "RegId").Value; }
                catch (NullReferenceException) { this.InformARegID = null; }

                try { this.InformBRegID = node.Element(wb + "InformB").Element(pref + "RegId").Value; }
                catch (NullReferenceException) { this.InformBRegID = null; }

                //Определяем правильный разделитель целой и дробной части
                char badSeparator = '.', goodSeparator = ',';
                if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.Equals(badSeparator))
                {
                    badSeparator = ',';
                    goodSeparator = '.';
                }
                //-------------------------------------------------------
                //Парсинг числовых значений
                try { this.Capacity = Convert.ToDouble(node.Element(wb + "Product").Element(pref + "Capacity").Value.Replace(badSeparator, goodSeparator)); }
                catch { this.Capacity = 0; }

                try { this.Quantity = Convert.ToDouble(node.Element(wb + "Quantity").Value.Replace(badSeparator, goodSeparator)); }
                catch { this.Quantity = 0; }

                try { this.Price = Convert.ToDouble(node.Element(wb + "Price").Value.Replace(badSeparator, goodSeparator)); }
                catch { this.Price = 0; }

                try { this.AlcVolume = Convert.ToDouble(node.Element(wb + "Product").Element(pref + "AlcVolume").Value.Replace(badSeparator, goodSeparator)); }
                catch { this.AlcVolume = 0; }
                //---------------------------
                //Попытка парсинга тега <Producer>. Если нету, значит накладная на пивас
                try
                {
                    this.Producer = new Contractor();
                    this.Producer.ParseXML(node.Element(wb + "Product").Element(pref + "Producer"));
                    this.IsBeer = false;
                }
                catch (NullReferenceException) { this.Producer = null; this.IsBeer = true; }
                catch (Exception) { throw; }
                //Попытка парсинга тега <Importer>. Если нету, значит накладная на пивас
                try
                {
                    this.Importer = new Contractor();
                    this.Importer.ParseXML(node.Element(wb + "Product").Element(pref + "Importer"));
                }
                catch (NullReferenceException) { this.Importer = null; }
                catch (Exception) { throw; }
                return true;
            }
            catch (NullReferenceException)
            {
                Utils.WriteLog("Ошибка парсинга позиции в WayBill", Utils.MessageType.WARRING);
            }
            catch (Exception ex)
            {
                Utils.WriteLog(ex.Message, Utils.MessageType.ERROR);
            }
            return false;
        }

        public override void AddToXML(XElement XMLNode, int identity)
        {
            try
            {
                XNamespace wb = XMLNode.GetNamespaceOfPrefix("wb");
                XNamespace pref = XMLNode.GetNamespaceOfPrefix("pref");
                XMLNode.Add(new XElement(wb + "Identity", identity));
                XMLNode.Add(new XElement(wb + "Quantity", this.Quantity));
                XMLNode.Add(new XElement(wb + "Price", this.Price));
                XMLNode.Add(new XElement(wb + "Product"));
                XMLNode.Element(wb + "Product").Add(new XElement(pref + "FullName", this.FullName));
                XMLNode.Element(wb + "Product").Add(new XElement(pref + "AlcCode", this.AlcCode));
                XMLNode.Element(wb + "Product").Add(new XElement(pref + "ProductVCode", this.ProductVCode));
                XMLNode.Add(new XElement(wb + "InformA"));
                XMLNode.Element(wb + "InformA").Add(new XElement(pref + "RegId", this.InformARegID));
                XMLNode.Add(new XElement(wb + "InformB"));
                XMLNode.Element(wb + "InformB").Add(new XElement(pref + "InformBItem"));
                XMLNode.Element(wb + "InformB").Element(pref + "InformBItem").Add(new XElement(pref + "BRegId", this.InformBRegID));
            }
            catch { throw; }
        }
    }

    /// <summary>
    /// Класс позиции в XML остатков
    /// </summary>
    public class StockPosition : Position, IEquatable<StockPosition>
    {
        private double quantityToReturn = 0;
        public double QuantityToReturn
        {
            get { return quantityToReturn; }
            set { quantityToReturn = value < 0 ? 0 : (value > this.Quantity ? this.Quantity : value); }
        }

        public bool Equals(StockPosition position)
        {
            return this.FullName.Equals(position.FullName) &&
                this.Capacity == position.Capacity &&
                this.InformARegID.Equals(position.InformARegID) &&
                this.InformBRegID.Equals(position.InformBRegID);
        }

        public StockPosition() 
        {
            
        }

        public StockPosition(XElement node)
        {
            this.ParseXML(node);
        }

        public override bool ParseXML(XElement node)
        {
            try
            {
                XNamespace rst = node.GetNamespaceOfPrefix("rst");
                XNamespace pref = node.GetNamespaceOfPrefix("pref");
                //Обязательные поля
                this.FullName = node.Element(rst + "Product").Element(pref + "FullName").Value;
                //Не обязательные поля
                try { this.AlcCode = node.Element(rst + "Product").Element(pref + "AlcCode").Value; }
                catch (NullReferenceException) { this.AlcCode = null; }

                try { this.ProductVCode = node.Element(rst + "Product").Element(pref + "ProductVCode").Value; }
                catch (NullReferenceException) { this.ProductVCode = null; }

                try { this.InformARegID = node.Element(rst + "InformARegId").Value; }
                catch (NullReferenceException) { this.InformARegID = null; }

                try { this.InformBRegID = node.Element(rst + "InformBRegId").Value; }
                catch (NullReferenceException) { this.InformBRegID = null; }

                //Определяем правильный разделитель целой и дробной части
                char badSeparator = '.', goodSeparator = ',';
                if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.Equals(badSeparator))
                {
                    badSeparator = ',';
                    goodSeparator = '.';
                }
                //-------------------------------------------------------
                //Парсинг числовых значений
                try { this.Capacity = Convert.ToDouble(node.Element(rst + "Product").Element(pref + "Capacity").Value.Replace(badSeparator, goodSeparator)); }
                catch { this.Capacity = 0; }

                try { this.Quantity = Convert.ToDouble(node.Element(rst + "Quantity").Value.Replace(badSeparator, goodSeparator)); }
                catch { this.Quantity = 0; }

                try { this.AlcVolume = Convert.ToDouble(node.Element(rst + "Product").Element(pref + "AlcVolume").Value.Replace(badSeparator, goodSeparator)); }
                catch { this.AlcVolume = 0; }
                //---------------------------
                //Попытка парсинга тега <Producer>.
                try
                {
                    this.Producer = new Contractor();
                    this.Producer.ParseXML(node.Element(rst + "Product").Element(pref + "Producer"));
                }
                catch (NullReferenceException) { this.Producer = null; }
                catch (Exception) { throw; }
                //Попытка парсинга тега <Importer>. Если нету, значит накладная на пивас
                try
                {
                    this.Importer = new Contractor();
                    this.Importer.ParseXML(node.Element(rst + "Product").Element(pref + "Importer"));
                    this.IsBeer = false;
                }
                catch (NullReferenceException) { this.Importer = null; this.IsBeer = true; }
                catch (Exception) { throw; }
                return true;
            }
            catch (ArgumentNullException)
            {
                Utils.WriteLog("Ошибка парсинга позиции в ReplyRests", Utils.MessageType.WARRING);
            }
            catch (Exception ex)
            {
                Utils.WriteLog(ex.Message, Utils.MessageType.ERROR);
            }
            return false;
        }

        public void AddToXML_ActWriteOf(XElement XMLNode, int identity) 
        {
            try
            {
                XNamespace awr = XMLNode.GetNamespaceOfPrefix("awr");
                XNamespace pref = XMLNode.GetNamespaceOfPrefix("pref");
                XMLNode.Add(new XElement(awr + "Identity", identity));
                XMLNode.Add(new XElement(awr + "Quantity", this.QuantityToReturn));
                XMLNode.Add(new XElement(awr + "InformB"));
                XMLNode.Element(awr + "InformB").Add(new XElement(pref + "BRegId", this.InformBRegID));
            }
            catch { throw; }
        }

        public override void AddToXML(XElement XMLNode, int identity)
        {
            try
            {
                XNamespace wb = XMLNode.GetNamespaceOfPrefix("wb");
                XNamespace pref = XMLNode.GetNamespaceOfPrefix("pref");
                XMLNode.Add(new XElement(wb + "Identity", identity));
                XMLNode.Add(new XElement(wb + "Quantity", this.QuantityToReturn));
                XMLNode.Add(new XElement(wb + "Price", this.Price));
                XMLNode.Add(new XElement(wb + "Product"));
                XMLNode.Element(wb + "Product").Add(new XElement(pref + "FullName", this.FullName));
                XMLNode.Element(wb + "Product").Add(new XElement(pref + "AlcCode", this.AlcCode));
                XMLNode.Element(wb + "Product").Add(new XElement(pref + "ProductVCode", this.ProductVCode));
                XMLNode.Add(new XElement(wb + "InformA"));
                XMLNode.Element(wb + "InformA").Add(new XElement(pref + "RegId", this.InformARegID));
                XMLNode.Add(new XElement(wb + "InformB"));
                XMLNode.Element(wb + "InformB").Add(new XElement(pref + "InformBItem"));
                XMLNode.Element(wb + "InformB").Element(pref + "InformBItem").Add(new XElement(pref + "BRegId", this.InformBRegID));
            }
            catch { throw; }
        }
    }
}
