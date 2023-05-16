
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
    public class ChartOfAccountController : Controller
    {
        private readonly UADbContext _context;
        public ChartOfAccountController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public ActionResult ChartOfAccount()
        {
            return View("~/Views/Books/ChartOfAccount.cshtml");
        }

        //View
        public async Task<IActionResult> AccountGroupList()
        {
            AccountGroupListReturn obj_return = new AccountGroupListReturn();
            try
            {
                obj_return.GroupList =await _context.AccountTables.Select(p => p.GroupType).Distinct()
                    .OrderBy(p => p).ToListAsync();
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
        public class AccountGroupListReturn
        {
            public AccountGroupListReturn()
            {
                GroupList = new List<string>();
            }
            public string? Message { get; set; }
            public List<string> GroupList { get; set; }
        }
        public class AccountsByGroupListParam
        {
            public string? GroupType { get; set; }
            public bool? IsActive { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public async Task<IActionResult> AccountsByGroupList([FromBody] AccountsByGroupListParam p1)
        {
            AccountsByGroupListReturn obj_return = new AccountsByGroupListReturn();
            try
            {
                List<AccountTable> list1 = new List<AccountTable>();
                if (p1.GroupType == "All")
                {
                    list1 =await _context.AccountTables.Where(p => p.Record == p1.IsActive)
                        .OrderBy(p => p.Title).ToListAsync();
                }
                else
                {
                    list1 =await _context.AccountTables.Where(p => p.GroupType == p1.GroupType &&
                        p.Record == p1.IsActive).OrderBy(p => p.Title).ToListAsync();
                }
                foreach (var item in list1)
                {
                    obj_return.AccountList.Add(new AccountDto(item));
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

        public class SearchAccountsByTitleParam
        {
            public string? Title { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public async Task<IActionResult> SearchAccountsByTitle([FromBody] SearchAccountsByTitleParam p1)
        {
            AccountsByGroupListReturn obj_return = new AccountsByGroupListReturn();
            try
            {
                List<AccountTable> list1 = await _context.AccountTables.Where(p => p.Title.Contains(p1.Title))
                        .OrderBy(p => p.Title).ToListAsync();
                
                foreach (var item in list1)
                {
                    obj_return.AccountList.Add(new AccountDto(item));
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
        public class AccountsByGroupListReturn
        {
            public AccountsByGroupListReturn()
            {
                AccountList = new List<AccountDto>();
            }
            public string Message { get; set; }
            public List<AccountDto> AccountList { get; set; }
        }
        public class AccountDto:AccountTable
        {
            public AccountDto() { }
            public AccountDto(AccountTable p1) 
            {
                this.AccountId = p1.AccountId;
                this.AccountType = p1.AccountType;
                this.GroupType = p1.GroupType;
                this.Marked = p1.Marked;
                this.Record = p1.Record;
                this.Title = p1.Title;
                this.TitleUrdu = p1.TitleUrdu;
                this.TransactionDate = p1.TransactionDate;
                
            }
        }
        
        //Delete
        public class DeleteParam
        {
            public long? Id { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_DELETE")]
        public async Task<IActionResult> Delete([FromBody] DeleteParam p1)
        {
            DeleteReturn obj_return=new DeleteReturn();
            try
            {
                AccountTable? objToDelete =await _context.AccountTables.Where(p => p.AccountId == p1.Id)
                    .FirstOrDefaultAsync();
                if (objToDelete == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                _context.AccountTables.Remove(objToDelete);
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
        public class DeleteReturn
        {
            public DeleteReturn()
            {
            }
            public string Message { get; set; }
        }

        //Save
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_ADD")]
        public async Task<IActionResult> Save([FromBody] AccountDto p1)
        {
            SaveReturn obj_return = new SaveReturn();
            try
            {
                if (p1.AccountId == 0)
                {
                    AccountTable obj = new AccountTable();
                    obj.Title = p1.Title;
                    obj.TitleUrdu = p1.TitleUrdu;
                    obj.AccountType= p1.AccountType;
                    obj.GroupType = p1.GroupType;
                    obj.Record = false;
                    _context.AccountTables.Add(obj); 
                    await _context.SaveChangesAsync();
                    obj_return.AccountList.Add(new AccountDto(obj));
                }
                else
                {
                    AccountTable? objToUpdate = await _context.AccountTables.Where(p => p.AccountId == p1.AccountId)
                        .FirstOrDefaultAsync();
                    if (objToUpdate == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    objToUpdate.Title = p1.Title;
                    objToUpdate.TitleUrdu = p1.TitleUrdu;
                    objToUpdate.AccountType = p1.AccountType;
                    objToUpdate.GroupType = p1.GroupType;
                    objToUpdate.Record = p1.Record;
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
                AccountList = new List<AccountDto>();
            }
            public string Message { get; set; }
            public List<AccountDto> AccountList { get; set; }
        }

        //Window Loaded
        public class WindowLoadedParam
        {
            public WindowLoadedParam()
            {
            }
            public long? AccountId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_UPDATE")]
        public async Task<IActionResult> WindowLoaded([FromBody] WindowLoadedParam p1)
        {
            WindowLoadedReturn obj_return = new WindowLoadedReturn();
            try
            {
                AccountTable? obj = await _context.AccountTables.Where(p => p.AccountId == p1.AccountId)
                    .FirstOrDefaultAsync();
                if (obj == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.ObjToUpdate = new AccountDto(obj);
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
                ObjToUpdate = new AccountDto();
            }
            public string? Message { get; set; }
            public AccountDto ObjToUpdate { get; set; }
        }

        //Lorry Info
        public class LorryInfoWindowLoadedParam
        {
            public LorryInfoWindowLoadedParam()
            {
            }
            public long? AccountId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_UPDATE")]
        public async Task<IActionResult> LorryInfoWindowLoaded([FromBody] LorryInfoWindowLoadedParam p1)
        {
            LorryInfoWindowLoadedReturn obj_return = new LorryInfoWindowLoadedReturn();
            try
            {
                LorryTable? obj = await _context.LorryTables.Where(p => p.AccountId == p1.AccountId)
                    .FirstOrDefaultAsync();
                if (obj == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.ObjToUpdate = obj;
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
        public class LorryInfoWindowLoadedReturn
        {
            public LorryInfoWindowLoadedReturn()
            {
                ObjToUpdate = new LorryTable();
            }
            public string? Message { get; set; }
            public LorryTable ObjToUpdate { get; set; }
        }
        
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_ADD")]
        public async Task<IActionResult> SaveLorryInfo([FromBody] LorryTable p1)
        {
            SaveLorryInfoReturn obj_return = new SaveLorryInfoReturn();
            try
            {
                LorryTable? objToUpdate = await _context.LorryTables.Where(p => p.AccountId == p1.AccountId)
                       .FirstOrDefaultAsync();
                bool newFlag = false;
                if (objToUpdate == null)
                {
                    objToUpdate = new LorryTable();
                    newFlag = true;
                }
                objToUpdate.Capacity = p1.Capacity;
                objToUpdate.ChassisNo = p1.ChassisNo;
                objToUpdate.DipChartDueDate = p1.DipChartDueDate;
                objToUpdate.EngineNo = p1.EngineNo;
                objToUpdate.Make = p1.Make;
                objToUpdate.Model = p1.Model;
                objToUpdate.OwnerName = p1.OwnerName;
                objToUpdate.TokenDueDate = p1.TokenDueDate;
                objToUpdate.TrackerDueDate = p1.TrackerDueDate;
                if (newFlag == true)
                {
                    objToUpdate.AccountId = p1.AccountId;
                    _context.LorryTables.Add(objToUpdate);
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
        public class SaveLorryInfoReturn
        {
            public SaveLorryInfoReturn()
            {
            }
            public string Message { get; set; }
        }

        //Contact
        public class ViewContactWindowLoadedParam
        {
            public ViewContactWindowLoadedParam()
            {
            }
            public long? AccountId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_UPDATE")]
        public async Task<IActionResult> ViewContactWindowLoaded([FromBody] ViewContactWindowLoadedParam p1)
        {
            ViewContactWindowLoadedReturn obj_return = new ViewContactWindowLoadedReturn();
            try
            {
                obj_return.ContactList = await _context.AccountContactTables
                    .Where(p => p.AccountId == p1.AccountId).ToListAsync();
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
        public class ViewContactWindowLoadedReturn
        {
            public ViewContactWindowLoadedReturn()
            {
                ContactList = new List<AccountContactTable>();
            }
            public string? Message { get; set; }
            public List<AccountContactTable> ContactList { get; set; }
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_ADD")]
        public async Task<IActionResult> AddContact([FromBody] AccountContactTable p1)
        {
            AddContactReturn obj_return = new AddContactReturn();
            try
            {
                if (p1.AccountContactId == 0)
                {
                    AccountContactTable obj = new AccountContactTable();
                    obj.AccountId = p1.AccountId;
                    obj.ContactNo = p1.ContactNo;
                    obj.Name = p1.Name;
                    _context.AccountContactTables.Add(obj);
                }
                else
                {
                    AccountContactTable? objToUpdate = await _context.AccountContactTables
                        .Where(p => p.AccountContactId == p1.AccountContactId).FirstOrDefaultAsync();
                    if (objToUpdate == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    objToUpdate.AccountId = p1.AccountId;
                    objToUpdate.ContactNo = p1.ContactNo;
                    objToUpdate.Name = p1.Name;
                    await _context.SaveChangesAsync();
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
        public class AddContactReturn
        {
            public AddContactReturn()
            {
            }
            public string Message { get; set; }
        }

        public class DeleteContactParam
        {
            public long? ContactId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_DELETE")]
        public async Task<IActionResult> DeleteContact([FromBody] DeleteContactParam p1)
        {
            DeleteContactReturn obj_return = new DeleteContactReturn();
            try
            {
                AccountContactTable? objToDelete = await _context.AccountContactTables
                    .Where(p => p.AccountContactId == p1.ContactId).FirstOrDefaultAsync();
                if (objToDelete == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                _context.AccountContactTables.Remove(objToDelete);
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
        public class DeleteContactReturn
        {
            public DeleteContactReturn()
            {
            }
            public string Message { get; set; }
        }

        public class NewContactWindowLoadedParam
        {
            public NewContactWindowLoadedParam()
            {
            }
            public long? ContactId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_UPDATE")]
        public async Task<IActionResult> NewContactWindowLoaded([FromBody] NewContactWindowLoadedParam p1)
        {
            NewContactWindowLoadedReturn obj_return = new NewContactWindowLoadedReturn();
            try
            {
                AccountContactTable? obj = await _context.AccountContactTables
                    .Where(p => p.AccountContactId == p1.ContactId).FirstOrDefaultAsync();
                if (obj == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.ObjToUpdate =obj;
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
        public class NewContactWindowLoadedReturn
        {
            public NewContactWindowLoadedReturn()
            {
                ObjToUpdate = new AccountContactTable();
            }
            public string? Message { get; set; }
            public AccountContactTable ObjToUpdate { get; set; }
        }
    }
}
