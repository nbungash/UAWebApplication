
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Esf;
using UAWebApplication.Data;
using UAWebApplication.Models;
using static UAWebApplication.Controllers.GeneralLedgerController;

namespace UAWebApplication.Controllers
{
    [Authorize]
    public class NewLorryBillController : Controller
    {
        private readonly UADbContext _context;
        public NewLorryBillController(UADbContext context)
        {
            _context = context;
        }

        public class ViewRecordsParam
        {
            public long? LorryId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,LORRY_BILL_VIEW")]
        public async Task<IActionResult> ViewRecords([FromBody] ViewRecordsParam p1)
        {
            ViewRecordsReturn obj_return = new ViewRecordsReturn();
            try
            {
                List<LorryTripClass> TripList = new List<LorryTripClass>();
                List<TripTable> TripListInDb = await _context.TripTables.Where(p => p.LorryBillNo == null && p.Lorry == p1.LorryId).ToListAsync();
                foreach (var item in TripListInDb)
                {
                    obj_return.TripList.Add(Convert(_context, item, false));
                }

                List<JournalClass> AdvanceList = new List<JournalClass>();
                List<JournalTable> AdvanceListInDb = await _context.JournalTables.Where(p => p.AccountId == p1.LorryId &&
                    p.LorryBillNo == null && p.TripId == null && p.Debit != null && p.Debit != 0 && p.EntryType == "GV" &&
                    p.TripId == null).ToListAsync();
                foreach (var item in AdvanceListInDb)
                {
                    obj_return.AdvanceList.Add(ConvertAdvanceClass(item, false));
                }
                List<JournalTable> CreditListInDb = await _context.JournalTables.Where(p => p.AccountId == p1.LorryId && p.LorryBillNo == null &&
                    p.TripId == null && p.Credit != null && p.Credit != 0 && p.EntryType == "GV" && p.TripId == null).ToListAsync();
                foreach (var item in CreditListInDb)
                {
                    obj_return.AdvanceList.Add(ConvertAdvanceClass(item, false));
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
        public class ViewRecordsReturn
        {
            public ViewRecordsReturn()
            {
                TripList = new List<LorryTripClass>();
                AdvanceList = new List<JournalClass>();
                SummaryList = new List<LorryBillSummaryDto>();
            }
            public string Message { get; set; }
            public List<LorryTripClass> TripList { get; set; }
            public List<JournalClass> AdvanceList { get; set; }
            public List<LorryBillSummaryDto> SummaryList { get; set; }
        }
        public class LorryTripClass
        {
            public long TripID { get; set; }
            public DateTime? EntryDate { get; set; }
            public int? TokenNo { get; set; }
            public DateTime? InvoiceDate { get; set; }
            public long? Lorry { get; set; }
            public string? Quantity { get; set; }
            public double? Rate { get; set; }
            public decimal? Freight { get; set; }
            public decimal? Commission { get; set; }
            public decimal? Tax { get; set; }
            public double? ShortQty { get; set; }
            public decimal? ShortRate { get; set; }
            public decimal? ShortAmount { get; set; }
            public decimal? TripAdvance { get; set; }
            public bool? Billed { get; set; }
            public string BilledChecked { get; set; }
            public string? ShippingTitle { get; set; }
            public string? DestinationTitle { get; set; }
            public string? ProductTitle { get; set; }
            public decimal? TripMunshiana { get; set; }
        }
        public class JournalClass
        {
            public long? Id { get; set; }
            public long? TransId { get; set; }
            public long? AccountID { get; set; }
            public DateTime? EntryDate { get; set; }
            public string? Description { get; set; }
            public decimal? Debit { get; set; }
            public decimal? Credit { get; set; }
            public bool? Billed { get; set; }
            public string BilledChecked { get; set; }
        }
        public class LorryBillSummaryDto
        {
            public string? Description { get; set; }
            public decimal? Amount { get; set; }
        }
        public static LorryTripClass Convert(UADbContext context, TripTable item, bool? billed)
        {
            LorryTripClass obj = new LorryTripClass();
            obj.TripID = item.TripId;
            obj.EntryDate = item.EntryDate;
            obj.TokenNo = item.TokenNo;
            obj.InvoiceDate = item.InvoiceDate;
            obj.Lorry = item.Lorry;
            if (item.QtyUnit == "M.TON")
            {
                obj.Quantity = string.Format("{0:f2}", item.Quantity);

            }
            else if (item.QtyUnit == "LTR")
            {
                obj.Quantity = string.Format("{0:f0}", item.Quantity);
            }
            obj.Rate = item.Rate ?? 0;
            obj.Freight = item.Freight ?? 0;
            obj.Commission = (item.Commission ?? 0) + (item.Tax ?? 0);
            obj.ShortQty = item.ShortQty;
            obj.ShortRate = item.ShortRate ?? 0;
            obj.ShortAmount = item.ShortAmount ?? 0;
            obj.ShippingTitle = context.ShippingTables.Where(p => p.Id == item.ShippingId).Select(p => p.Title).FirstOrDefault();
            obj.DestinationTitle = context.DestinationTables.Where(p => p.Id == item.DestinationId).Select(p => p.Title).FirstOrDefault();
            obj.ProductTitle = context.ProductTables.Where(p => p.Id == item.ProductId).Select(p => p.Title).FirstOrDefault();
            obj.Billed = billed;
            if (billed == true)
            {
                obj.BilledChecked = "checked";
            }
            else
            {
                obj.BilledChecked = "";
            }

            //Get trip advance amount from JournalTable
            decimal? journaladvance = context.JournalTables.Where(p => p.TripId == item.TripId && (p.EntryType == "GV" || p.EntryType == "TM") &&
                p.AccountId == item.Lorry).Sum(p => p.Debit).GetValueOrDefault(0);
            obj.TripAdvance = journaladvance;
            return obj;
        }
        public static JournalClass ConvertAdvanceClass(JournalTable item, bool? billed)
        {
            JournalClass obj = new JournalClass();
            obj.AccountID = item.AccountId;
            obj.Id = item.Id;
            obj.TransId = item.TransId;
            obj.Debit = item.Debit;
            obj.Credit = item.Credit;
            obj.Description = item.Description;
            obj.EntryDate = item.EntryDate;
            obj.Billed = billed;
            if (billed == true)
            {
                obj.BilledChecked = "checked";
            }
            else
            {
                obj.BilledChecked = "";
            }
            return obj;
        }

        public class SaveParam
        {
            public SaveParam()
            {
                TripList = new List<LorryTripClass>();
                AdvanceList = new List<JournalClass>();
            }
            public long? LorryBillId { get; set; }
            public long? LorryId { get; set; }
            public DateTime? BillDate { get; set; }
            public decimal? BillCharges { get; set; }
            public List<LorryTripClass> TripList { get; set; }
            public List<JournalClass> AdvanceList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,LORRY_BILL_VIEW")]
        public async Task<IActionResult> Save([FromBody] SaveParam p1)
        {
            ViewRecordsReturn obj_return = new ViewRecordsReturn();
            try
            {
                if (p1.LorryBillId == 0)
                {
                    LorryBillTable lbt = new LorryBillTable();
                    lbt.BillDate = p1.BillDate;
                    lbt.Lorry = p1.LorryId;
                    lbt.OwnerName = await _context.LorryTables.Where(p => p.AccountId == p1.LorryId).Select(p => p.OwnerName).FirstOrDefaultAsync();
                    lbt.BillDateString = string.Format("{0:dd-MMM-yyyy}", p1.BillDate);
                    lbt.BillCharges = p1.BillCharges;
                    //lbt.UserID = App.userid;
                    lbt.TransactionDate = DateTime.Now;

                    List<LorryTripClass> tripList = new List<LorryTripClass>();
                    foreach (var item in p1.TripList)
                    {
                        TripTable? tdt = await _context.TripTables.Where(p => p.TripId == item.TripID).FirstOrDefaultAsync();
                        if (tdt == null)
                        {
                            throw new Exception(string.Format("Trip bearing Id {0} is not found.", item.TripID));
                        }
                        lbt.TripTables.Add(tdt);
                        tripList.Add(item);
                    }
                    foreach (var item in p1.AdvanceList)
                    {
                        JournalTable jt = await _context.JournalTables.Where(p => p.Id == item.Id).FirstOrDefaultAsync();
                        if (jt == null)
                        {
                            throw new Exception(string.Format("Record bearing Id {0} is not found.", item.Id));
                        }
                        lbt.JournalTables.Add(jt);
                    }
                    NewJournalEntries(_context, lbt, tripList, p1.BillCharges);
                    _context.LorryBillTables.Add(lbt);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    LorryBillTable? lbt = await _context.LorryBillTables.Where(p => p.BillNo == p1.LorryBillId).FirstOrDefaultAsync();
                    if (lbt == null)
                    {
                        throw new Exception("Oops! Lorry Bill not found.");
                    }
                    lbt.BillDate = p1.BillDate;
                    lbt.OwnerName = await _context.LorryTables.Where(p => p.AccountId == lbt.Lorry).Select(p => p.OwnerName).FirstOrDefaultAsync();
                    lbt.BillDateString = string.Format("{0:dd-MMM-yyyy}", lbt.BillDate);
                    lbt.BillCharges = p1.BillCharges;
                    //lbt.UserID = App.userid;
                    lbt.TransactionDate = DateTime.Now;

                    //Reset Saved Trips and Records
                    List<TripTable> list1 = await _context.TripTables.Where(p => p.LorryBillNo == p1.LorryBillId).ToListAsync();
                    foreach (var item in list1)
                    {
                        item.LorryBillNo = null;
                    }
                    List<LorryTripClass> tripList = new List<LorryTripClass>();
                    foreach (var item in p1.TripList)
                    {
                        TripTable? tdt = await _context.TripTables.Where(p => p.TripId == item.TripID).FirstOrDefaultAsync();
                        if (tdt == null)
                        {
                            throw new Exception(string.Format("Oops! Trip bearing Id {0} is not foundl", item.TripID));
                        }
                        tdt.LorryBillNo = lbt.BillNo;
                        tripList.Add(item);
                    }
                    List<JournalTable> list2 = await _context.JournalTables.Where(p => p.LorryBillNo == p1.LorryBillId).ToListAsync();
                    foreach (var item in list2)
                    {
                        item.LorryBillNo = null;
                    }
                    foreach (var item in p1.AdvanceList)
                    {
                        JournalTable? jt = await _context.JournalTables.Where(p => p.Id == item.Id).FirstOrDefaultAsync();
                        if (jt == null)
                        {
                            throw new Exception(string.Format("Oops! Record bearing Id {0} is not foundl", item.Id));
                        }
                        jt.LorryBillNo = lbt.BillNo;
                    }
                    UpdateJournalEntries(_context, lbt, tripList, p1.BillCharges);
                    await _context.SaveChangesAsync();
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
            }
            public string Message { get; set; }
        }
        private void NewJournalEntries(UADbContext context, LorryBillTable lbt, List<LorryTripClass> tripList, decimal? billChargesAmount)
        {
            //Get TransactionID
            long? transID = context.JournalTables.Select(p => p.TransId).Max() + 1; ;
            long? voucherno = GetVoucherNo(_context,lbt.BillDate);

            decimal? freightAmount = 0;
            decimal? commissionAmount = 0;
            decimal? taxAmount = 0;
            decimal? shortageAmount = 0;
            decimal? tripadvance = 0;

            foreach (var item in tripList)
            {
                TripTable tt = context.TripTables.Where(p => p.TripId == item.TripID).First();
                freightAmount += (tt.Freight ?? 0);
                commissionAmount += (tt.Commission ?? 0);
                taxAmount += (tt.Tax ?? 0);
                shortageAmount += (tt.ShortAmount ?? 0);
                tripadvance += (context.JournalTables.Where(p => p.TripId == item.TripID).Sum(p => p.Debit).GetValueOrDefault(0));
            }
            decimal? freightPayableAmount = freightAmount
                - commissionAmount
                - taxAmount
                - shortageAmount
                - billChargesAmount;

            if (freightAmount != 0)
            {
                JournalTable obj = new JournalTable();
                obj.TransId = transID;
                obj.VoucherNo = voucherno;
                obj.EntryDate = lbt.BillDate;
                obj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(), " LORRY BILL FREIHJT OF ", lbt.BillDate).ToUpper();
                obj.AccountId = context.AccountTables.Where(p => p.Title == "FREIGHT").Select(p => p.AccountId).First();
                obj.Debit = freightAmount;
                obj.Credit = null;
                obj.EntryType = "LORRY BILL FREIGHT";
                //obj.UserID = App.userid;
                obj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(obj);
            }

            if (commissionAmount != 0)
            {
                JournalTable obj = new JournalTable();
                obj.TransId = transID;
                obj.VoucherNo = voucherno;
                obj.EntryDate = lbt.BillDate;
                obj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(), " LORRY BILL COMMISSION OF ", lbt.BillDate).ToUpper();
                obj.AccountId = context.AccountTables.Where(p => p.Title == "COMMISSION").Select(p => p.AccountId).First();
                obj.Debit = null;
                obj.Credit = commissionAmount;
                obj.EntryType = "LORRY BILL COMMISSION";
                //obj.UserID = App.userid;
                obj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(obj);
            }
            if (taxAmount != 0)
            {
                JournalTable obj = new JournalTable();
                obj.TransId = transID;
                obj.VoucherNo = voucherno;
                obj.EntryDate = lbt.BillDate;
                obj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(), " LORRY BILL TAX OF ", lbt.BillDate).ToUpper();
                obj.AccountId = context.AccountTables.Where(p => p.Title == "TAX").Select(p => p.AccountId).First();
                obj.Debit = null;
                obj.Credit = taxAmount;
                obj.EntryType = "LORRY BILL TAX";
                //obj.UserID = App.userid;
                obj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(obj);
            }

            if (shortageAmount != 0)
            {
                JournalTable obj = new JournalTable();
                obj.TransId = transID;
                obj.VoucherNo = voucherno;
                obj.EntryDate = lbt.BillDate;
                obj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(), " LORRY BILL SHORTAGE OF ", lbt.BillDate).ToUpper();
                obj.AccountId = context.AccountTables.Where(p => p.Title == "SHORT").Select(p => p.AccountId).First();
                obj.Debit = null;
                obj.Credit = shortageAmount;
                obj.EntryType = "LORRY BILL SHORT";
                //obj.UserID = App.userid;
                obj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(obj);
            }
            if (billChargesAmount != 0)
            {
                JournalTable obj = new JournalTable();
                obj.TransId = transID;
                obj.VoucherNo = voucherno;
                obj.EntryDate = lbt.BillDate;
                obj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(), " LORRY BILL CHARGES OF ", lbt.BillDate).ToUpper();
                obj.AccountId = context.AccountTables.Where(p => p.Title == "LORRY BILL CHARGES").Select(p => p.AccountId).First();
                obj.Debit = null;
                obj.Credit = billChargesAmount;
                obj.EntryType = "LORRY BILL LORRY BILL CHARGES";
                //obj.UserID = App.userid;
                obj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(obj);
            }

            JournalTable obj1 = new JournalTable();
            obj1.TransId = transID;
            obj1.VoucherNo = voucherno;
            obj1.EntryDate = lbt.BillDate;
            obj1.AccountId = lbt.Lorry;
            string string1 = "";
            if (freightPayableAmount >= 0)
            {
                obj1.Debit = null;
                obj1.Credit = freightPayableAmount;
                string1 = "BACHAT";
            }
            else if (freightPayableAmount < 0)
            {
                obj1.Credit = null;
                obj1.Debit = Math.Abs(freightPayableAmount.Value);
                string1 = "BANAM";
            }

            obj1.Description = string.Format("{0} LORRY BILL {1} OF {2:MMM - yyyy}", context.AccountTables
                .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(), string1, lbt.BillDate).ToUpper();
            obj1.EntryType = "LORRY BILL FREIGHT PAYABLE";
            //obj1.UserID = App.userid;
            obj1.TransactionDate = DateTime.Today;
            lbt.JournalTables.Add(obj1);

            long? transidForPayment = ++transID;
            LorryTable lt = context.LorryTables.Where(p => p.AccountId == lbt.Lorry).First();
        }
        private void UpdateJournalEntries(UADbContext context, LorryBillTable lbt, List<LorryTripClass> tripList, decimal? billChargesAmount)
        {
            //Get TransactionId & JournalId
            long? transactionId = context.JournalTables.Where(p => p.LorryBillNo == lbt.BillNo && p.EntryType == "LORRY BILL FREIGHT PAYABLE")
                .Select(p => p.TransId).FirstOrDefault();

            long? voucherno = context.JournalTables.Where(p => p.LorryBillNo == lbt.BillNo && p.EntryType == "LORRY BILL FREIGHT PAYABLE")
                .Select(p => p.VoucherNo).FirstOrDefault();

            decimal? freightAmount = 0;
            decimal? commissionAmount = 0;
            decimal? taxAmount = 0;
            decimal? shortageAmount = 0;

            foreach (var item in tripList)
            {
                TripTable tt = context.TripTables.Where(p => p.TripId == item.TripID).First();
                freightAmount += (tt.Freight ?? 0);
                commissionAmount += (tt.Commission ?? 0);
                taxAmount += (tt.Tax ?? 0);
                shortageAmount += (tt.ShortAmount ?? 0);
            }
            decimal? freightPayableAmount = freightAmount - commissionAmount - taxAmount - shortageAmount - billChargesAmount;

            //Lorry Bill Freight
            JournalTable? lbf = context.JournalTables
                .Where(p => p.LorryBillNo == lbt.BillNo && p.EntryType == "LORRY BILL FREIGHT").FirstOrDefault();
            if (lbf != null)
            {
                if (freightAmount != 0)
                {
                    lbf.EntryDate = lbt.BillDate;
                    lbf.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                        .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                        " LORRY BILL FREIGHT OF ", lbt.BillDate).ToUpper();
                    lbf.Debit = freightAmount;
                    //lbf.UserID = App.userid;
                    lbf.TransactionDate = DateTime.Today;
                }
                else
                {
                    context.JournalTables.Remove(lbf);
                }
            }
            else if (lbf == null && freightAmount != 0)
            {
                JournalTable obj = new JournalTable();
                obj.TransId = transactionId;
                obj.VoucherNo = voucherno;
                obj.EntryDate = lbt.BillDate;
                obj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                    " LORRYU BILL FREIGHT OF ", lbt.BillDate).ToUpper();
                obj.AccountId = context.AccountTables.Where(p => p.Title == "FREIGHT").Select(p => p.AccountId).First();
                obj.Debit = freightAmount;
                obj.Credit = null;
                obj.EntryType = "LORRY BILL FREIGHT";
                //obj.UserID = App.userid;
                obj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(obj);
            }

