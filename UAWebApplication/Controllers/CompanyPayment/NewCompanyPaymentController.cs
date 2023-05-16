using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize]
    public class NewCompanyPaymentController : Controller
    {
        private readonly UADbContext _context;
        public NewCompanyPaymentController(UADbContext context)
        {
            _context = context;
        }
        public class NewCompanyPaymentWindowLoadedParam
        {
            public long? CompanyPaymentId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_ADD")]
        public async Task<IActionResult> NewCompanyPaymentWindowLoaded([FromBody]NewCompanyPaymentWindowLoadedParam p2)
        {
            NewCompanyPaymentWindowLoadedReturnClass obj = new NewCompanyPaymentWindowLoadedReturnClass();
            obj.Message = "OK";
            try
            {
                PsosummaryTable obj1 = await _context.PsosummaryTables.Where(p => p.Id == p2.CompanyPaymentId).FirstOrDefaultAsync();
                if (obj1 == null)
                {
                    throw new Exception("Oops! Company payment not found.");
                }
                PsosummaryTable obj2 = new PsosummaryTable();
                obj2.Id = obj1.Id;
                obj2.CompanyId = obj1.CompanyId;
                obj2.SummaryDate = obj1.SummaryDate;
                obj.CompanyPaymentList.Add(obj2);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        obj.Message = string.Format("{0}", ex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        obj.Message = string.Format("{0}", ex.InnerException.Message);
                    }
                }
                else
                {
                    obj.Message = string.Format("{0}", ex.Message);
                }
            }
            string json = JsonConvert.SerializeObject(obj);
            return Json(json);
        }
        public class NewCompanyPaymentWindowLoadedReturnClass
        {
            public NewCompanyPaymentWindowLoadedReturnClass() { CompanyPaymentList = new List<PsosummaryTable>(); }
            public string Message { get; set; }
            public List<PsosummaryTable> CompanyPaymentList { get; set; }
        }

        public class SavePaymentParamClass
        {
            public int? CompanyId { get; set; }
            public DateTime? PaymentDate { get; set; }
            public long? Id { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_PAYMENT_ADD")]
        public async Task<IActionResult> SavePayment([FromBody]SavePaymentParamClass p2)
        {
            string message = "Error";
            try
            {

                if (p2.Id == 0)
                {
                    PsosummaryTable cpt = new PsosummaryTable();
                    cpt.CompanyId = p2.CompanyId;
                    cpt.SummaryDate = p2.PaymentDate;
                    _context.PsosummaryTables.Add(cpt);
                }
                else
                {
                    PsosummaryTable? cpt = await _context.PsosummaryTables.Where(p => p.Id == p2.Id).FirstOrDefaultAsync();
                    if (cpt == null)
                    {
                        throw new Exception("Oops! payment summary not found.");
                    }
                    cpt.SummaryDate = p2.PaymentDate;

                }
                await _context.SaveChangesAsync();
                message = "OK";
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

    }
}