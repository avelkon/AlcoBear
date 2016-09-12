using System;
using System.Xml.Linq;

namespace AlcoBear
{
    public sealed class FormBRegInfo : IEquatable<Invoice>
    {
        public string date;
        public string wbRegId;
        public string number;
        public string clientRegId;
        public string identity;
        public string url;

        public bool Equals(Invoice invoice)
        {
            return (this.number.Equals(invoice.Number)) && 
                   (this.date.Equals(invoice.CreateDate)) &&
                   (this.clientRegId.Equals(invoice.Shipper.ClientRegID)) && 
                   (String.IsNullOrWhiteSpace(this.identity) || this.identity.Equals(invoice.Identity));
        }

        public void ParseXML(string url)
        {
            try
            {
                this.url = url;
                XElement formB = XElement.Load(url);
                XNamespace ns = formB.GetNamespaceOfPrefix("ns");
                XNamespace wbr = formB.GetNamespaceOfPrefix("wbr");
                XNamespace oref = formB.GetNamespaceOfPrefix("oref");
                XElement formBHeader = formB.Element(ns + "Document").Element(ns + "TTNInformBReg").Element(wbr + "Header");
                this.number = formBHeader.Element(wbr + "WBNUMBER").Value;
                this.date = formBHeader.Element(wbr + "WBDate").Value;
                this.clientRegId = formBHeader.Element(wbr + "Shipper").Element(oref + "ClientRegId").Value;
                try { this.identity = formBHeader.Element(wbr + "Identity").Value.Trim(); }
                catch (NullReferenceException) { this.identity = null; }
                this.wbRegId = formBHeader.Element(wbr + "WBRegId").Value;
            }
            catch
            {
                throw;
            }
        }
    }
}
