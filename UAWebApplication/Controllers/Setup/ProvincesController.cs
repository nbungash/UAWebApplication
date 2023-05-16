
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
    public class ProvincesController : Controller
    {
        private readonly UADbContext _context;
        public ProvincesController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
        public ActionResult Provinces()
        {
            return View("~/Views/Setup/Provinces.cshtml");
        }

        //View
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
        public JsonResult ProvincesList()
        {
            ProvincesListReturn obj_return = new ProvincesListReturn();
            try
            {
                List<ProvincesTable> list1 = _context.ProvincesTables.OrderBy(p => p.Name).ToList();
                foreach (var item in list1)
                {
                    obj_return.ProvincesList.Add(new ProvincesDto(item));
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
        public class ProvincesListReturn
        {
            public ProvincesListReturn()
            {
                ProvincesList = new List<ProvincesDto>();
            }
            public string Message { get; set; }
            public List<ProvincesDto> ProvincesList { get; set; }
        }
        public class ProvincesDto:ProvincesTable
        {
            public ProvincesDto() { }
            public ProvincesDto(ProvincesTable p1) 
            {
                this.Id = p1.Id;
                this.Name = p1.Name;
                this.InterProvinceSalesTax = p1.InterProvinceSalesTax;
            }
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
                ProvincesTable? objToDelete =await _context.ProvincesTables.Where(p => p.Id == p1.Id)
                    .FirstOrDefaultAsync();
                if (objToDelete == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                _context.ProvincesTables.Remove(objToDelete);
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
        public async Task<IActionResult> Save([FromBody] ProvincesDto p1)
        {
            SaveReturn obj_return = new SaveReturn();
            try
            {
                if (p1.Id == 0)
                {
                    ProvincesTable obj = new ProvincesTable();
                    obj.Name = p1.Name;
                    obj.InterProvinceSalesTax = p1.InterProvinceSalesTax;
                    _context.ProvincesTables.Add(obj); 
                    await _context.SaveChangesAsync();
                    obj_return.ProvincesList.Add(new ProvincesDto(obj));
                }
                else
                {
                    ProvincesTable? objToUpdate = await _context.ProvincesTables.Where(p => p.Id == p1.Id)
                        .FirstOrDefaultAsync();
                    if (objToUpdate == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    objToUpdate.Name = p1.Name;
                    objToUpdate.InterProvinceSalesTax = p1.InterProvinceSalesTax;
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
                ProvincesList = new List<ProvincesDto>();
            }
            public string Message { get; set; }
            public List<ProvincesDto> ProvincesList { get; set; }
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
                ProvincesTable? obj = await _context.ProvincesTables.Where(p => p.Id == p1.Id)
                    .FirstOrDefaultAsync();
                if (obj == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.ObjToUpdate = new ProvincesDto(obj);
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
                ObjToUpdate = new ProvincesDto();
            }
            public string? Message { get; set; }
            public ProvincesDto ObjToUpdate { get; set; }
        }

    }
}
