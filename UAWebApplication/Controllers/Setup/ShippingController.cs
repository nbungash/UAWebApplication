
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
    public class ShippingController : Controller
    {
        private readonly UADbContext _context;
        public ShippingController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
        public ActionResult Shipping()
        {
            return View("~/Views/Setup/Shipping.cshtml");
        }

        //View
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
        public class ShippingsByCompanyListParam
        {
            public long? CompanyId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public JsonResult ShippingsByCompanyList([FromBody] ShippingsByCompanyListParam p1)
        {
            ShippingsByCompanyListReturn obj_return = new ShippingsByCompanyListReturn();
            try
            {
                List<ShippingTable> list1 = _context.ShippingTables.Where(p => p.PartyId == p1.CompanyId)
                    .OrderBy(p => p.Title).ToList();
                foreach (var item in list1)
                {
                    obj_return.ShippingList.Add(new ShippingDto(item,_context));
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
        public class ShippingsByCompanyListReturn
        {
            public ShippingsByCompanyListReturn()
            {
                ShippingList = new List<ShippingDto>();
            }
            public string Message { get; set; }
            public List<ShippingDto> ShippingList { get; set; }
        }
        public class ShippingDto:ShippingTable
        {
            public ShippingDto() { }
            public ShippingDto(ShippingTable p1, UADbContext context) 
            {
                this.Id = p1.Id;
                this.Title = p1.Title;
                this.TitleUrdu = p1.TitleUrdu;
                this.ShippingCode = p1.ShippingCode;
                this.PartyId = p1.PartyId;
                this.CompanyTitle = context.AccountTables.Where(p => p.AccountId == p1.PartyId)
                    .Select(p => p.Title).FirstOrDefault();
            }
            public string? CompanyTitle { get; set; }
        }
        
        //Delete
        public class DeleteParam
        {
            public int? Id { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_DELETE")]
        public async Task<IActionResult> Delete([FromBody] DeleteParam p1)
        {
            DeleteReturn obj_return=new DeleteReturn();
            try
            {
                ShippingTable? objToDelete =await _context.ShippingTables.Where(p => p.Id == p1.Id)
                    .FirstOrDefaultAsync();
                if (objToDelete == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                _context.ShippingTables.Remove(objToDelete);
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
        public async Task<IActionResult> Save([FromBody] ShippingDto p1)
        {
            SaveReturn obj_return = new SaveReturn();
            try
            {
                if (p1.Id == 0)
                {
                    ShippingTable obj = new ShippingTable();
                    obj.Title = p1.Title;
                    obj.PartyId = p1.PartyId;
                    obj.ShippingCode = p1.ShippingCode;
                    obj.TitleUrdu = p1.TitleUrdu;
                    _context.ShippingTables.Add(obj); 
                    await _context.SaveChangesAsync();
                    obj_return.ShippingList.Add(new ShippingDto(obj,_context));
                }
                else
                {
                    ShippingTable? objToUpdate = await _context.ShippingTables.Where(p => p.Id == p1.Id)
                        .FirstOrDefaultAsync();
                    if (objToUpdate == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    objToUpdate.Title = p1.Title;
                    objToUpdate.PartyId = p1.PartyId;
                    objToUpdate.ShippingCode = p1.ShippingCode;
                    objToUpdate.TitleUrdu = p1.TitleUrdu;
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
                ShippingList = new List<ShippingDto>();
            }
            public string Message { get; set; }
            public List<ShippingDto> ShippingList { get; set; }
        }

        //Window Loaded
        public class WindowLoadedParam
        {
            public WindowLoadedParam()
            {
            }
            public int Id { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_UPDATE")]
        public async Task<IActionResult> WindowLoaded([FromBody] WindowLoadedParam p1)
        {
            WindowLoadedReturn obj_return = new WindowLoadedReturn();
            try
            {
                ShippingTable? obj = await _context.ShippingTables.Where(p => p.Id == p1.Id)
                    .FirstOrDefaultAsync();
                if (obj == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.ObjToUpdate = new ShippingDto(obj,_context);
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
                ObjToUpdate = new ShippingDto();
            }
            public string? Message { get; set; }
            public ShippingDto ObjToUpdate { get; set; }
        }

    }
}
