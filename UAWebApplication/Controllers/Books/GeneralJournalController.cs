
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;
using static UAWebApplication.Controllers.ChartOfAccountController;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,GENERAL_JOURNAL_VIEW")]
    public class GeneralJournalController : Controller
    {
        private readonly UADbContext _context;
        public GeneralJournalController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,GENERAL_JOURNAL_VIEW")]
        public ActionResult GeneralJournal()
        {
            return View("~/Views/Books/GeneralJournal.cshtml");
        }

        //View
        public class FilterByDateRangeParam
        {
            public string? DateFilter { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public async Task<IActionResult> FilterByDateRange([FromBody] FilterByDateRangeParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<JournalTable> list1 = new List<JournalTable>();
                if (p1.DateFilter == "1")
                {
                    list1 = await _context.JournalTables.Where(p => p.EntryDate >= p1.FromDate &&
                        p.EntryDate <= p1.ToDate).OrderBy(p => p.EntryDate).ToListAsync();
                }
                else
                {
                    list1 = await _context.JournalTables.Where(p => p.TransactionDate >= p1.FromDate &&
                        p.TransactionDate <= p1.ToDate).OrderBy(p => p.EntryDate).ToListAsync();
                }
                foreach (var item in list1)
                {
                    obj_return.JournalList.Add(new JournalDto(item,_context));
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
        
        public class FindByAccountParam
        {
            public long? AccountId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public async Task<IActionResult> FindByAccount([FromBody] FindByAccountParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<JournalTable> list1 = await _context.JournalTables.Where(p =>p.AccountId==p1.AccountId &&
                    p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.JournalList.Add(new JournalDto(item, _context));
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

        public class FindByTransactionIdParam
        {
            public long? TransactionId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public async Task<IActionResult> FindByTransactionId([FromBody] FindByTransactionIdParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<JournalTable> list1 = await _context.JournalTables
                    .Where(p => p.TransId == p1.TransactionId).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.JournalList.Add(new JournalDto(item, _context));
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

        public class FindByVoucherNoParam
        {
            public long? VoucherNo { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public async Task<IActionResult> FindByVoucherNo([FromBody] FindByVoucherNoParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<JournalTable> list1 = await _context.JournalTables
                    .Where(p => p.VoucherNo == p1.VoucherNo).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.JournalList.Add(new JournalDto(item, _context));
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

        public class FindByChequeNoParam
        {
            public string ChequeNo { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public async Task<IActionResult> FindByChequeNo([FromBody] FindByChequeNoParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<JournalTable> list1 = await _context.JournalTables
                    .Where(p => p.ChequeNo == p1.ChequeNo).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.JournalList.Add(new JournalDto(item, _context));
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

        public class FindByTripIdParam
        {
            public long? TripId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public async Task<IActionResult> FindByTripId([FromBody] FindByTripIdParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<JournalTable> list1 = await _context.JournalTables
                    .Where(p => p.TripId == p1.TripId).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.JournalList.Add(new JournalDto(item, _context));
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
        public class FilterByDateRangeReturn
        {
            public FilterByDateRangeReturn()
            {
                JournalList = new List<JournalDto>();
            }
            public string Message { get; set; }
            public List<JournalDto> JournalList { get; set; }
        }
        public class JournalDto : JournalTable
        {
            public JournalDto() { }
            public JournalDto(JournalTable jt,UADbContext context)
            {
                this.Id = jt.Id;
                this.TripId = jt.TripId;
                this.TransId = jt.TransId;
                this.EntryDate = jt.EntryDate;
                this.AccountId = jt.AccountId;
                this.Credit = jt.Credit;
                this.Debit = jt.Debit;
                this.EntryType = jt.EntryType;
                this.Description = jt.Description;
                this.ChequeNo = jt.ChequeNo;
                this.AccountTitle = context.AccountTables.Where(p => p.AccountId == jt.AccountId)
                    .Select(p => p.Title).FirstOrDefault();
            }
            public string AccountTitle { get; set; }
        }

        //Delete
        public class DeleteTransactionParam
        {
            public long? TransactionId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_DELETE")]
        public async Task<IActionResult> DeleteTransaction([FromBody] DeleteTransactionParam p1)
        {
            DeleteTransactionReturn obj_return = new DeleteTransactionReturn();
            try
            {
                List<JournalTable> list1 = await _context.JournalTables.Where(p => p.TransId == p1.TransactionId).ToListAsync();
                foreach (var obj in list1)
                {
                    if(obj.LorryBillNo != null)
                    {
                        throw new Exception("Oops! Record is included in Lorry Bill.");
                    }
                    _context.JournalTables.Remove(obj);
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
        public class DeleteTransactionReturn
        {
            public DeleteTransactionReturn()
            {
            }
            public string Message { get; set; }
        }

    }
}
