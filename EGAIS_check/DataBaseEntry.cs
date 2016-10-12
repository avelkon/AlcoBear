using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AlcoBear
{
    /// <summary>
    /// DataBase layer static class
    /// </summary>
    public static class DataBaseEntry
    {
        //<body>

        private static readonly string CName_ApplicationSettings = "C_AppSettings";
        private static readonly string CName_Contractors = "C_Contractors";
        
        private static Contractor thisCompany = null;
        /// <summary>
        /// Данные текущей компании
        /// </summary>
        public static Contractor ThisCompany
        {
            get
            {
                if (thisCompany == null)
                 using (LiteDatabase db = new LiteDatabase(Properties.Settings.Default.dbFilePath))
                 {
                     thisCompany = db.GetCollection<Contractor>(CName_Contractors).FindOne(x => x.ClientRegID.Equals(Properties.Settings.Default.FSRAR_ID));
                 }
                return thisCompany;
            }
            set
            {
                using (LiteDatabase db = new LiteDatabase(Properties.Settings.Default.dbFilePath))
                {
                    db.GetCollection<Contractor>(CName_Contractors).Delete((BsonValue)Properties.Settings.Default.FSRAR_ID);
                    if (value != null)
                    {
                        db.GetCollection<Contractor>(CName_Contractors).Insert(value);
                        thisCompany = value;
                        Properties.Settings.Default.FSRAR_ID = value.ClientRegID;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }
                
        /// <summary>
        /// Запрос данных контрагента из БД
        /// </summary>
        /// <param name="INN">ИНН контрагента</param>
        /// <param name="KPP">КПП контрагента</param>
        /// <returns>Информация о контрагенте</returns>
        public static Contractor GetContractor(string INN, string KPP)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(INN) || String.IsNullOrWhiteSpace(KPP)) return null;
                Contractor cl_contragent;
                using (LiteDatabase db = new LiteDatabase(Properties.Settings.Default.dbFilePath))
                {
                    cl_contragent = db.GetCollection<Contractor>(CName_Contractors).FindOne(x => x.INN.Equals(INN) && x.KPP.Equals(KPP));
                }
                return cl_contragent;
            }
            catch { throw; }
        }

        /// <summary>
        /// Запрос данных контрагента из БД
        /// </summary>
        /// <param name="ClientRegID">RegID контрагента (FSRAR ID)</param>
        /// <returns>Информация о контрагенте</returns>
        public static Contractor GetContractor(string ClientRegID)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(ClientRegID)) return null;
                Contractor cl_contragent;
                using (LiteDatabase db = new LiteDatabase(Properties.Settings.Default.dbFilePath))
                {
                    //cl_contragent = db.GetCollection<Contractor>(CName_Contractors).FindOne(x => x.ClientRegID.Equals(ClientRegID));
                    cl_contragent = db.GetCollection<Contractor>(CName_Contractors).FindById(ClientRegID);
                }
                return cl_contragent;
            }
            catch { throw; }
        }

        /// <summary>
        /// Запрос списка контрагентов с заданным ИНН (всех, если ИНН не указан)
        /// </summary>
        /// <param name="INN">ИНН контрагента</param>
        /// <returns>Список контрагентов</returns>
        public static IEnumerable<Contractor> GetContractors(string INN = null)
        {
            try
            {
                IEnumerable<Contractor> cl_contragent;
                using (LiteDatabase db = new LiteDatabase(Properties.Settings.Default.dbFilePath))
                {
                    cl_contragent = new List<Contractor>(String.IsNullOrWhiteSpace(INN) ?
                        db.GetCollection<Contractor>(CName_Contractors).FindAll() :
                        db.GetCollection<Contractor>(CName_Contractors).Find(x => x.INN.Equals(INN)));
                        
                }
                return cl_contragent;
            }
            catch
            {
                Utils.WriteLog(@"Fail on <GetContractors>", Utils.MessageType.WARRING);
                throw; 
            }
        }

        /// <summary>
        /// Добавляет контрагента в БД
        /// </summary>
        /// <param name="newContragent">Добавляемый контрагент</param>
        /// <param name="reWrite">TRUE если нужно перзаписать данные контрагента, иначе FALSE</param>
        /// <returns>TRUE если контрагента успешно добавлен, иначе FALSE</returns>
        public static bool AddContractor(Contractor newContragent, bool reWrite = false)
        {
            if (!newContragent.IsValid()) return false;
            try
            {
                using (LiteDatabase db = new LiteDatabase(Properties.Settings.Default.dbFilePath))
                {
                    LiteCollection<Contractor> contractors = db.GetCollection<Contractor>(CName_Contractors);
                    if (contractors.Exists(x => x.ClientRegID.Equals(newContragent.ClientRegID)))
                    {
                        if (reWrite) contractors.Update(newContragent.ClientRegID, newContragent);
                        else return false;
                    }
                    else
                    {
                        contractors.Insert(newContragent);
                    }
                }
                return true;
            }
            catch { throw; }
        }

        public static void DeleteContragents()
        {
            try
            {
                using (LiteDatabase db = new LiteDatabase(Properties.Settings.Default.dbFilePath))
                {
                    db.DropCollection(CName_Contractors);
                }
            }
            catch
            {
                throw;
            }
        }

        // </body>
    }
}
