
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;
using static UAWebApplication.Controllers.ChartOfAccountController;
using static UAWebApplication.Controllers.TripController;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_VIEW")]
    public class TripController : Controller
    {
        private readonly UADbContext _context;
        public TripController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_VIEW")]
        public ActionResult Trip()
        {
            return View("~/Views/Books/Trip.cshtml");
        }

        //View
        public class FilterByDateRangeParam
        {
            public long? CompanyId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_VIEW")]
        public async Task<IActionResult> FilterByDateRange([FromBody] FilterByDateRangeParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<TripTable> list1 = new List<TripTable>();
                if (p1.CompanyId == 0)
                {
                    list1 = await _context.TripTables.Where(p => p.EntryDate >= p1.FromDate &&
                        p.EntryDate <= p1.ToDate).OrderBy(p => p.EntryDate).ToListAsync();
                }
                else
                {
                    list1 = await _context.TripTables.Where(p =>p.PartyId==p1.CompanyId &&
                    p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).OrderBy(p => p.EntryDate).ToListAsync();
                }
                foreach (var item in list1)
                {
                    obj_return.TripList.Add(new TripDto(item,_context));
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
        public class TripDto : TripTable
        {
            public TripDto() { }
            public TripDto(TripTable jt, UADbContext context)
            {
                this.TripId=jt.TripId;
                this.TokenNo = jt.TokenNo;
                this.Lorry = jt.Lorry;
                this.LorryTitle = context.AccountTables.Where(p => p.AccountId == jt.Lorry)
                    .Select(p=>p.Title).FirstOrDefault();
                this.InvoiceDate = jt.InvoiceDate;
                this.EntryDate = jt.EntryDate;
                this.Quantity = jt.Quantity;
                this.QtyUnit = jt.QtyUnit;
                this.Rate = jt.Rate;
                this.Freight = jt.Freight;
                this.Commission = jt.Commission;
                this.ShortQty = jt.ShortQty;
                this.ShortRate = jt.ShortRate;
                this.ShortAmount = jt.ShortAmount;
                this.ShippingId = jt.ShippingId;
                this.DestinationId = jt.DestinationId;
                this.ProductId = jt.ProductId;
                this.PartyBillId = jt.PartyBillId;
                this.LorryBillNo = jt.LorryBillNo;
                this.UserId = jt.UserId;
                this.TransactionDate = jt.TransactionDate;
                this.CommissionPercent = jt.CommissionPercent;
                this.TaxPercent = jt.TaxPercent;
                this.Tax = jt.Tax;
                this.SummaryId = jt.SummaryId;
                this.SummaryShort = jt.SummaryShort;
                this.PartyId = jt.PartyId;
                List<JournalTable> jtList = context.JournalTables
                    .Where(p => p.TripId == jt.TripId && p.Debit != null && p.AccountId == jt.Lorry).ToList();
                this.TripAdvance = jtList.Where(p => p.EntryType != "TM").Sum(p => p.Debit).GetValueOrDefault(0);
                this.Munshiana = jtList.Where(p => p.EntryType == "TM").Sum(p => p.Debit).GetValueOrDefault(0);
            }
            public string? LorryTitle { get; set; }
            public decimal? TripAdvance { get; set; }
            public decimal? Munshiana { get; set; }
        }

        public class FindByAllParam
        {
            public long? CompanyId { get; set; }
            public long? LorryId { get; set; }
            public long? ShippingId { get; set; }
            public long? DestinationId { get; set; }
            public long? ProductId { get; set; }
            public string? DateCriteria { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
                
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_VIEW")]
        public async Task<IActionResult> FindByAll([FromBody] FindByAllParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<TripTable> td = new List<TripTable>();
                long? shippingid =p1.ShippingId;
                long? destinationid =p1.DestinationId;
                long? productid =p1.ProductId;
                long? lorryid =p1.LorryId;
                if (p1.DateCriteria == "Invoice Date")
                {
                    if (lorryid == 0)
                    {
                        if (shippingid == 0 && destinationid == 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.InvoiceDate >= p1.FromDate &&
                                p.InvoiceDate <= p1.ToDate).ToListAsync();

                        }
                        if (shippingid == 0 && destinationid == 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ProductId == productid &&
                                p.InvoiceDate >= p1.FromDate && p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid != 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.DestinationId == destinationid &&
                                p.ProductId == productid && p.InvoiceDate >= p1.FromDate &&
                                p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid != 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.DestinationId == destinationid && p.ProductId == productid &&
                                p.InvoiceDate >= p1.FromDate && p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid == 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.InvoiceDate >= p1.FromDate && p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid == 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.ProductId == productid && p.InvoiceDate >= p1.FromDate &&
                                p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid != 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.DestinationId == destinationid &&
                                p.InvoiceDate >= p1.FromDate && p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                    }
                    else
                    {
                        if (shippingid == 0 && destinationid == 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.Lorry == lorryid &&
                                p.InvoiceDate >= p1.FromDate && p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid == 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ProductId == productid &&
                                p.Lorry == lorryid && p.InvoiceDate >= p1.FromDate &&
                                p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid != 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.DestinationId == destinationid &&
                                p.ProductId == productid && p.Lorry == lorryid &&
                                p.InvoiceDate >= p1.FromDate && p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid != 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.DestinationId == destinationid && p.ProductId == productid &&
                                p.Lorry == lorryid && p.InvoiceDate >= p1.FromDate &&
                                p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid == 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.Lorry == lorryid && p.InvoiceDate >= p1.FromDate &&
                                p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid == 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.ProductId == productid && p.Lorry == lorryid && p.InvoiceDate >= p1.FromDate &&
                                p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid != 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.DestinationId == destinationid &&
                                p.Lorry == lorryid && p.InvoiceDate >= p1.FromDate &&
                                p.InvoiceDate <= p1.ToDate).ToListAsync();
                        }
                    }
                }
                else
                {
                    if (lorryid == 0)
                    {
                        if (shippingid == 0 && destinationid == 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.EntryDate >= p1.FromDate &&
                                p.EntryDate <= p1.ToDate).ToListAsync();

                        }
                        if (shippingid == 0 && destinationid == 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ProductId == productid &&
                                p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid != 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.DestinationId == destinationid &&
                                p.ProductId == productid && p.EntryDate >= p1.FromDate &&
                                p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid != 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.DestinationId == destinationid && p.ProductId == productid &&
                                p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid == 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid == 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.ProductId == productid && p.EntryDate >= p1.FromDate &&
                                p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid != 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.DestinationId == destinationid &&
                                p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                    }
                    else
                    {
                        if (shippingid == 0 && destinationid == 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.Lorry == lorryid &&
                                p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid == 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ProductId == productid &&
                            p.Lorry == lorryid && p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid != 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.DestinationId == destinationid &&
                                p.ProductId == productid && p.Lorry == lorryid && p.EntryDate >= p1.FromDate &&
                                p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid != 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.DestinationId == destinationid && p.ProductId == productid && p.Lorry == lorryid &&
                                p.EntryDate >= p1.FromDate && p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid == 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.Lorry == lorryid && p.EntryDate >= p1.FromDate &&
                                p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid != 0 && destinationid == 0 && productid != 0)
                        {
                            td =await _context.TripTables.Where(p => p.ShippingId == shippingid &&
                                p.ProductId == productid && p.Lorry == lorryid && p.EntryDate >= p1.FromDate &&
                                p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                        if (shippingid == 0 && destinationid != 0 && productid == 0)
                        {
                            td =await _context.TripTables.Where(p => p.DestinationId == destinationid &&
                                p.Lorry == lorryid && p.EntryDate >= p1.FromDate &&
                                p.EntryDate <= p1.ToDate).ToListAsync();
                        }
                    }
                }
                foreach (var item in td)
                {
                    obj_return.TripList.Add(new TripDto(item, _context));
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

        public class FindByTripIdParam
        {
            public long? TripId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_VIEW")]
        public async Task<IActionResult> FindByTripId([FromBody] FindByTripIdParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<TripTable> list1 = await _context.TripTables.Where(p => p.TripId == p1.TripId).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.TripList.Add(new TripDto(item, _context));
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

        public class FindByInvoiceNoParam
        {
            public int? InvoiceNo { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_VIEW")]
        public async Task<IActionResult> FindByInvoiceNo([FromBody] FindByInvoiceNoParam p1)
        {
            FilterByDateRangeReturn obj_return = new FilterByDateRangeReturn();
            try
            {
                List<TripTable> list1 = await _context.TripTables
                    .Where(p => p.TokenNo == p1.InvoiceNo).ToListAsync();
                foreach (var item in list1)
                {
                    obj_return.TripList.Add(new TripDto(item, _context));
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
        public class FilterByDateRangeReturn
        {
            public FilterByDateRangeReturn()
            {
                TripList = new List<TripDto>();
            }
            public string Message { get; set; }
            public List<TripDto> TripList { get; set; }
        }
        
        //Delete
        public class DeleteTripParam
        {
            public long? TripId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,TRIP_DELETE")]
        public async Task<IActionResult> DeleteTrip([FromBody] DeleteTripParam p1)
        {
            DeleteTripReturn obj_return = new DeleteTripReturn();
            try
            {
                TripTable? tt =await _context.TripTables.Where(p => p.TripId == p1.TripId).FirstOrDefaultAsync();
                if (tt == null)
                {
                    throw new Exception("Oops! Trip not found.");
                }
                _context.TripTables.Remove(tt);

                var journaldata =await _context.JournalTables.Where(p => p.TripId == p1.TripId).ToListAsync();
                foreach (var item in journaldata)
                {
                    _context.JournalTables.Remove(item);
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
        public class DeleteTripReturn
        {
            public string Message { get; set; }
        }

    }
}
