
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRANSACTION_VIEW")]
    public class NewTransactionController : Controller
    {
        private readonly UADbContext _context;
        public NewTransactionController(UADbContext context)
        {
            _context = context;
        }

        public class NewTransactionWindowLoadedParam
        {
            public long TransactionId { get; set; }

        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRANSACTION_VIEW")]
        public async Task<IActionResult> NewTransactionWindowLoaded([FromBody] NewTransactionWindowLoadedParam p1)
        {
            NewTransactionWindowLoadedReturn obj_return = new NewTransactionWindowLoadedReturn();
            try
            {
                List<JournalTable> list1 = await _context.JournalTables.Include(p=>p.Account)
                    .Where(p => p.TransId == p1.TransactionId && p.EntryType=="GV").ToListAsync();
                if (list1.Count == 0)
                {
                    throw new Exception("Oops! No Record to Display");
                }
                JournalTable debitRecord = list1.Where(p => p.Debit != null).First();
                JournalTable creditRecord = list1.Where(p => p.Credit != null).First();
                obj_return.TransactionObj.VoucherNo = debitRecord.VoucherNo;
                obj_return.TransactionObj.ChequeNo = creditRecord.ChequeNo;
                obj_return.TransactionObj.EntryDate = debitRecord.EntryDate;
                obj_return.TransactionObj.CreditAccountId = creditRecord.AccountId;
                obj_return.TransactionObj.DebitAccountId = debitRecord.AccountId;
                obj_return.TransactionObj.TransId= debitRecord.TransId;
                obj_return.TransactionObj.DebitAccountGroup = debitRecord.Account.GroupType;
                obj_return.TransactionObj.CreditAccountGroup = creditRecord.Account.GroupType;
                obj_return.TransactionObj.Amount = debitRecord.Debit;
                obj_return.TransactionObj.Description = debitRecord.Description;
                obj_return.TransactionObj.ReceiverName = debitRecord.ReceiverName;
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
        public class NewTransactionWindowLoadedReturn
        {
            public NewTransactionWindowLoadedReturn()
            {
                TransactionObj = new NewTransactionDto();
            }
            public string? Message { get; set; }
            public NewTransactionDto? TransactionObj { get; set; }
        }
        public class NewTransactionDto
        {
            public long? TransId { get; set; }
            public long? DebitAccountId { get; set; }
            public long? CreditAccountId { get; set; }
            public string? CreditAccountGroup { get; set; }
            public string? DebitAccountGroup { get; set; }
            public DateTime? EntryDate { get; set; }
            public string? ReceiverName { get; set; }
            public string? Description { get; set; }
            public long? VoucherNo { get; set; }
            public string? ChequeNo { get; set; }
            public decimal? Amount { get; set; }
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRANSACTION_ADD")]
        public async Task<IActionResult> Save([FromBody] NewTransactionDto p1)
        {
            SaveReturn obj_return = new SaveReturn();
            try
            {
                if (p1.TransId == 0)
                {
                    long? transactionId = await _context.JournalTables.MaxAsync(p => p.TransId)+1;

                    JournalTable obj = new JournalTable();
                    obj.TransId = transactionId;
                    obj.VoucherNo = p1.VoucherNo;
                    obj.EntryDate =p1.EntryDate;
                    obj.AccountId =p1.DebitAccountId;
                    obj.Debit =p1.Amount;
                    obj.Credit = null;
                    obj.Description =p1.Description;
                    obj.ChequeNo =p1.ChequeNo;
                    obj.ReceiverName =p1.ReceiverName;
                    obj.EntryType = "GV";
                    _context.JournalTables.Add(obj);

                    JournalTable obj1 = new JournalTable();
                    obj1.TransId = transactionId;
                    obj1.VoucherNo =p1.VoucherNo;
                    obj1.EntryDate =p1.EntryDate;
                    obj1.AccountId = p1.CreditAccountId;
                    obj1.Debit = null;
                    obj1.Credit =p1.Amount;
                    obj1.Description =p1.Description;
                    obj1.ChequeNo =p1.ChequeNo;
                    obj1.ReceiverName =p1.ReceiverName;
                    obj1.EntryType = "GV";
                    _context.JournalTables.Add(obj1);
                    
                }
                else
                {
                    JournalTable? obj =await _context.JournalTables.Where(p => p.TransId == p1.TransId &&
                        p.Debit != null).FirstOrDefaultAsync();
                    if (obj == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    obj.EntryDate =p1.EntryDate;
                    obj.AccountId =p1.DebitAccountId;
                    obj.Debit =p1.Amount;
                    obj.Credit = null;
                    obj.Description =p1.Description;
                    obj.VoucherNo =p1.VoucherNo;
                    obj.ChequeNo =p1.ChequeNo;
                    obj.ReceiverName =p1.ReceiverName;
                    obj.EntryType = "GV";

                    JournalTable? obj1 =await _context.JournalTables.Where(p => p.TransId == p1.TransId &&
                        p.Credit != null).FirstOrDefaultAsync();
                    if (obj1 == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    obj1.EntryDate =p1.EntryDate;
                    obj1.AccountId =p1.CreditAccountId;
                    obj1.Debit = null;
                    obj1.Credit =p1.Amount;
                    obj1.Description =p1.Description;
                    obj1.VoucherNo =p1.VoucherNo;
                    obj1.ChequeNo =p1.ChequeNo;
                    obj1.ReceiverName =p1.ReceiverName;
                    obj1.EntryType = "GV";
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
        public class SaveReturn
        {
            public SaveReturn()
            {
                //AccountList = new List<AccountDto>();
            }
            public string Message { get; set; }
            //public List<AccountDto> AccountList { get; set; }
        }
    }
}