            JournalTable? lbc = context.JournalTables
                .Where(p => p.LorryBillNo == lbt.BillNo && p.EntryType == "LORRY BILL COMMISSION").FirstOrDefault();
            if (lbc != null)
            {
                if (commissionAmount != 0)
                {
                    lbc.EntryDate = lbt.BillDate;
                    lbc.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                            .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                            " LORRY BILL COMMISSION OF ", lbt.BillDate).ToUpper();
                    lbc.Credit = commissionAmount;
                    //lbc.UserID = App.userid;
                    lbc.TransactionDate = DateTime.Today;
                }
                else
                {
                    context.JournalTables.Remove(lbc);
                }
            }
            else if (lbc == null && commissionAmount != 0)
            {
                JournalTable commissionObj = new JournalTable();
                commissionObj.TransId = transactionId;
                commissionObj.VoucherNo = voucherno;
                commissionObj.EntryDate = lbt.BillDate;
                commissionObj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                    " LORRY BILL COMMISSION OF ", lbt.BillDate).ToUpper();
                commissionObj.AccountId = context.AccountTables.Where(p => p.Title == "COMMISSION").Select(p => p.AccountId).First();
                commissionObj.Debit = null;
                commissionObj.Credit = commissionAmount;
                commissionObj.EntryType = "LORRY BILL COMMISSION";
                //commissionObj.UserID = App.userid;
                commissionObj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(commissionObj);
            }

            JournalTable? lbt1 = context.JournalTables
                .Where(p => p.LorryBillNo == lbt.BillNo && p.EntryType == "LORRY BILL TAX").FirstOrDefault();
            if (lbt1 != null)
            {
                if (taxAmount != 0)
                {
                    lbt1.EntryDate = lbt.BillDate;
                    lbt1.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                            .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                            " LORRY BILL TAX OF ", lbt.BillDate).ToUpper();
                    lbt1.Credit = taxAmount;
                    //lbt1.UserID = App.userid;
                    lbt1.TransactionDate = DateTime.Today;
                }
                else
                {
                    context.JournalTables.Remove(lbt1);
                }
            }
            else if (lbt1 == null && taxAmount != 0)
            {
                JournalTable commissionObj = new JournalTable();
                commissionObj.TransId = transactionId;
                commissionObj.VoucherNo = voucherno;
                commissionObj.EntryDate = lbt.BillDate;
                commissionObj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                    " LORRY BILL TAX OF ", lbt.BillDate).ToUpper();
                commissionObj.AccountId = context.AccountTables.Where(p => p.Title == "TAX").Select(p => p.AccountId).First();
                commissionObj.Debit = null;
                commissionObj.Credit = taxAmount;
                commissionObj.EntryType = "LORRY BILL TAX";
                //commissionObj.UserID = App.userid;
                commissionObj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(commissionObj);
            }


            JournalTable? lbs = context.JournalTables
                .Where(p => p.LorryBillNo == lbt.BillNo && p.EntryType == "LORRY BILL SHORT").FirstOrDefault();
            if (lbs != null)
            {
                if (shortageAmount != 0)
                {
                    lbs.EntryDate = lbt.BillDate;
                    lbs.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                    " LORRY BILL SHORTAGE OF ", lbt.BillDate).ToUpper();
                    lbs.Credit = shortageAmount;
                    //lbs.UserID = App.userid;
                    lbs.TransactionDate = DateTime.Now;
                }
                else
                {
                    context.JournalTables.Remove(lbs);
                }
            }
            else if (lbs == null && shortageAmount != 0)
            {
                JournalTable shortageObj = new JournalTable();
                shortageObj.TransId = transactionId;
                shortageObj.VoucherNo = voucherno;
                shortageObj.EntryDate = lbt.BillDate;
                shortageObj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                    " LORRY BILL SHORTAGE OF ", lbt.BillDate).ToUpper();
                shortageObj.AccountId = context.AccountTables.Where(p => p.Title == "SHORT").Select(p => p.AccountId).First();
                shortageObj.Debit = null;
                shortageObj.Credit = shortageAmount;
                shortageObj.EntryType = "LORRY BILL SHORT";
                //shortageObj.UserID = App.userid;
                shortageObj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(shortageObj);
            }

            JournalTable? lbbca = context.JournalTables
                .Where(p => p.LorryBillNo == lbt.BillNo && p.EntryType == "LORRY BILL LORRY BILL CHARGES").FirstOrDefault();
            if (lbbca != null)
            {
                if (billChargesAmount != 0)
                {
                    lbbca.EntryDate = lbt.BillDate;
                    lbbca.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                    " LORRY BILL CHARGES OF ", lbt.BillDate).ToUpper();
                    lbbca.Credit = billChargesAmount;
                    //lbbca.UserID = App.userid;
                    lbbca.TransactionDate = DateTime.Today;
                }
                else
                {
                    context.JournalTables.Remove(lbbca);
                }
            }
            else if (lbbca == null && billChargesAmount != 0)
            {
                JournalTable lorryBillChargesObj = new JournalTable();
                lorryBillChargesObj.TransId = transactionId;
                lorryBillChargesObj.VoucherNo = voucherno;
                lorryBillChargesObj.EntryDate = lbt.BillDate;
                lorryBillChargesObj.Description = string.Format("{0} {1} {2:MMM-yyyy}", context.AccountTables
                    .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(),
                    " LORRY BILL CHARGES OF ", lbt.BillDate).ToUpper();
                lorryBillChargesObj.AccountId = context.AccountTables.Where(p => p.Title == "LORRY BILL CHARGES").Select(p => p.AccountId).First();
                lorryBillChargesObj.Debit = null;
                lorryBillChargesObj.Credit = billChargesAmount;
                lorryBillChargesObj.EntryType = "LORRY BILL LORRY BILL CHARGES";
                //lorryBillChargesObj.UserID = App.userid;
                lorryBillChargesObj.TransactionDate = DateTime.Today;
                lbt.JournalTables.Add(lorryBillChargesObj);
            }


            JournalTable freightPayableObj = context.JournalTables
                .Where(p => p.LorryBillNo == lbt.BillNo && p.EntryType == "LORRY BILL FREIGHT PAYABLE").FirstOrDefault();
            freightPayableObj.EntryDate = lbt.BillDate;
            freightPayableObj.AccountId = lbt.Lorry;

            string string1 = "";
            if (freightPayableAmount >= 0)
            {
                freightPayableObj.Debit = null;
                freightPayableObj.Credit = freightPayableAmount;
                string1 = "BACHAT";
            }
            else if (freightPayableAmount < 0)
            {
                freightPayableObj.Credit = null;
                freightPayableObj.Debit = Math.Abs(freightPayableAmount.Value);
                string1 = "BANAM";
            }

            freightPayableObj.Description =
                string.Format("{0} LORRY BILL {1} OF {2:MMM - yyyy}", context.AccountTables
                .Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault(), string1, lbt.BillDate).ToUpper();
            freightPayableObj.EntryType = "LORRY BILL FREIGHT PAYABLE";
            //freightPayableObj.UserID = App.userid;
            freightPayableObj.TransactionDate = DateTime.Today;
            lbt.JournalTables.Add(freightPayableObj);

        }
        public long? GetVoucherNo(UADbContext context, DateTime? date1)
        {
            long? Id = 0;
            List<JournalTable> jtList = context.JournalTables.Where(p => p.EntryDate.Value.Year == date1.Value.Year).ToList();
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

        public class WindowLoadedParam
        {
            public long? LorryBillId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,LORRY_BILL_VIEW")]
        public async Task<IActionResult> WindowLoaded([FromBody] WindowLoadedParam p1)
        {
            WindowLoadedReturn obj_return = new WindowLoadedReturn();
            try
            {
                LorryBillTable? lbt = await _context.LorryBillTables.Where(p => p.BillNo == p1.LorryBillId).FirstOrDefaultAsync();
                if (lbt == null)
                {
                    throw new Exception("Oops! Lorry Bill not found.");
                }
                obj_return.BillDate = lbt.BillDate;
                obj_return.LorryId = lbt.Lorry;
                obj_return.BillCharges = lbt.BillCharges;
                //Trips
                var billedtrips = await _context.TripTables.Where(p => p.LorryBillNo == p1.LorryBillId).OrderBy(P => P.InvoiceDate).ToListAsync();
                foreach (var item in billedtrips)
                {
                    obj_return.TripList.Add(Convert(_context, item, true));
                }
                var unbilledtrips = await _context.TripTables.Where(p => p.LorryBillNo == null && p.Lorry == lbt.Lorry)
                    .OrderBy(p => p.InvoiceDate).ToListAsync();
                foreach (var item in unbilledtrips)
                {
                    obj_return.TripList.Add(Convert(_context, item, false));
                }

                //Advance
                List<JournalTable> billedAdvanceRecords = await _context.JournalTables.Where(p => p.AccountId == lbt.Lorry &&
                    p.LorryBillNo == lbt.BillNo && (p.EntryType == "PV" || p.EntryType == "RV" || p.EntryType == "GV") &&
                    p.TripId == null).ToListAsync();
                foreach (var item in billedAdvanceRecords)
                {
                    obj_return.AdvanceList.Add(ConvertAdvanceClass(item, true));

                }
                List<JournalTable> unbilledAdvanceRecords = await _context.JournalTables.Where(p => p.AccountId == lbt.Lorry &&
                    p.LorryBillNo == null && (p.EntryType == "PV" || p.EntryType == "RV" || p.EntryType == "GV") &&
                    p.TripId == null).ToListAsync();
                foreach (var item in unbilledAdvanceRecords)
                {
                    obj_return.AdvanceList.Add(ConvertAdvanceClass(item, false));
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
        public class WindowLoadedReturn
        {
            public WindowLoadedReturn()
            {
                TripList = new List<LorryTripClass>();
                AdvanceList = new List<JournalClass>();
            }
            public string? Message { get; set; }
            public long? LorryId { get; set; }
            public decimal? BillCharges { get; set; }
            public DateTime? BillDate { get; set; }
            public List<LorryTripClass> TripList { get; set; }
            public List<JournalClass> AdvanceList { get; set; }
        }
    }
}