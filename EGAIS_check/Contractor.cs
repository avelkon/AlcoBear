using LiteDB;
using System;
using System.Xml.Linq;

namespace AlcoBear
{
    /// <summary>
    /// (ЕГАИС) Класс контрагента
    /// </summary>
    public class Contractor : IEquatable<Contractor>
    {

        /// <summary>
        /// Класс информации об адресе
        /// </summary>
        public class AdressInfo
        {
            public string Description { get; set; }
            public string Country { get; set; }
            public string RegionCode { get; set; }
        }

        /// <summary>
        /// ID слиента в ЕГАИС (FSRAR_ID)
        /// </summary>
        [BsonId]
        public string ClientRegID { get; set; }

        /// <summary>
        /// ИНН контрагента
        /// </summary>
        public string INN { get; set; }

        /// <summary>
        /// КПП контрагента
        /// </summary>
        public string KPP { get; set; }

        /// <summary>
        /// Полное название контрагента
        /// </summary>
        public string FullName { get; set; }
        
        private string fld_ShortName = "";
        /// <summary>
        /// Сокращенное название контрагента
        /// </summary>
        public string ShortName 
        {
            get { return String.IsNullOrWhiteSpace(fld_ShortName) ? FullName : fld_ShortName; }
            set { fld_ShortName = value; }
        }

        private AdressInfo fld_Address = new AdressInfo();
        /// <summary>
        /// Юридический адрес контрагента
        /// </summary>
        public AdressInfo Address { get { return fld_Address; } set { fld_Address = value; } }

        /// <summary>
        /// Парсит узел, достает из него реквизиты контрагента
        /// </summary>
        /// <param name="contractor">узел XML файла</param>
        public void ParseXML(XElement node)
        {
            try
            {
                XNamespace oref = node.GetNamespaceOfPrefix("oref");
                try
                {
                    this.FullName = node.Element(oref + "FullName").Value;
                    this.ClientRegID = node.Element(oref + "ClientRegId").Value;
                    this.Address.Description = node.Element(oref + "address").Element(oref + "description").Value;
                    this.Address.Country = node.Element(oref + "address").Element(oref + "Country").Value;
                }
                catch (NullReferenceException) 
                { 
                    Utils.WriteLog("Не найден обязательный тег при парсинге контрагента");
                    throw;
                }
                try { this.INN = node.Element(oref + "INN").Value; }
                catch (NullReferenceException) { this.INN = null; }
                try { this.KPP = node.Element(oref + "KPP").Value; }
                catch (NullReferenceException) { this.KPP = null; }
                try { this.ShortName = node.Element(oref + "ShortName").Value; }
                catch (NullReferenceException) { this.ShortName = ""; }
                try { this.Address.RegionCode = node.Element(oref + "address").Element(oref + "RegionCode").Value; }
                catch (NullReferenceException) { this.Address.RegionCode = ""; }
            }
            catch
            {
                throw;
            }
        }

        public Contractor() { }

        public Contractor(XElement node)
        {
            this.ParseXML(node);
        }

        public bool Equals(Contractor contractor)
        {
            return this.INN.Equals(contractor.INN) && this.KPP.Equals(contractor.KPP) && this.FullName.ToLower().Equals(contractor.FullName.ToLower());
        }

        public static Exception NotValidContragentException = new Exception("Не заполнена обязательная информация о контрагенте");

        /// <summary>
        /// Проверяет полноту информации о контрагенте
        /// </summary>
        /// <returns>TRUE если есть вся обязательная информация, иначе FALSE</returns>
        public bool IsValid()
        {
            return !(String.IsNullOrWhiteSpace(this.ClientRegID) ||
                     String.IsNullOrWhiteSpace(this.FullName) ||
                     String.IsNullOrWhiteSpace(this.Address.Country) ||
                     String.IsNullOrWhiteSpace(this.Address.Description));
        }

        /// <summary>
        /// Добавляет в имформацию о контрагенте в XML узел
        /// </summary>
        /// <param name="XMLNode">XML Узел</param>
        public void AddToXML(XElement XMLNode)
        {
            try
            {
                XNamespace oref = XMLNode.GetNamespaceOfPrefix("oref");
                XMLNode.Add(new XElement(oref + "ClientRegId", this.ClientRegID));
                XMLNode.Add(new XElement(oref + "FullName", this.FullName));
                XMLNode.Add(new XElement(oref + "address"));
                XMLNode.Element(oref + "address").Add(new XElement(oref + "Country", this.Address.Country));
                XMLNode.Element(oref + "address").Add(new XElement(oref + "description", this.Address.Description));
            }
            catch { throw; }
        }

        /// <summary>
        /// Строковое представление контрагента
        /// </summary>
        /// <returns>Короткое или полное наименование организации</returns>
        public override string ToString()
        {
            return (String.IsNullOrWhiteSpace(this.ShortName) ? this.FullName : this.ShortName) + (String.IsNullOrWhiteSpace(this.KPP) ? "" : (" (" + this.KPP + ")"));
        }

    }
}
