
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UAWebApplication.Models;
using UAWebApplication.Data;

namespace UAWebApplication.Controllers
{
   [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
    public class RoleController : Controller
    {
        private readonly UADbContext _context;
        private RoleManager<AppRole> _roleManager;
        public RoleController(UADbContext context, RoleManager<AppRole> roleMgr)
        {
            this._context = context;
            this._roleManager = roleMgr;
        }

        public ActionResult Role()
        {
            return View("~/Views/Admin/Roles.cshtml");
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

        public class RolesParamClass
        {
            public int? ResourceId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> RolesList([FromBody] RolesParamClass p1)
        {
            RolesListReturnClass obj_return = new RolesListReturnClass();
            try
            {
                List<AspNetRole> list2 = await _context.AspNetRoles.Where(p => p.ResourceId == p1.ResourceId)
                    .OrderBy(p => p.Name).ToListAsync();
                foreach (var item in list2)
                {
                    AspNetRole obj = new AspNetRole();
                    obj.Id = item.Id;
                    obj.Name = item.Name;
                    obj.NormalizedName = item.Name.ToUpper();
                    obj.ResourceId = item.ResourceId;
                    obj_return.RolesList.Add(obj);
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
        public class RolesListReturnClass
        {
            public RolesListReturnClass()
            {
                RolesList = new List<AspNetRole>();
            }
            public string Message { get; set; }
            public List<AspNetRole> RolesList { get; set; }
        }

        //Delete Role
        public class DeleteRoleParam
        {
            public string RoleId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> DeleteRole([FromBody] DeleteRoleParam p1)
        {
            DeleteRoleReturnClass obj_return = new DeleteRoleReturnClass();
            try
            {
                AppRole role = await _roleManager.FindByIdAsync(p1.RoleId);

                if (role != null)
                {
                    IdentityResult result = await _roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        obj_return.Message = "Role Deleted Successfully";
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            obj_return.Message += string.Format("{0},", error);
                        }
                    }
                }
                else
                {
                    obj_return.Message = "Role Not Found";
                }
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
        public class DeleteRoleReturnClass
        {
            public DeleteRoleReturnClass()
            {

            }
            public string Message { get; set; }
        }

        //New Role
        public class Role2Param
        {
            public string RoleId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> Role2([FromBody] Role2Param p1)
        {
            Role2ReturnClass obj_return = new Role2ReturnClass();
            try
            {
                AppRole role = await _roleManager.FindByIdAsync(p1.RoleId);
                obj_return.Role1 = role;
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
        public class Role2ReturnClass
        {
            public Role2ReturnClass()
            {

            }
            public string Message { get; set; }
            public AppRole Role1 { get; set; }
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> SaveRole([FromBody] AspNetRole role1)
        {
            SaveRoleReturnClass obj_return = new SaveRoleReturnClass();
            try
            {
                if (role1.Id == "0")
                {
                    AppRole role2 = new AppRole { Name = role1.Name, ResourceId = role1.ResourceId };
                    IdentityResult result = await _roleManager.CreateAsync(role2);
                    if (result.Succeeded)
                    {
                        obj_return.RolesList.Add(role2);
                        obj_return.Message = "Saved Successfully";
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            obj_return.Message += string.Format("{0},", error);
                        }
                    }

                }
                else
                {
                    AppRole role3 = await _roleManager.FindByIdAsync(role1.Id);
                    if (role3 != null)
                    {
                        role3.Name = role1.Name;
                        role3.ResourceId = role1.ResourceId;
                        IdentityResult result = await _roleManager.UpdateAsync(role3);
                        if (result.Succeeded)
                        {
                            obj_return.RolesList.Add(role3);
                            obj_return.Message = "Updated Successfully";
                        }
                    }
                    else
                    {
                        obj_return.Message = "Role Not Found";
                    }
                }
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
        public class SaveRoleReturnClass
        {
            public SaveRoleReturnClass()
            {
                RolesList = new List<AppRole>();
            }
            public string Message { get; set; }
            public List<AppRole> RolesList { get; set; }
        }
    }
}
