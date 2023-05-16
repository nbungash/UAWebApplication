
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
    public class ProductController : Controller
    {
        private readonly UADbContext _context;
        public ProductController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
        public ActionResult Product()
        {
            return View("~/Views/Setup/Product.cshtml");
        }

        //View
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SETUP_VIEW")]
        public class ProductsByCompanyListParam
        {
            public long? CompanyId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,CHART_OF_ACCOUNT_VIEW")]
        public JsonResult ProductsByCompanyList([FromBody] ProductsByCompanyListParam p1)
        {
            ProductsByCompanyListReturn obj_return = new ProductsByCompanyListReturn();
            try
            {
                List<ProductTable> list1 = _context.ProductTables.Where(p => p.PartyId == p1.CompanyId)
                    .OrderBy(p => p.Title).ToList();
                foreach (var item in list1)
                {
                    obj_return.ProductList.Add(new ProductDto(item,_context));
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
        public class ProductsByCompanyListReturn
        {
            public ProductsByCompanyListReturn()
            {
                ProductList = new List<ProductDto>();
            }
            public string Message { get; set; }
            public List<ProductDto> ProductList { get; set; }
        }
        public class ProductDto:ProductTable
        {
            public ProductDto() { }
            public ProductDto(ProductTable p1, UADbContext context) 
            {
                this.Id = p1.Id;
                this.Title = p1.Title;
                this.TitleUrdu = p1.TitleUrdu;
                this.ProductCode = p1.ProductCode;
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
                ProductTable? objToDelete =await _context.ProductTables.Where(p => p.Id == p1.Id)
                    .FirstOrDefaultAsync();
                if (objToDelete == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                _context.ProductTables.Remove(objToDelete);
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
        public async Task<IActionResult> Save([FromBody] ProductDto p1)
        {
            SaveReturn obj_return = new SaveReturn();
            try
            {
                if (p1.Id == 0)
                {
                    ProductTable obj = new ProductTable();
                    obj.Title = p1.Title;
                    obj.PartyId = p1.PartyId;
                    obj.ProductCode = p1.ProductCode;
                    obj.TitleUrdu = p1.TitleUrdu;
                    _context.ProductTables.Add(obj); 
                    await _context.SaveChangesAsync();
                    obj_return.ProductList.Add(new ProductDto(obj,_context));
                }
                else
                {
                    ProductTable? objToUpdate = await _context.ProductTables.Where(p => p.Id == p1.Id)
                        .FirstOrDefaultAsync();
                    if (objToUpdate == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    objToUpdate.Title = p1.Title;
                    objToUpdate.PartyId = p1.PartyId;
                    objToUpdate.ProductCode = p1.ProductCode;
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
                ProductList = new List<ProductDto>();
            }
            public string Message { get; set; }
            public List<ProductDto> ProductList { get; set; }
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
                ProductTable? obj = await _context.ProductTables.Where(p => p.Id == p1.Id)
                    .FirstOrDefaultAsync();
                if (obj == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.ObjToUpdate = new ProductDto(obj,_context);
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
                ObjToUpdate = new ProductDto();
            }
            public string? Message { get; set; }
            public ProductDto ObjToUpdate { get; set; }
        }

    }
}
