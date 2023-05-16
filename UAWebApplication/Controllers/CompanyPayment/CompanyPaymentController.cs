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
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize]
    public class CompanyPaymentController : Controller
    {
        private readonly UADbContext _context;
        public CompanyPaymentController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public ActionResult CompanyPayment()
        {
            return View("~/Views/CompanyPayment/CompanyPayment.cshtml");
        }

        //View Company Bill
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> Years()
        {
            YearsReturnClass obj_return = new YearsReturnClass();
            try
            {
                List<int> year_list = await _context.PsosummaryTables.Where(p => p.SummaryDate != null)
                    .Select(p => p.SummaryDate.Value.Year).Distinct().OrderByDescending(p => p).ToListAsync();
                obj_return.YearList = year_list;
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
        public class YearsReturnClass
        {
            public YearsReturnClass()
            {
                YearList = new List<int>();
            }
            public string? Message { get; set; }
            public List<int> YearList { get; set; }
        }

        public class ViewPaymentsParam
        {
            public long? Year { get; set; }
            public int? CompanyId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> ViewPayments([FromBody]ViewPaymentsParam p1)
        {
            ViewPaymentsReturn obj = new ViewPaymentsReturn();
            try
            {
                List<PsosummaryTable> list1 =await _context.PsosummaryTables.Include(p=>p.Company)
                    .Where(p => p.CompanyId == p1.CompanyId && p.SummaryDate.Value.Year == p1.Year)
                    .OrderByDescending(p => p.SummaryDate).ToListAsync();
                foreach (var item in list1)
                {
                    obj.CompanyPaymentList.Add(new CompanyPaymentsClass(item));
                }
                obj.Message = "OK";
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        obj.Message = string.Format("{0}", ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        obj.Message = string.Format("{0}", ex.InnerException.Message);
                    }
                }
                else
                {
                    obj.Message = string.Format("{0}", ex.Message);
                }
            }
            string json = JsonConvert.SerializeObject(obj);
            return Json(json);
        }
        public class ViewPaymentsReturn
        {
            public ViewPaymentsReturn() { CompanyPaymentList = new List<CompanyPaymentsClass>(); }
            public string Message { get; set; }
            public List<CompanyPaymentsClass> CompanyPaymentList { get; set; }
        }
        public class CompanyPaymentsClass :PsosummaryTable
        {
            public CompanyPaymentsClass(PsosummaryTable cpt)
            {
                this.Id = cpt.Id;
                this.MonthTitle = String.Format("{0:MMM-yyyy}", cpt.SummaryDate);
                this.CompanyId = cpt.CompanyId;
                this.SummaryDate = cpt.SummaryDate;
                this.CompanyTitle = cpt.Company.Title;
                if (cpt.IsFinalized == true)
                {
                    this.Finalized = "checked";
                }
                else
                {
                    this.Finalized = "";
                }
            }
            public string? CompanyTitle { get; set; }
            public string? MonthTitle { get; set; }
            public string? Finalized { get; set; }
        }

        //Trips
        public class ViewTripsParam
        {
            public long? PaymentId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> ViewTrips([FromBody] ViewTripsParam p1)
        {
            ViewTripsReturn obj_return = new ViewTripsReturn();
            try
            {
                PsosummaryTable? pbt = await _context.PsosummaryTables.Where(p => p.Id == p1.PaymentId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Payment not found.");
                }
                List<TripTable> billedList = await _context.TripTables.Where(p => p.SummaryId == p1.PaymentId)
                    .OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in billedList)
                {
                    obj_return.TripList.Add(new TripDto(_context, item, "checked"));
                }
                List<TripTable> unBilledList = await _context.TripTables.Where(p => p.SummaryId == null &&
                    p.PartyId == pbt.CompanyId).OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in unBilledList)
                {
                    obj_return.TripList.Add(new TripDto(_context, item, ""));
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
        public class ViewTripsReturn
        {
            public ViewTripsReturn()
            {
                TripList = new List<TripDto>();
            }
            public string Message { get; set; }
            public List<TripDto> TripList { get; set; }
        }
        public class TripDto : TripTable
        {
            public TripDto() { }
            public TripDto(UADbContext context, TripTable tt, string isBilled)
            {
                this.IsBilled = isBilled;
                this.TripId = tt.TripId;
                this.TokenNo = tt.TokenNo;
                this.Lorry = tt.Lorry;
                this.LorryTitle = context.AccountTables.Where(p => p.AccountId == tt.Lorry).Select(p => p.Title).FirstOrDefault();
                this.InvoiceDate = tt.InvoiceDate;
                this.EntryDate = tt.EntryDate;
                this.Quantity = tt.Quantity;
                this.Freight = tt.Freight;
                this.ShortQty = tt.ShortQty;
                this.ShortAmount = tt.ShortAmount;
                this.ShippingId = tt.ShippingId;
                this.ShippingTitle = context.ShippingTables.Where(p => p.Id == tt.ShippingId)
                    .Select(p => p.Title).FirstOrDefault();
                this.DestinationId = tt.DestinationId;
                this.DestinationTitle = context.DestinationTables.Where(p => p.Id == tt.DestinationId)
                    .Select(p => p.Title).FirstOrDefault();
                this.ProductId = tt.ProductId;
                this.ProductTitle = context.ProductTables.Where(p => p.Id == tt.ProductId)
                    .Select(p => p.Title).FirstOrDefault();
                this.PartyBillId = tt.PartyBillId;
                this.PartyId = tt.PartyId;

            }
            public string? IsBilled { get; set; }
            public string? ShippingTitle { get; set; }
            public string? DestinationTitle { get; set; }
            public string? ProductTitle { get; set; }
            public string? LorryTitle { get; set; }
        }

        public class SaveTripsParam
        {
            public SaveTripsParam()
            {
                TripList = new List<TripDto>();
            }
            public long? PaymentId { get; set; }
            public List<TripDto> TripList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> SaveTrips([FromBody] SaveTripsParam p1)
        {
            SaveTripsReturn obj_return = new SaveTripsReturn();
            try
            {
                PsosummaryTable? pbt = await _context.PsosummaryTables.Where(p => p.Id == p1.PaymentId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party Payment not found.");
                }
                List<TripTable> tripList = await _context.TripTables.Where(p => p.SummaryId == p1.PaymentId).ToListAsync();
                foreach (var trip in tripList)
                {
                    trip.SummaryId = null;
                }
                if (p1.TripList != null)
                {
                    foreach (var trip in p1.TripList)
                    {
                        TripTable? tt = await _context.TripTables.Where(p => p.TripId == trip.TripId).FirstOrDefaultAsync();
                        if (tt == null)
                        {
                            throw new Exception(string.Format("Oops! Trip bearing Id {0} not found", trip.TripId));
                        }
                        tt.SummaryId = pbt.Id;
                    }
                }
                await _context.SaveChangesAsync();
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
        public class SaveTripsReturn
        {
            public SaveTripsReturn()
            {
            }
            public string? Message { get; set; }
        }

        //Shortages
        public class ViewShortagesParam
        {
            public long? PaymentId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> ViewShortages([FromBody] ViewShortagesParam p1)
        {
            ViewTripsReturn obj_return = new ViewTripsReturn();
            try
            {
                PsosummaryTable? pbt = await _context.PsosummaryTables.Where(p => p.Id == p1.PaymentId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Payment not found.");
                }
                List<TripTable> billedList = await _context.TripTables.Where(p => p.SummaryShortId == p1.PaymentId)
                    .OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in billedList)
                {
                    obj_return.TripList.Add(new TripDto(_context, item, "checked"));
                }
                List<TripTable> unBilledList = await _context.TripTables.Where(p => p.SummaryShortId == null &&
                    p.PartyId == pbt.CompanyId && (p.ShortQty != null || p.ShortAmount != null) && (p.ShortQty >0 || p.ShortAmount > 0))
                    .OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in unBilledList)
                {
                    obj_return.TripList.Add(new TripDto(_context, item, ""));
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
        public class ViewShortagesReturn
        {
            public ViewShortagesReturn()
            {
                TripList = new List<TripDto>();
            }
            public string Message { get; set; }
            public List<TripDto> TripList { get; set; }
        }
        public class SaveShortagesParam
        {
            public SaveShortagesParam()
            {
                TripList = new List<TripDto>();
            }
            public long? PaymentId { get; set; }
            public List<TripDto> TripList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> SaveShortages([FromBody] SaveShortagesParam p1)
        {
            SaveShortagesReturn obj_return = new SaveShortagesReturn();
            try
            {
                PsosummaryTable? pbt = await _context.PsosummaryTables.Where(p => p.Id == p1.PaymentId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party Payment not found.");
                }
                List<TripTable> tripList = await _context.TripTables.Where(p => p.SummaryShortId == p1.PaymentId).ToListAsync();
                foreach (var trip in tripList)
                {
                    trip.SummaryShortId = null;
                }
                if (p1.TripList != null)
                {
                    foreach (var trip in p1.TripList)
                    {
                        TripTable? tt = await _context.TripTables.Where(p => p.TripId == trip.TripId).FirstOrDefaultAsync();
                        if (tt == null)
                        {
                            throw new Exception(string.Format("Oops! Trip bearing Id {0} not found", trip.TripId));
                        }
                        tt.SummaryShortId = pbt.Id;
                    }
                }
                await _context.SaveChangesAsync();
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
        public class SaveShortagesReturn
        {
            public SaveShortagesReturn()
            {
            }
            public string? Message { get; set; }
        }

        //Transactions
        public class AttachTransViewParam
        {
            public long? PaymentId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> AttachTransView([FromBody] AttachTransViewParam p1)
        {
            AttachTransViewReturn obj_return = new AttachTransViewReturn();
            try
            {
                PsosummaryTable? pbt = await _context.PsosummaryTables.Where(p => p.Id == p1.PaymentId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Payment not found.");
                }
                List<JournalTable> list1 = await _context.JournalTables.Where(p => p.SummaryId == null && p.AccountId == pbt.CompanyId &&
                    p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.TransactionList.Add(new JournalDto(_context, item,false));
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
        public class AttachTransViewReturn
        {
            public AttachTransViewReturn()
            {
                TransactionList = new List<JournalDto>();
            }
            public string Message { get; set; }
            public List<JournalDto> TransactionList { get; set; }
        }
        public class JournalDto : JournalTable
        {
            public JournalDto() { }
            public JournalDto(UADbContext context, JournalTable tt, bool? isChecked)
            {
                if (isChecked == true)
                {
                    this.IsChecked = "checked";
                }
                else
                {
                    this.IsChecked = "";
                }
                this.Id = tt.Id;
                this.TransId = tt.TransId;
                this.AccountId = tt.AccountId;
                this.AccountTitle = context.AccountTables.Where(p => p.AccountId == tt.AccountId).Select(p => p.Title).FirstOrDefault();
                this.ChequeNo = tt.ChequeNo;
                this.Credit = tt.Credit;
                this.Debit = tt.Debit;
                this.Description = tt.Description;
                this.EntryDate = tt.EntryDate;
            }
            public string? IsChecked { get; set; }
            public string? AccountTitle { get; set; }
        }
        public class AttachTransSaveParam
        {
            public AttachTransSaveParam()
            {
                TransactionList = new List<JournalDto>();
            }
            public long? PaymentId { get; set; }
            public List<JournalDto> TransactionList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> AttachTransSave([FromBody] AttachTransSaveParam p1)
        {
            AttachTransSaveReturn obj_return = new AttachTransSaveReturn();
            try
            {
                PsosummaryTable? pbt = await _context.PsosummaryTables.Where(p => p.Id == p1.PaymentId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party Payment not found.");
                }
                if (p1.TransactionList != null)
                {
                    foreach (var j in p1.TransactionList)
                    {
                        JournalTable? tt = await _context.JournalTables.Where(p => p.Id == j.Id).FirstOrDefaultAsync();
                        if (tt == null)
                        {
                            throw new Exception(string.Format("Oops! Record bearing Id {0} not found", j.Id));
                        }
                        tt.SummaryId = pbt.Id;
                    }
                }
                await _context.SaveChangesAsync();
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
        public class AttachTransSaveReturn
        {
            public AttachTransSaveReturn()
            {
            }
            public string? Message { get; set; }
        }

        public class ViewTransViewParam
        {
            public long? PaymentId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> ViewTransView([FromBody] ViewTransViewParam p1)
        {
            AttachTransViewReturn obj_return = new AttachTransViewReturn();
            try
            {
                PsosummaryTable? pbt = await _context.PsosummaryTables.Where(p => p.Id == p1.PaymentId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Payment not found.");
                }
                List<JournalTable> list1 = await _context.JournalTables
                    .Where(p => p.SummaryId == pbt.Id).OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.TransactionList.Add(new JournalDto(_context, item, true));
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
        public class ViewTransViewReturn
        {
            public ViewTransViewReturn()
            {
                TransactionList = new List<JournalDto>();
            }
            public string Message { get; set; }
            public List<JournalDto> TransactionList { get; set; }
        }
        public class ViewTransSaveParam
        {
            public ViewTransSaveParam()
            {
                TransactionList = new List<JournalDto>();
            }
            public long? PaymentId { get; set; }
            public List<JournalDto> TransactionList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_VIEW")]
        public async Task<IActionResult> ViewTransSave([FromBody] AttachTransSaveParam p1)
        {
            ViewTransSaveReturn obj_return = new ViewTransSaveReturn();
            try
            {
                PsosummaryTable? pbt = await _context.PsosummaryTables.Where(p => p.Id == p1.PaymentId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party Payment not found.");
                }
                List<JournalTable> list1 = await _context.JournalTables
                    .Where(p => p.SummaryId == pbt.Id).OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in list1)
                {
                    item.SummaryId = null;
                }
                if (p1.TransactionList != null)
                {
                    foreach (var j in p1.TransactionList)
                    {
                        JournalTable? tt = await _context.JournalTables.Where(p => p.Id == j.Id).FirstOrDefaultAsync();
                        if (tt == null)
                        {
                            throw new Exception(string.Format("Oops! Record bearing Id {0} not found", j.Id));
                        }
                        tt.SummaryId = pbt.Id;
                    }
                }
                await _context.SaveChangesAsync();
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
        public class ViewTransSaveReturn
        {
            public ViewTransSaveReturn()
            {
            }
            public string? Message { get; set; }
        }




    }
}