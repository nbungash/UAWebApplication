
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;
using static UAWebApplication.Controllers.TripController;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_VIEW")]
    public class NewTripController : Controller
    {
        private readonly UADbContext _context;
        public NewTripController(UADbContext context)
        {
            _context = context;
        }

        public class NewTripWindowLoadedParam
        {
            public long TripId { get; set; }

        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_VIEW")]
        public async Task<IActionResult> NewTripWindowLoaded([FromBody] NewTripWindowLoadedParam p1)
        {
            NewTripWindowLoadedReturn obj_return = new NewTripWindowLoadedReturn();
            try
            {
                TripTable? tt= await _context.TripTables.FirstOrDefaultAsync(p => p.TripId == p1.TripId);
                if (tt == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.TripObj = new TripDto(tt, _context); 
                
                JournalTable? munshianaRecord =await _context.JournalTables
                            .Where(p => p.TripId ==p1.TripId && p.EntryType == "TM" && p.Credit != null).FirstOrDefaultAsync();
                if (munshianaRecord != null)
                {
                    obj_return.Munshiana = munshianaRecord.Credit.GetValueOrDefault(0);
                }

                List<JournalTable> list1 = await _context.JournalTables.Where(p => p.TripId == p1.TripId &&
                    p.EntryType == "GV" && p.Debit != null).ToListAsync();
                int SNo = 0;
                foreach (var item in list1)
                {
                    JournalTable? credit_record = await _context.JournalTables.Where(p => p.TransId == item.TransId &&
                        p.Credit != null).FirstOrDefaultAsync();
                    obj_return.TripAdvanceList.Add(new TripAdvanceClass(credit_record, _context, ++SNo));
                }
                obj_return.Message = "OK";
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        obj_return.Message = string.Format("{0}", ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        obj_return.Message = string.Format("{0}", ex.InnerException.Message);
                    }
                }
                else
                {
                    obj_return.Message = string.Format("{0}", ex.Message);
                }
            }
            string json = JsonConvert.SerializeObject(obj_return);
            return Json(json);
        }
        public class NewTripWindowLoadedReturn
        {
            public NewTripWindowLoadedReturn()
            {
                TripObj = new TripDto();
                TripAdvanceList = new List<TripAdvanceClass>();
            }
            public string Message { get; set; }
            public TripDto? TripObj { get; set; }
            public decimal? Munshiana { get; set; }
            public List<TripAdvanceClass> TripAdvanceList { get; set; }
        }
        public class TripAdvanceClass : JournalTable
        {
            public TripAdvanceClass() { }
            public TripAdvanceClass(JournalTable jt, UADbContext context,int? SNo)
            {
                this.ChequeNo = jt.ChequeNo;
                this.VoucherNo = jt.VoucherNo;
                this.AccountGroup = context.AccountTables.Where(p => p.AccountId == jt.AccountId)
                    .Select(p => p.GroupType).FirstOrDefault();
                this.AccountTitle = context.AccountTables.Where(p => p.AccountId == jt.AccountId)
                    .Select(p => p.Title).FirstOrDefault();
                this.Debit = jt.Debit;
                this.AccountId = jt.AccountId;
                this.Credit = jt.Credit;
                this.Description = jt.Description;
                this.EntryDate = jt.EntryDate;
                this.EntryType = jt.EntryType;
                this.Id = jt.Id;
                this.TransId = jt.TransId;
                this.SNo = SNo;
            }
            public string? AccountGroup { get; set; }
            public string? AccountTitle { get; set; }
            public int? SNo { get; set; }
        }

        public class SaveParam
        {
            public SaveParam()
            {
                TripAdvanceList = new List<JournalTable>();
            }
            public long TripId { get; set; }

            public int? TokenNo { get; set; }

            public long? Lorry { get; set; }
            public string? LorryString { get; set; }

            public DateTime? InvoiceDate { get; set; }

            public DateTime? EntryDate { get; set; }

            public double? Quantity { get; set; }

            public decimal? Freight { get; set; }

            public decimal? Commission { get; set; }

            public double? ShortQty { get; set; }

            public decimal? ShortAmount { get; set; }

            public long? ShippingId { get; set; }

            public long? DestinationId { get; set; }

            public long? ProductId { get; set; }

            public decimal? CommissionPercent { get; set; }

            public decimal? TaxPercent { get; set; }

            public decimal? Tax { get; set; }

            public long? PartyId { get; set; }
            public decimal? Munshiana { get; set; }
            public List<JournalTable> TripAdvanceList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_ADD")]
        public async Task<IActionResult> Save([FromBody] SaveParam p1)
        {
            SaveReturn obj_return = new SaveReturn();
            try
            {
                if (p1.TripId == 0)
                {
                    TripTable db1 = new TripTable();
                    db1.PartyId = p1.PartyId;
                    db1.EntryDate = p1.EntryDate;
                    db1.Lorry = p1.Lorry;
                    db1.ShippingId = p1.ShippingId;
                    db1.DestinationId = p1.DestinationId;
                    db1.ProductId = p1.ProductId;
                    db1.TokenNo = p1.TokenNo;
                    db1.InvoiceDate = p1.InvoiceDate;
                    db1.Quantity = p1.Quantity;
                    db1.Freight = p1.Freight;
                    db1.CommissionPercent = p1.CommissionPercent;
                    db1.Commission = p1.Commission;
                    db1.TaxPercent = p1.TaxPercent;
                    db1.Tax = p1.Tax;
                    db1.ShortQty = p1.ShortQty;
                    db1.ShortAmount = p1.ShortAmount;
                    db1.TransactionDate = DateTime.Now;
                    _context.TripTables.Add(db1);
                    long? transactionId = await _context.JournalTables.MaxAsync(p => p.TransId) + 1;
                    long? voucherNo = GetVoucherNo(p1.EntryDate);
                    JournalTransactions(_context, db1, p1.Munshiana, transactionId, voucherNo, p1.LorryString);
                    TripAdvaneTransactions(_context, db1, transactionId, p1.TripAdvanceList, false);

                    await _context.SaveChangesAsync();
                    obj_return.Message = "OK";
                }
                else
                {
                    TripTable? db2 = await _context.TripTables.FirstOrDefaultAsync(p => p.TripId == p1.TripId);
                    db2.EntryDate = p1.EntryDate;
                    db2.PartyId = p1.PartyId;
                    db2.Lorry = p1.Lorry;
                    db2.ShippingId = p1.ShippingId;
                    db2.DestinationId = p1.DestinationId;
                    db2.ProductId = p1.ProductId;
                    db2.TokenNo = p1.TokenNo;
                    db2.InvoiceDate = p1.InvoiceDate;
                    db2.Quantity = p1.Quantity;
                    db2.Freight = p1.Freight;
                    db2.CommissionPercent = p1.CommissionPercent;
                    db2.Commission = p1.Commission;
                    db2.TaxPercent = p1.TaxPercent;
                    db2.Tax = p1.Tax;
                    db2.ShortQty = p1.ShortQty;
                    db2.ShortAmount = p1.ShortAmount;
                    db2.TransactionDate = DateTime.Now;
                    long? transactionId = await _context.JournalTables.MaxAsync(p => p.TransId) + 1;
                    JournalTable? list = await _context.JournalTables.Where(p => p.TripId == p1.TripId).FirstOrDefaultAsync();
                    long? voucherNo = null;
                    if (list == null)
                    {
                        voucherNo = GetVoucherNo(p1.EntryDate);
                    }
                    else
                    {
                        voucherNo = list.VoucherNo;
                    }
                    JournalTransactions(_context, db2, p1.Munshiana, transactionId, voucherNo, p1.LorryString);

                    TripAdvaneTransactions(_context, db2, transactionId, p1.TripAdvanceList, true);
                    await _context.SaveChangesAsync();
                    obj_return.Message = "OK";
                }
                obj_return.Message = "OK";
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        obj_return.Message = string.Format("{0}", ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        obj_return.Message = string.Format("{0}", ex.InnerException.Message);
                    }
                }
                else
                {
                    obj_return.Message = string.Format("{0}", ex.Message);
                }
            }
            string json = JsonConvert.SerializeObject(obj_return);
            return Json(json);
        }
        public class SaveReturn
        {
            public SaveReturn()
            {
                //AccountList = new List<AccountDto>();
            }
            public string Message { get; set; }
            //public List<AccountDto> AccountList { get; set; }
        }
        public long? GetVoucherNo(DateTime? date1)
        {
            long? Id = 0;
            List<JournalTable> jtList = _context.JournalTables.Where(p => p.EntryDate.Value.Year == date1.Value.Year).ToList();
            if (jtList.Count == 0)
            {
                Id = 1;
            }
            else
            {
                Id = jtList.Select(p => p.VoucherNo).Max() + 1;
            }
            return Id;
        }
        private void JournalTransactions(UADbContext context, TripTable db1,
            decimal? munshiana, long? transactionId, long? voucherNo, string? lorry)
        {
            long tripmunshianaid = context.AccountTables.Where(p => p.Title == "TRIP MUNSHIANA").Select(P => P.AccountId).First();

            if (db1.TripId == 0 && munshiana != 0)
            {
                string description = string.Format("{0} Trip Munshiana Token# {1}", context.ShippingTables
                    .Where(p => p.Id == db1.DestinationId).Select(p => p.Title).FirstOrDefault(), db1.TokenNo);
                db1.JournalTables.Add(AddJournalTable(db1.EntryDate, db1.Lorry, munshiana, null, description, transactionId,
                    voucherNo, "TM", "", null, lorry, null, null, null));
                db1.JournalTables.Add(AddJournalTable(db1.EntryDate, tripmunshianaid, null, munshiana, description, transactionId,
                    voucherNo, "TM", "", null, lorry, null, null, null));
            }
            else if (db1.TripId != 0)
            {
                List<JournalTable> munshianaList = context.JournalTables
                    .Where(p => p.TripId == db1.TripId && p.EntryType == "TM").ToList();
                if (munshianaList.Count != 0)
                {
                    if (munshiana != 0)
                    {
                        JournalTable creditJt = context.JournalTables
                            .Where(p => p.TripId == db1.TripId && p.EntryType == "TM" && p.Credit != null).First();
                        JournalTable debitJt = context.JournalTables
                            .Where(p => p.TripId == db1.TripId && p.EntryType == "TM" && p.Debit != null).First();
                        string description = string.Format("{0} Trip Munshiana Token# {1}", context.ShippingTables
                            .Where(p => p.Id == db1.DestinationId).Select(p => p.Title).FirstOrDefault(), db1.TokenNo);
                        UpdateJournalTable(debitJt, db1.EntryDate, db1.Lorry, munshiana, null, description, "", null, lorry, null, null);
                        UpdateJournalTable(creditJt, db1.EntryDate, tripmunshianaid, null, munshiana, description, "", null, lorry, null, null);
                    }
                    else
                    {
                        foreach (var item in munshianaList)
                        {
                            context.JournalTables.Remove(item);
                        }
                    }
                }
                else
                {
                    if (munshiana != 0)
                    {
                        string description = string.Format("{0} Trip Munshiana Token# {1}",
                            context.ShippingTables.Where(p => p.Id == db1.DestinationId)
                            .Select(p => p.Title).FirstOrDefault(), db1.TokenNo);
                        db1.JournalTables.Add(AddJournalTable(db1.EntryDate,
                            db1.Lorry, munshiana, null, description, transactionId,
                            voucherNo, "TM", "", null, lorry, null, null, null));
                        db1.JournalTables.Add(AddJournalTable(db1.EntryDate,
                            tripmunshianaid, null, munshiana, description, transactionId,
                            voucherNo, "TM", "", null, lorry, null, null, null));
                    }
                }
            }
        }
        public static JournalTable AddJournalTable(DateTime? EntryDate, long? Lorry, decimal? Debit,
            decimal? Credit, string Description, long? transactionId, long? VoucherNo, string EntryType,
            string ReceiverName, string ChequeNo, string lorry, decimal? diesel_qty, decimal? distance,
            long? pump_transaction_id)
        {
            JournalTable j1 = new JournalTable();
            j1.EntryDate = EntryDate;
            j1.AccountId = Lorry;
            j1.Credit = Credit;
            j1.Debit = Debit;
            j1.Description = Description;
            j1.TransactionDate = DateTime.Now;
            j1.TransId = transactionId;
            j1.VoucherNo = VoucherNo;
            j1.EntryType = EntryType;
            //j1.UserID = App.userid;
            j1.ReceiverName = ReceiverName;
            j1.ChequeNo = ChequeNo;
            j1.Lorry = lorry;
            j1.Quanitity = diesel_qty;
            j1.PumpTransId = pump_transaction_id;
            return j1;
        }
        public static void UpdateJournalTable(JournalTable j1, DateTime? EntryDate, long? Lorry, decimal? Debit,
            decimal? Credit, string Description, string ReceiverName, string ChequeNo, string lorry, decimal? diesel_qty,
            decimal? distance)
        {
            j1.EntryDate = EntryDate;
            j1.AccountId = Lorry;
            j1.Credit = Credit;
            j1.Debit = Debit;
            j1.Description = Description;
            j1.TransactionDate = DateTime.Now;
            //j1.UserId = App.userid;
            j1.ReceiverName = ReceiverName;
            j1.ChequeNo = ChequeNo;
            j1.Lorry = lorry;
            j1.Quanitity = diesel_qty;
        }
        private void TripAdvaneTransactions(UADbContext context, TripTable db1, long? transactionId,
            List<JournalTable> advance_list, bool flag)
        {
            if (flag == false)
            {
                //Trip Advance
                foreach (var item in advance_list)
                {
                    JournalTable obj = new JournalTable();
                    obj.TransId = ++transactionId;
                    //obj.VoucherNo = voucher_no;
                    obj.EntryDate = item.EntryDate;
                    long? debitid = db1.Lorry;
                    obj.AccountId = debitid;
                    obj.Debit = item.Credit;
                    obj.Credit = null;
                    obj.Description = item.Description;
                    obj.ChequeNo = item.ChequeNo;
                    //obj.ReceiverName = payee_txt.Text;
                    obj.EntryType = "GV";
                    //obj.TripId = db1.TripId;
                    obj.Lorry = item.Lorry;
                    db1.JournalTables.Add(obj);

                    JournalTable obj1 = new JournalTable();
                    obj1.TransId = transactionId;
                    //obj1.VoucherNo = voucher_no;
                    obj1.EntryDate = item.EntryDate;
                    obj1.AccountId = item.AccountId;
                    obj1.Debit = null;
                    obj1.Credit = item.Credit;
                    obj1.Description = item.Description;
                    obj1.ChequeNo = item.ChequeNo;
                    //obj1.ReceiverName = payee_txt.Text;
                    obj1.EntryType = "GV";
                    obj1.TripId = db1.TripId;
                    obj.Lorry = item.Lorry;
                    db1.JournalTables.Add(obj1);
                }
            }
            else
            {
                List<long?> jt_list = context.JournalTables.Where(p => p.TripId == db1.TripId &&
                    p.EntryType == "GV").Select(p => p.TransId).AsEnumerable().Distinct().ToList();
                foreach (var item in advance_list)
                {
                    if (item.TransId != null)
                    {
                        jt_list.Remove(item.TransId);
                    }
                    else if (item.TransId == null)
                    {
                        JournalTable obj = new JournalTable();
                        obj.TransId = ++transactionId;
                        //obj.VoucherNo = voucher_no;
                        obj.EntryDate = item.EntryDate;
                        long? debitid = db1.Lorry;
                        obj.AccountId = debitid;
                        obj.Debit = item.Credit;
                        obj.Credit = null;
                        obj.Description = item.Description;
                        obj.ChequeNo = item.ChequeNo;
                        //obj.ReceiverName = payee_txt.Text;
                        obj.EntryType = "GV";
                        obj.TripId = db1.TripId;
                        obj.Lorry = item.Lorry;
                        db1.JournalTables.Add(obj);

                        JournalTable obj1 = new JournalTable();
                        obj1.TransId = transactionId;
                        //obj1.VoucherNo = voucher_no;
                        obj1.EntryDate = item.EntryDate;
                        obj1.AccountId = item.AccountId;
                        obj1.Debit = null;
                        obj1.Credit = item.Credit;
                        obj1.Description = item.Description;
                        obj1.ChequeNo = item.ChequeNo;
                        //obj1.ReceiverName = payee_txt.Text;
                        obj1.EntryType = "GV";
                        obj1.TripId = db1.TripId;
                        obj.Lorry = item.Lorry;
                        db1.JournalTables.Add(obj1);
                    }
                }
                //Delete Records Remove from advance grid
                foreach (var trans_id in jt_list)
                {
                    List<JournalTable> list1 = context.JournalTables.Where(p => p.TransId == trans_id).ToList();
                    foreach (var jt in list1)
                    {
                        context.JournalTables.Remove(jt);
                    }
                }
            }
        }
    }
}
