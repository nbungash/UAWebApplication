
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UAWebApplication.Models;
using UAWebApplication.Data;

namespace UAWebApplication.Controllers
{
    [Authorize]
    public class ResourceController : Controller
    {
        private readonly UADbContext _context;
        public ResourceController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public ActionResult Resource()
        {
            return View("~/Views/Admin/Resource.cshtml");
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public JsonResult ResourceList()
        {
            ResourceListReturnClass obj_return = new ResourceListReturnClass();
            try
            {
                List<ResourceTable> list1 = _context.ResourceTables.OrderBy(p => p.Title).ToList();
                foreach (var item in list1)
                {
                    ResourceTable obj = new ResourceTable();
                    obj.Id = item.Id;
                    obj.Title = item.Title;
                    obj_return.ResourceList.Add(obj);
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
        public class ResourceListReturnClass
        {
            public ResourceListReturnClass()
            {
                ResourceList = new List<ResourceTable>();
            }
            public string Message { get; set; }
            public List<ResourceTable> ResourceList { get; set; }
        }

        //Delete Resource
        public class DeletePageParam
        {
            public int? ResourceId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public ActionResult DeleteResource([FromBody]DeletePageParam p1)
        {
            string message = "";
            try
            {
                ResourceTable tt = _context.ResourceTables.Where(p => p.Id == p1.ResourceId).FirstOrDefault();
                if (tt != null)
                {
                    _context.ResourceTables.Remove(tt);
                    _context.SaveChanges();
                    message = "Resource Deleted Successfully";
                }
                else
                {
                    throw new Exception("Oops! Deletion Failed");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        message = string.Format("{0}", ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        message = string.Format("{0}", ex.InnerException.Message);
                    }
                }
                else
                {
                    message = string.Format("{0}", ex.Message);
                }
            }
            string json = JsonConvert.SerializeObject(message);
            return Json(json);
        }

        //New Resource
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public ActionResult SaveResource([FromBody]ResourceTable tt)
        {
            SaveResourceReturnClass obj_return = new SaveResourceReturnClass();
            try
            {
                ResourceTable obj = new ResourceTable();
                obj.Title = tt.Title;
                _context.ResourceTables.Add(obj);

                ResourceTable resource = _context.ResourceTables.Where(p => p.Title == tt.Title).FirstOrDefault();
                if (resource != null)
                {
                    throw new Exception("Resource already Exists");
                }

                _context.SaveChanges();
                obj_return.ResourceList.Add(obj);
                obj_return.Message = "Saved Successfully";
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
        public class SaveResourceReturnClass 
        {
            public SaveResourceReturnClass() 
            {
                ResourceList = new List<ResourceTable>();
            }
            public string Message { get; set; }
            public List<ResourceTable> ResourceList { get; set; }
        }


    }
}
