
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
    public class UserController : Controller
    {
        private readonly UADbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public UserController(UADbContext context, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userMgr)
        {
            this._context = context;
            this._userManager = userMgr;
            this._signInManager = signInManager;
        }

        // View Users
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public ActionResult UserManagement()
        {
            return View("~/Views/Admin/UserManagement.cshtml");
        }


        // User List
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public ActionResult UserList()
        {
            UserListReturnClass obj_return = new UserListReturnClass();
            try
            {
                List<AspNetUser> list2 = _context.AspNetUsers.Where(p => p.UserName != "developer").OrderBy(p => p.UserName).ToList();
                foreach (var item in list2)
                {
                    UserClass obj = new UserClass();
                    obj.Id = item.Id;
                    obj.UserName = item.UserName;
                    obj.Email = item.Email;
                    if (item.EmailConfirmed == true)
                    {
                        obj.ActivationStatus = "Activated";
                    }
                    else
                    {
                        obj.ActivationStatus = "Not Activated";
                    }
                    obj_return.UserList.Add(obj);
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
        public class UserListReturnClass
        {
            public UserListReturnClass()
            {
                UserList = new List<UserClass>();
            }
            public string Message { get; set; }
            public List<UserClass> UserList { get; set; }
        }
        public class UserClass : AspNetUser
        {
            public string ActivationStatus { get; set; }
        }

        //Activate User
        public class ActivateUserParamClass
        {
            public string UserId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> ActivateUser([FromBody] ActivateUserParamClass p1)
        {
            ActivateUserReturnClass obj_return = new ActivateUserReturnClass();
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(p1.UserId);

                if (user != null)
                {
                    user.EmailConfirmed = true;
                    IdentityResult result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        obj_return.Message = "User Activated Successfully";
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
                    obj_return.Message = "User Not Found";
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
        public class ActivateUserReturnClass
        {
            public ActivateUserReturnClass()
            {

            }
            public string Message { get; set; }
        }

        //DeActivate User
        public class DeActivateUserParamClass
        {
            public string UserId { get; set; }
        }
        //[Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> DeActivateUser([FromBody] DeActivateUserParamClass p1)
        {
            DeActivateUserReturnClass obj_return = new DeActivateUserReturnClass();
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(p1.UserId);

                if (user != null)
                {
                    user.EmailConfirmed = false;
                    IdentityResult result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        obj_return.Message = "User De Activated Successfully";
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
                    obj_return.Message = "User Not Found";
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
        public class DeActivateUserReturnClass
        {
            public DeActivateUserReturnClass()
            {

            }
            public string Message { get; set; }
        }

        //New User
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> SaveUser([FromBody] ApplicationUser tt)
        {
            SaveUserReturnClass obj_return = new SaveUserReturnClass();
            try
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = tt.UserName,
                    Email = tt.Email,
                    DisplayName = tt.DisplayName,
                    UserTheme = "base",
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount= 0,
                };
                IdentityResult result = await _userManager.CreateAsync(user, "P@ssw0rd");
                if (result.Succeeded)
                {
                    UserClass obj = new UserClass();
                    obj.Id = user.Id;
                    obj.UserName = user.UserName;
                    obj.Email = user.Email;
                    obj.ActivationStatus = "Not Activated";
                    obj_return.UserList.Add(obj);
                    obj_return.Message = "Saved Successfully";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        obj_return.Message += string.Format("{0},", error.Description);
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
        public class SaveUserReturnClass
        {
            public SaveUserReturnClass()
            {
                UserList = new List<UserClass>();
            }
            public string? Message { get; set; }
            public List<UserClass> UserList { get; set; }
        }

        //User Theme
        public async Task<ActionResult> ShowUserTheme()
        {
            ShowUserThemeReturnClass obj_return = new ShowUserThemeReturnClass();
            try
            {
                if (User.Identity.IsAuthenticated) 
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        obj_return.UserTheme = user.UserTheme;
                        obj_return.Message = "OK";
                    }
                }
                else 
                {
                    obj_return.UserTheme = "start";
                    obj_return.Message = "OK";
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
        public class ShowUserThemeReturnClass
        {
            public ShowUserThemeReturnClass()
            {

            }
            public string Message { get; set; }
            public string UserTheme { get; set; }
        }

        //Delete User
        public class DeleteUserParamClass
        {
            public string UserId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> DeleteUser([FromBody] DeleteUserParamClass p1)
        {
            DeleteUserReturnClass obj_return = new DeleteUserReturnClass();
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(p1.UserId);

                if (user != null)
                {
                    IdentityResult result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        obj_return.Message = "User Deleted Successfully";
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
                    obj_return.Message = "User Not Found";
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
        public class DeleteUserReturnClass
        {
            public DeleteUserReturnClass()
            {

            }
            public string Message { get; set; }
        }

        //User Roles 
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
        public class UserRolesListParamClass
        {
            public int? ResourceId { get; set; }
            public string UserId { get; set; }
        }
       [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> UserRolesList([FromBody] UserRolesListParamClass p1)
        {
            UserRolesListReturnClass obj_return = new UserRolesListReturnClass();
            try
            {
                List<AspNetRole> list2 = await _context.AspNetRoles.Where(p => p.ResourceId == p1.ResourceId && p.Name != "DEVELOPER")
                    .OrderBy(p => p.Name).ToListAsync();
                foreach (var item in list2)
                {
                    UserRolesClass obj = new UserRolesClass();
                    obj.Id = item.Id;
                    obj.Name = item.Name;
                    AspNetUserRole user_role = _context.AspNetUserRoles.Where(p => p.RoleId == item.Id && p.UserId == p1.UserId).FirstOrDefault();
                    if (user_role != null)
                    {
                        obj.IsChecked = true;
                    }
                    else
                    {
                        obj.IsChecked = false;
                    }
                    obj_return.UserRolesList.Add(obj);
                    
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
        public class UserRolesListReturnClass
        {
            public UserRolesListReturnClass()
            {
                UserRolesList = new List<UserRolesClass>();
            }
            public string Message { get; set; }
            public List<UserRolesClass> UserRolesList { get; set; }
        }
        public class UserRolesClass
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool? IsChecked { get; set; }
        }

        public class SaveUserRolesParam
        {
            public SaveUserRolesParam()
            {
                UserRolesList = new List<UserRolesClass>();
            }
            public string UserId { get; set; }
            public int ResourceId { get; set; }
            public List<UserRolesClass> UserRolesList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR")]
        public async Task<ActionResult> SaveUserRoles([FromBody] SaveUserRolesParam p1)
        {
            SaveUserRolesReturnClass obj_return = new SaveUserRolesReturnClass();
            try
            {
                if (p1.UserRolesList != null)
                {
                    ApplicationUser user =await _userManager.FindByIdAsync(p1.UserId);
                    List<string> roles_list =await _context.AspNetRoles.Where(p => p.ResourceId == p1.ResourceId && p.Name != "DEVELOPER")
                        .Select(p => p.Name).ToListAsync();
                    if (roles_list.Count > 0)
                    {
                        IdentityResult result =await _userManager.RemoveFromRolesAsync(user, roles_list);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                obj_return.Message += string.Format("{0},", error);
                            }
                        }
                    }
                    foreach (var item in p1.UserRolesList)
                    {
                        if (item.IsChecked == true)
                        {
                            bool? is_in_role =await _userManager.IsInRoleAsync(user, item.Name.Trim());
                            if (is_in_role == false)
                            {
                                IdentityResult result =await _userManager.AddToRoleAsync(user, item.Name.Trim());
                                if (!result.Succeeded)
                                {
                                    string error_string = "";
                                    foreach (var error in result.Errors)
                                    {
                                        error_string += string.Format("{0},", error);
                                    }
                                    throw new Exception(error_string);
                                }

                            }
                        }
                    }
                    obj_return.Message = "Saved Successfully.";
                }
                else
                {
                    obj_return.Message = "Oops! There is no Data.";
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
        public class SaveUserRolesReturnClass
        {
            public SaveUserRolesReturnClass()
            {
            }
            public string Message { get; set; }
        }

    }
}
