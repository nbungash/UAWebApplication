using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Globalization;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers.CompanyBill
{
    public class SalesTaxSummaryController : Controller
    {
        private readonly UADbContext _context;
        public SalesTaxSummaryController(UADbContext context)
        {
            _context = context;
        }
        public ActionResult SalesTaxSummary()
        {
            return View("~/Views/CompanyBill/SalesTaxSummary.cshtml");
        }

       //Get Year
        [Authorize]
        public async Task<ActionResult> YearList()
        {
            YearListReturnClass obj_return = new YearListReturnClass();
            try
            {
                List<int> years = await _context.PartyBillTables.Where(p => p.BillDate.Value.Year <= DateTime.Today.Year)
                    .Select(p => p.BillDate.Value.Year).Distinct().OrderByDescending(p => p).ToListAsync();
               
                obj_return.yearText = years;

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
        public class YearListReturnClass
        {
            public YearListReturnClass()
            {
                yearText = new List<int>();
            }
            public string? Message { get; set; }
            public List<int> yearText { get; set; }
        }


        // Get DateList 
        [Authorize]
        public class SalesTaxSummaryDateParamClass
        {
            public int? BillYear { get; set; }
        }
        public async Task<ActionResult> BillDateList([FromBody] SalesTaxSummaryDateParamClass p1)
        {
            DateListReturnClass obj_return = new DateListReturnClass();
            try
            {
                _context.Database.SetCommandTimeout(300);
                var BillDate = await _context.PartyBillTables.Where(p => p.BillDate.Value.Year == p1.BillYear)
                    .Select(p => string.Format("{0:dd-MM-yyyy}", p.BillDate)).Distinct().ToListAsync();

                obj_return.DateList = BillDate;
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
        public class DateListReturnClass
        {
            public DateListReturnClass()
            {
                DateList = new List<string>();
            }
            public string Message { get; set; }
            public List<string>? DateList { get; set; }
        }
        
        //View
        public class SalesTaxSummaryClass
        {
            public string BillDate { get; set; }
        }
        public async Task<ActionResult> SummaryView([FromBody] SalesTaxSummaryClass p1)
        {
            SummaryViewReturnClass obj_return = new SummaryViewReturnClass();
            try
            {
                _context.Database.SetCommandTimeout(300);
                int day = int.Parse(p1.BillDate.Substring(0, 2));
                int month = int.Parse(p1.BillDate.Substring(3, 2));
                int year = int.Parse(p1.BillDate.Substring(6, 4));
                DateTime? billDate = new DateTime(year, month, day);
                List<PartyBillTable> partyBillList = await _context.PartyBillTables.Where(p => p.BillDate == billDate).ToListAsync();
                foreach (var partyBill in partyBillList)
                {
                    List<SalesTaxInvoicesTable> SalesList = await _context.SalesTaxInvoicesTables.Where(p => p.PartyBillId == partyBill.Id).ToListAsync();
                    if (SalesList == null)
                    {
                        throw new Exception("There is no SalesTax Generated");
                    }
                    int count = 1;
                    //string previousBillNo = null;
                    foreach (var item in SalesList)
                    {
                        SummaryViewClass obj = new SummaryViewClass();
                        obj.Sno = count++;
                        var percentage = item.SalesTaxPercent;
                        obj.BillDate = string.Format("{0:dd-MMM-yyyy}", item.InvoiceDate);
                        var Freight = await _context.TripTables.Where(p => p.PartyBillId == item.PartyBillId).SumAsync(p => p.Freight);
                        obj.BillNo = await _context.PartyBillTables.Where(p => p.Id == item.PartyBillId).Select(p => p.BillNo).FirstOrDefaultAsync();
                        obj.Amount = Freight;
                        string? Shipping = await _context.ProvincesTables.Where(p => p.Id == item.ShippingProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                        string? Destination = await _context.ProvincesTables.Where(p => p.Id == item.DestinationProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                        obj.FromToPrvince = string.Format("{0} To {1}", Shipping, Destination);
                        obj.InvoiceProvince = string.Format("{0:n2}", await _context.ProvincesTables.Where(p => p.Id == item.InvoiceProvinceId).Select(p => p.Name).FirstOrDefaultAsync());
                        var InvoiceOfProvince = await _context.ProvincesTables.Where(p => p.Id == item.InvoiceProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                        obj.InvoiceNo = item.InvoiceNo;
                        if (Shipping == Destination)
                        {
                            decimal? salesTax = Freight * (percentage / 100);
                            obj.SalesTaxAmount = string.Format("{0:n2}", Freight + salesTax);
                        }
                        else
                        {
                            decimal? halfAmount = Freight / 2;
                            decimal? salesTax = halfAmount * (percentage / 100);
                            obj.SalesTaxAmount = string.Format("{0:n2}", halfAmount + salesTax);
                        }
                        if (Shipping == InvoiceOfProvince)
                        {
                            obj.Type = "Shipping";
                        }
                        else
                        {
                            obj.Type = "Destination";
                        }
                        obj.BillId = item.PartyBillId;
                        obj_return.SummaryView.Add(obj);

                        //previousBillNo = obj.BillNo;
                    }
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
        public class SummaryViewReturnClass
        {
            public SummaryViewReturnClass()
            {
                SummaryView = new List<SummaryViewClass>();

            }
            public string Message { get; set; }
            public List<SummaryViewClass> SummaryView { get; set; }
        }
        public class SummaryViewClass
        {
            public int? Sno { get; set; }
            public string? BillDate { get; set; }
            public string? BillNo { get; set; }
            public decimal? Amount { get; set; }
            public string? FromToPrvince { get; set; }
            public string? InvoiceProvince { get; set; }
            public string? SalesTaxAmount { get; set; }
            public string? InvoiceNo { get; set; }
            public string? Type { get; set; }
            public long? BillId { get; set; }
        }

        //View Date Duration
        public class SalesTaxSummaryDateDurationParam
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
        public async Task<ActionResult> SummaryViewByDateDuration([FromBody] SalesTaxSummaryDateDurationParam p1)
        {
            SummaryViewReturnClass obj_return = new SummaryViewReturnClass();
            try
            {
                _context.Database.SetCommandTimeout(300);
                List<SalesTaxInvoicesTable> SalesList = await _context.SalesTaxInvoicesTables.Where(p => p.InvoiceDate >= p1.FromDate && p.InvoiceDate <= p1.ToDate).ToListAsync();

                int count = 1;
                string previousBillNo = null;
                foreach (var item in SalesList)
                {
                    SummaryViewClass obj = new SummaryViewClass();
                    obj.Sno = count++;
                    var percentage = item.SalesTaxPercent;
                    obj.BillDate = string.Format("{0:dd-MMM-yyyy}", item.InvoiceDate);
                    var Freight = await _context.TripTables.Where(p => p.PartyBillId == item.PartyBillId).SumAsync(p => p.Freight);
                    obj.BillNo = await _context.PartyBillTables.Where(p => p.Id == item.PartyBillId).Select(p => p.BillNo).FirstOrDefaultAsync();

                    if (obj.BillNo == previousBillNo)
                    {
                        obj.Amount = null;
                    }
                    else
                    {
                        obj.Amount = Freight;
                    }

                    string? Shipping = await _context.ProvincesTables.Where(p => p.Id == item.ShippingProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    string? Destination = await _context.ProvincesTables.Where(p => p.Id == item.DestinationProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    obj.FromToPrvince = string.Format("{0} To {1}", Shipping, Destination);
                    obj.InvoiceProvince = string.Format("{0:n2}", await _context.ProvincesTables.Where(p => p.Id == item.InvoiceProvinceId).Select(p => p.Name).FirstOrDefaultAsync());
                    var InvoiceOfProvince = await _context.ProvincesTables.Where(p => p.Id == item.InvoiceProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    obj.InvoiceNo = item.InvoiceNo;
                    if (Shipping == Destination)
                    {
                        decimal? salesTax = Freight * (percentage / 100);
                        obj.SalesTaxAmount = string.Format("{0:n2}", Freight + salesTax);
                    }
                    else
                    {
                        decimal? halfAmount = Freight / 2;
                        decimal? salesTax = halfAmount * (percentage / 100);
                        obj.SalesTaxAmount = string.Format("{0:n2}", halfAmount + salesTax);
                    }
                    if (Shipping == InvoiceOfProvince)
                    {
                        obj.Type = "Shipping";
                    }
                    else
                    {
                        obj.Type = "Destination";
                    }
                    obj.BillId = item.PartyBillId;
                    obj_return.SummaryView.Add(obj);

                    previousBillNo = obj.BillNo;
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

        //View Specific Date List
        public class SalesTaxSummaryDateParam
        {
            public DateTime? Date { get; set; }
        }
        public async Task<ActionResult> SummaryViewByDate([FromBody] SalesTaxSummaryDateParam p1)
        {
            SummaryViewReturnClass obj_return = new SummaryViewReturnClass();
            try
            {
                _context.Database.SetCommandTimeout(300);
                List<SalesTaxInvoicesTable> SalesList = await _context.SalesTaxInvoicesTables.Where(p => p.InvoiceDate == p1.Date).ToListAsync();

                int count = 1;
                string previousBillNo = null;
                foreach (var item in SalesList)
                {
                    SummaryViewClass obj = new SummaryViewClass();
                    obj.Sno = count++;
                    var percentage = item.SalesTaxPercent;
                    obj.BillDate = string.Format("{0:dd-MMM-yyyy}", item.InvoiceDate);
                    var Freight = await _context.TripTables.Where(p => p.PartyBillId == item.PartyBillId).SumAsync(p => p.Freight);
                    obj.BillNo = await _context.PartyBillTables.Where(p => p.Id == item.PartyBillId).Select(p => p.BillNo).FirstOrDefaultAsync();

                    if (obj.BillNo == previousBillNo)
                    {
                        obj.Amount = null;
                    }
                    else
                    {
                        obj.Amount = Freight;
                    }

                    string? Shipping = await _context.ProvincesTables.Where(p => p.Id == item.ShippingProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    string? Destination = await _context.ProvincesTables.Where(p => p.Id == item.DestinationProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    obj.FromToPrvince = string.Format("{0} To {1}", Shipping, Destination);
                    obj.InvoiceProvince = string.Format("{0:n2}", await _context.ProvincesTables.Where(p => p.Id == item.InvoiceProvinceId).Select(p => p.Name).FirstOrDefaultAsync());
                    var InvoiceOfProvince = await _context.ProvincesTables.Where(p => p.Id == item.InvoiceProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    obj.InvoiceNo = item.InvoiceNo;
                    if (Shipping == Destination)
                    {
                        decimal? salesTax = Freight * (percentage / 100);
                        obj.SalesTaxAmount = string.Format("{0:n2}", Freight + salesTax);
                    }
                    else
                    {
                        decimal? halfAmount = Freight / 2;
                        decimal? salesTax = halfAmount * (percentage / 100);
                        obj.SalesTaxAmount = string.Format("{0:n2}", halfAmount + salesTax);
                    }
                    if (Shipping == InvoiceOfProvince)
                    {
                        obj.Type = "Shipping";
                    }
                    else
                    {
                        obj.Type = "Destination";
                    }
                    obj.BillId = item.PartyBillId;
                    obj_return.SummaryView.Add(obj);

                    previousBillNo = obj.BillNo;
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

        //View Specific Date List
        public class SalesTaxSummaryInvoiceParam
        {
            public string? InvoiceNo { get; set; }
        }
        public async Task<ActionResult> SummaryViewByInvoice([FromBody] SalesTaxSummaryInvoiceParam p1)
        {
            SummaryViewReturnClass obj_return = new SummaryViewReturnClass();
            try
            {
                _context.Database.SetCommandTimeout(300);
                List<SalesTaxInvoicesTable> SalesList = await _context.SalesTaxInvoicesTables.Where(p => p.InvoiceNo.Contains(p1.InvoiceNo)).ToListAsync();

                int count = 1;
                string previousBillNo = null;
                foreach (var item in SalesList)
                {
                    SummaryViewClass obj = new SummaryViewClass();
                    obj.Sno = count++;
                    var percentage = item.SalesTaxPercent;
                    obj.BillDate = string.Format("{0:dd-MMM-yyyy}", item.InvoiceDate);
                    var Freight = await _context.TripTables.Where(p => p.PartyBillId == item.PartyBillId).SumAsync(p => p.Freight);
                    obj.BillNo = await _context.PartyBillTables.Where(p => p.Id == item.PartyBillId).Select(p => p.BillNo).FirstOrDefaultAsync();

                    if (obj.BillNo == previousBillNo)
                    {
                        obj.Amount = null;
                    }
                    else
                    {
                        obj.Amount = Freight;
                    }

                    string? Shipping = await _context.ProvincesTables.Where(p => p.Id == item.ShippingProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    string? Destination = await _context.ProvincesTables.Where(p => p.Id == item.DestinationProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    obj.FromToPrvince = string.Format("{0} To {1}", Shipping, Destination);
                    obj.InvoiceProvince = string.Format("{0:n2}", await _context.ProvincesTables.Where(p => p.Id == item.InvoiceProvinceId).Select(p => p.Name).FirstOrDefaultAsync());
                    var InvoiceOfProvince = await _context.ProvincesTables.Where(p => p.Id == item.InvoiceProvinceId).Select(p => p.Name).FirstOrDefaultAsync();
                    obj.InvoiceNo = item.InvoiceNo;
                    if (Shipping == Destination)
                    {
                        decimal? salesTax = Freight * (percentage / 100);
                        obj.SalesTaxAmount = string.Format("{0:n2}", Freight + salesTax);
                    }
                    else
                    {
                        decimal? halfAmount = Freight / 2;
                        decimal? salesTax = halfAmount * (percentage / 100);
                        obj.SalesTaxAmount = string.Format("{0:n2}", halfAmount + salesTax);
                    }
                    if (Shipping == InvoiceOfProvince)
                    {
                        obj.Type = "Shipping";
                    }
                    else
                    {
                        obj.Type = "Destination";
                    }
                    obj.BillId = item.PartyBillId;
                    obj_return.SummaryView.Add(obj);

                    previousBillNo = obj.BillNo;
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
    }
}
