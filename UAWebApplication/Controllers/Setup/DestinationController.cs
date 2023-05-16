
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
    public class DestinationController : Controller
    {
        private readonly UADbContext _context;
        public DestinationController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
        public ActionResult Destination()
        {
            return View("~/Views/Setup/Destination.cshtml");
        }

        //View
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
        public class DestinationsByCompanyListParam
        {
            public long? CompanyId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public JsonResult DestinationsByCompanyList([FromBody] DestinationsByCompanyListParam p1)
        {
            DestinationsByCompanyListReturn obj_return = new DestinationsByCompanyListReturn();
            try
            {
                List<DestinationTable> list1 = _context.DestinationTables.Where(p => p.PartyId == p1.CompanyId)
                    .OrderBy(p => p.Title).ToList();
                foreach (var item in list1)
                {
                    obj_return.DestinationList.Add(new DestinationDto(item,_context));
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
        public class DestinationsByCompanyListReturn
        {
            public DestinationsByCompanyListReturn()
            {
                DestinationList = new List<DestinationDto>();
            }
            public string Message { get; set; }
            public List<DestinationDto> DestinationList { get; set; }
        }
        public class DestinationDto:DestinationTable
        {
            public DestinationDto() { }
            public DestinationDto(DestinationTable p1, UADbContext context) 
            {
                this.Id = p1.Id;
                this.Title = p1.Title;
                this.TitleUrdu = p1.TitleUrdu;
                this.DestinationCode = p1.DestinationCode;
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
                DestinationTable? objToDelete =await _context.DestinationTables.Where(p => p.Id == p1.Id)
                    .FirstOrDefaultAsync();
                if (objToDelete == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                _context.DestinationTables.Remove(objToDelete);
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
        public async Task<IActionResult> Save([FromBody] DestinationDto p1)
        {
            SaveReturn obj_return = new SaveReturn();
            try
            {
                if (p1.Id == 0)
                {
                    DestinationTable obj = new DestinationTable();
                    obj.Title = p1.Title;
                    obj.PartyId = p1.PartyId;
                    obj.DestinationCode = p1.DestinationCode;
                    obj.TitleUrdu = p1.TitleUrdu;
                    _context.DestinationTables.Add(obj); 
                    await _context.SaveChangesAsync();
                    obj_return.DestinationList.Add(new DestinationDto(obj,_context));
                }
                else
                {
                    DestinationTable? objToUpdate = await _context.DestinationTables.Where(p => p.Id == p1.Id)
                        .FirstOrDefaultAsync();
                    if (objToUpdate == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    objToUpdate.Title = p1.Title;
                    objToUpdate.PartyId = p1.PartyId;
                    objToUpdate.DestinationCode = p1.DestinationCode;
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
                DestinationList = new List<DestinationDto>();
            }
            public string Message { get; set; }
            public List<DestinationDto> DestinationList { get; set; }
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
                DestinationTable? obj = await _context.DestinationTables.Where(p => p.Id == p1.Id)
                    .FirstOrDefaultAsync();
                if (obj == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.ObjToUpdate = new DestinationDto(obj,_context);
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
                ObjToUpdate = new DestinationDto();
            }
            public string? Message { get; set; }
            public DestinationDto ObjToUpdate { get; set; }
        }

    }
}
