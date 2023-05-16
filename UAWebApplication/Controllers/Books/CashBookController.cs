
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CASH_BOOK_VIEW")]
    public class CashBookController : Controller
    {
        private readonly UADbContext _context;
        public CashBookController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CASH_BOOK_VIEW")]
        public ActionResult CashBook()
        {
            return View("~/Views/Books/CashBook.cshtml");
        }

        public class SearchByEntryDateParam
        {
            public DateTime? SearchDate { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CASH_BOOK_VIEW")]
        public async Task<IActionResult> SearchByEntryDate([FromBody]SearchByEntryDateParam p1)
        {
            SearchByEntryDateReturnClass obj_return = new SearchByEntryDateReturnClass();
            try
            {
                List<CashBookDto> db_credit_list = new List<CashBookDto>();
                AccountTable cash_in_hand_account = await _context.AccountTables.Where(p => p.Title == "CASH IN HAND").FirstAsync();

                //Cash brought forward
                CashBookDto bf_amount = new CashBookDto();
                bf_amount.Description = "Cash B/F : ";
                bf_amount.Type = "AMDAN";
                decimal? total_debit_cash =await _context.JournalTables.Where(p => p.AccountId == cash_in_hand_account.AccountId &&
                    p.EntryDate < p1.SearchDate).SumAsync(p => p.Debit);
                decimal? total_credit_cash =await _context.JournalTables.Where(p => p.AccountId == cash_in_hand_account.AccountId &&
                    p.EntryDate < p1.SearchDate).SumAsync(p => p.Credit);
                bf_amount.Amount = total_debit_cash - total_credit_cash;
                db_credit_list.Add(bf_amount);

                List<CashBookDto> db_debit_list = new List<CashBookDto>();
                List<JournalTable> j_list = await _context.JournalTables.Include(p=>p.Account)
                        .Where(p => p.EntryDate == p1.SearchDate).OrderBy(p => p.TransId).ToListAsync();
                foreach (var item in j_list)
                {
                    if (item.AccountId != cash_in_hand_account.AccountId)
                    {
                        CashBookDto obj = new CashBookDto();
                        obj.EntryType = item.EntryType;
                        obj.TransId = item.TransId;
                        obj.AccountTitle = item.Account.Title;
                        obj.ChequeNo = item.ChequeNo;
                        obj.Description = item.Description;
                        obj.TripId = item.TripId;
                        if (item.Credit != null)
                        {
                            obj.Type = "AMDAN";
                            obj.Amount = item.Credit.Value;
                            db_credit_list.Add(obj);
                        }
                        else if (item.Debit != null)
                        {
                            obj.Type = "KHARCH";
                            obj.Amount = item.Debit.Value;
                            db_debit_list.Add(obj);
                        }
                    }
                }

                //Total Kharch
                decimal? total_kharch = db_debit_list.Sum(p => p.Amount);
                CashBookDto total_debit_obj = new CashBookDto();
                total_debit_obj.Description = "Total Kharch  ";
                total_debit_obj.Type = "KHARCH";
                total_debit_obj.Amount = total_kharch;
                db_debit_list.Add(total_debit_obj);

                //Total Amdan
                decimal? total_amdan = db_credit_list.Sum(p => p.Amount);
                CashBookDto total_credit_obj = new CashBookDto();
                total_credit_obj.Description = "Total Amdan : ";
                total_credit_obj.Type = "AMDAN";
                total_credit_obj.Amount = total_amdan;
                db_credit_list.Add(total_credit_obj);

                //Total Kharch
                CashBookDto total_debit2_obj = new CashBookDto();
                total_debit2_obj.Description = "Total Kharch : ";
                total_debit2_obj.Type = "AMDAN";
                total_debit2_obj.Amount = total_kharch;
                db_credit_list.Add(total_debit2_obj);

                //Cash Carry Forward
                CashBookDto cf_obj = new CashBookDto();
                cf_obj.Description = "Cash C/F : ";
                cf_obj.Type = "AMDAN";
                cf_obj.Amount = total_amdan - total_kharch;
                db_credit_list.Add(cf_obj);

                obj_return.DbList.AddRange(db_credit_list);
                obj_return.DbList.AddRange(db_debit_list);
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
        public class SearchByEntryDateReturnClass
        {
            public SearchByEntryDateReturnClass()
            {
                DbList = new List<CashBookDto>();
            }
            public string? Message { get; set; }
            public List<CashBookDto> DbList { get; set; }
        }
        public class CashBookDto
        {
            public string? Type { get; set; }
            public long? TransId { get; set; }
            public long? TripId { get; set; }
            public string? EntryType { get; set; }
            public string? AccountTitle { get; set; }
            public string? Description { get; set; }
            public string? ChequeNo { get; set; }
            public decimal? Amount { get; set; }
        }

    }
}
