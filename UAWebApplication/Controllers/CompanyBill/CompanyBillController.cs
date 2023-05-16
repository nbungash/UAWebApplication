using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize]
    public class CompanyBillController : Controller
    {
        private readonly UADbContext _context;
        public CompanyBillController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_VIEW")]
        public ActionResult CompanyBill()
        {
            return View("~/Views/CompanyBill/ViewBill.cshtml");
        }

        //View Company Bill
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_VIEW")]
        public async Task<IActionResult> Years()
        {
            YearsReturnClass obj_return = new YearsReturnClass();
            try
            {
                List<int> year_list = await _context.PartyBillTables.Where(p => p.BillDate != null)
                    .Select(p => p.BillDate.Value.Year).Distinct().OrderByDescending(p => p).ToListAsync();
                obj_return.YearList = year_list;
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
        public class YearsReturnClass
        {
            public YearsReturnClass()
            {
                YearList = new List<int>();
            }
            public string? Message { get; set; }
            public List<int> YearList { get; set; }
        }

        public class BillListParam
        {
            public int? CompanyId { get; set; }
            public long? Year { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_VIEW")]
        public async Task<IActionResult> BillList([FromBody] BillListParam p1)
        {
            BillListReturnClass obj_return = new BillListReturnClass();
            try
            {

                List<PartyBillTable> list = await _context.PartyBillTables.Where(p => p.PartyId == p1.CompanyId &&
                    p.BillDate.Value.Year == p1.Year).OrderByDescending(p => p.BillNo).ToListAsync();
                foreach (var item in list)
                {
                    PartyBillClass obj = new PartyBillClass();
                    obj.Id = item.Id;
                    obj.BillNo = item.BillNo;
                    obj.BillDate = item.BillDate;
                    obj.IsFinalized = item.IsFinalized;
                    if (item.IsFinalized == true)
                    {
                        obj.IsChecked = "checked";
                    }
                    else
                    {
                        obj.IsChecked = "";
                    }
                    obj_return.PartyBillList.Add(obj);
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
        public class BillListReturnClass
        {
            public BillListReturnClass()
            {
                PartyBillList = new List<PartyBillClass>();
            }
            public string Message { get; set; }
            public List<PartyBillClass> PartyBillList { get; set; }
        }
        public class PartyBillClass
        {
            public long Id { get; set; }
            public long? PartyId { get; set; }
            public string? BillNo { get; set; }
            public DateTime? BillDate { get; set; }
            public bool? IsFinalized { get; set; }
            public string? IsChecked { get; set; }
        }

        //Delete company bill
        public class DeletePartyBillParam
        {
            public long BillId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_DELETE")]
        public async Task<ActionResult> DeletePartyBill([FromBody] DeletePartyBillParam p1)
        {
            DeletePartyBillReturn obj_return = new DeletePartyBillReturn();
            try
            {

                PartyBillTable? pbt = await _context.PartyBillTables.Where(p => p.Id == p1.BillId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Record not found");
                }
                List<TripTable> trip_list = await _context.TripTables.Where(p => p.PartyBillId == p1.BillId).ToListAsync();
                foreach (var item in trip_list)
                {
                    item.PartyBillId = null;
                }
                List<JournalTable> journal_list = await _context.JournalTables.Where(p => p.PartyBillId == p1.BillId).ToListAsync();
                foreach (var item in journal_list)
                {
                    item.PartyBillId = null;
                }
                List<SalesTaxInvoicesTable> sales_tax_invoices_list = await _context.SalesTaxInvoicesTables
                    .Where(p => p.PartyBillId == p1.BillId).ToListAsync();
                foreach (var item in sales_tax_invoices_list)
                {
                    item.PartyBillId = null;
                }
                _context.PartyBillTables.Remove(pbt);
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
        public class DeletePartyBillReturn
        {
            public DeletePartyBillReturn()
            {

            }
            public string Message { get; set; }
        }

        //New Company Bill
        public class NewCompanyBillWindowLoadedParam
        {
            public long? BillId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_UPDATE")]
        public async Task<IActionResult> NewCompanyBillWindowLoaded([FromBody] NewCompanyBillWindowLoadedParam p1)
        {
            NewCompanyBillWindowLoadedReturn obj_return = new NewCompanyBillWindowLoadedReturn();
            try
            {
                PartyBillTable? obj = await _context.PartyBillTables.Where(p => p.Id == p1.BillId).FirstOrDefaultAsync();
                if (obj == null)
                {
                    throw new Exception("Oops! Record not found.");
                }
                obj_return.ObjToUpdate = new PartyBillClass { Id = obj.Id, BillDate = obj.BillDate, BillNo = obj.BillNo, PartyId = obj.PartyId };
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
        public class NewCompanyBillWindowLoadedReturn
        {
            public NewCompanyBillWindowLoadedReturn()
            {
                ObjToUpdate = new PartyBillClass();
            }
            public string? Message { get; set; }
            public PartyBillClass ObjToUpdate { get; set; }
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_ADD")]
        public async Task<IActionResult> SaveBill([FromBody] PartyBillClass p1)
        {
            SaveBillReturn obj_return = new SaveBillReturn();
            try
            {
                if (p1.Id == 0)
                {
                    PartyBillTable obj = new PartyBillTable();
                    obj.BillNo = p1.BillNo;
                    obj.BillDate = p1.BillDate;
                    obj.PartyId = p1.PartyId;
                    _context.PartyBillTables.Add(obj);
                }
                else
                {
                    PartyBillTable? obj = await _context.PartyBillTables.Where(p => p.Id == p1.Id).FirstOrDefaultAsync();
                    if (obj == null)
                    {
                        throw new Exception("Oops! Record not found.");
                    }
                    obj.BillNo = p1.BillNo;
                    obj.BillDate = p1.BillDate;
                    obj.PartyId = p1.PartyId;
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
        public class SaveBillReturn
        {
            public SaveBillReturn()
            {
            }
            public string? Message { get; set; }
        }

        //Trips
        public class ViewTripsParam
        {
            public long? BillId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_VIEW")]
        public async Task<IActionResult> ViewTrips([FromBody] ViewTripsParam p1)
        {
            ViewTripsReturn obj_return = new ViewTripsReturn();
            try
            {
                PartyBillTable? pbt = await _context.PartyBillTables.Where(p => p.Id == p1.BillId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Bill not found.");
                }
                List<TripTable> billedList = await _context.TripTables.Where(p => p.PartyBillId == p1.BillId)
                    .OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in billedList)
                {
                    obj_return.TripList.Add(new TripDto(_context, item, "checked"));
                }
                List<TripTable> unBilledList = await _context.TripTables.Where(p => p.PartyBillId == null &&
                    p.PartyId == pbt.PartyId).OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in unBilledList)
                {
                    obj_return.TripList.Add(new TripDto(_context, item, ""));
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
        public class ViewTripsReturn
        {
            public ViewTripsReturn()
            {
                TripList = new List<TripDto>();
            }
            public string Message { get; set; }
            public List<TripDto> TripList { get; set; }
        }
        public class TripDto : TripTable
        {
            public TripDto() { }
            public TripDto(UADbContext context, TripTable tt, string isBilled)
            {
                this.IsBilled = isBilled;
                this.TripId = tt.TripId;
                this.TokenNo = tt.TokenNo;
                this.Lorry = tt.Lorry;
                this.LorryTitle = context.AccountTables.Where(p => p.AccountId == tt.Lorry).Select(p => p.Title).FirstOrDefault();
                this.InvoiceDate = tt.InvoiceDate;
                this.EntryDate = tt.EntryDate;
                this.Quantity = tt.Quantity;
                this.Freight = tt.Freight;
                this.ShippingId = tt.ShippingId;
                this.ShippingTitle = context.ShippingTables.Where(p => p.Id == tt.ShippingId)
                    .Select(p => p.Title).FirstOrDefault();
                this.DestinationId = tt.DestinationId;
                this.DestinationTitle = context.DestinationTables.Where(p => p.Id == tt.DestinationId)
                    .Select(p => p.Title).FirstOrDefault();
                this.ProductId = tt.ProductId;
                this.ProductTitle = context.ProductTables.Where(p => p.Id == tt.ProductId)
                    .Select(p => p.Title).FirstOrDefault();
                this.PartyBillId = tt.PartyBillId;
                this.PartyId = tt.PartyId;

            }
            public string? IsBilled { get; set; }
            public string? ShippingTitle { get; set; }
            public string? DestinationTitle { get; set; }
            public string? ProductTitle { get; set; }
            public string? LorryTitle { get; set; }
        }

        public class SaveTripsParam
        {
            public SaveTripsParam()
            {
                TripList = new List<TripDto>();
            }
            public long? PartyBillId { get; set; }
            public List<TripDto> TripList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_ADD")]
        public async Task<IActionResult> SaveTrips([FromBody] SaveTripsParam p1)
        {
            SaveTripsReturn obj_return = new SaveTripsReturn();
            try
            {
                PartyBillTable? pbt = await _context.PartyBillTables.Where(p => p.Id == p1.PartyBillId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party Bill not found.");
                }
                List<TripTable> tripList = await _context.TripTables.Where(p => p.PartyBillId == p1.PartyBillId).ToListAsync();
                foreach (var trip in tripList)
                {
                    trip.PartyBillId = null;
                }
                if (p1.TripList != null)
                {
                    foreach (var trip in p1.TripList)
                    {
                        TripTable? tt = await _context.TripTables.Where(p => p.TripId == trip.TripId).FirstOrDefaultAsync();
                        if (tt == null)
                        {
                            throw new Exception(string.Format("Oops! Trip bearing Id {0} not found", trip.TripId));
                        }
                        tt.PartyBillId = pbt.Id;
                    }
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
        public class SaveTripsReturn
        {
            public SaveTripsReturn()
            {
            }
            public string? Message { get; set; }
        }

        //Print
        public class PSOBillTripClass
        {
            public int? SNo { get; set; }
            public string? LorryNo { get; set; }
            public string? Product { get; set; }
            public string? Quantity { get; set; }
            public DateTime? InvoiceDate { get; set; }
            public decimal? Amount { get; set; }
            public string? TokenNo { get; set; }
            public string? Shipping { get; set; }
            public string? Destination { get; set; }
            public string? InvoiceNo { get; set; }
        }
        public class PrintPSOBillParam
        {
            public long BillId { get; set; }

        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,PSOBILL_PRINT")]
        public async Task<ContentResult> PSOBillPreview([FromBody] PrintPSOBillParam p1)
        {
            string base64EncodedPDF = "";
            string message = "";
            try
            {
                PartyBillTable? pbt = _context.PartyBillTables.Where(p => p.Id == p1.BillId).FirstOrDefault();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party Bill not found.");
                }
                string? CompanyTitle = "UNITED AZAD TRASPORT CORPORATION";
                var NewCode = 7773001405;
                var Address = "LS";
                var NTN = "NTN NO";
                List<TripTable> TripList = await _context.TripTables.Where(p => p.PartyBillId == p1.BillId).ToListAsync();
                var SumFrieght = TripList.Select(p => p.Freight).Sum().GetValueOrDefault();
                int count = 0;
                List<PSOBillTripClass> tripList = new List<PSOBillTripClass>();
                foreach (var item in TripList)
                {
                    tripList.Add(new PSOBillTripClass
                    {
                        SNo = ++count,
                        LorryNo = await _context.AccountTables.Where(p => p.AccountId == item.Lorry).Select(p => p.Title).FirstOrDefaultAsync(),
                        Product = await _context.ProductTables.Where(p => p.Id == item.ProductId).Select(p => p.Title).FirstOrDefaultAsync(),
                        Quantity = string.Format("{0}", item.Quantity),
                        InvoiceDate = item.InvoiceDate,
                        Amount = item.Freight,
                        TokenNo = string.Format("{0}", item.TokenNo),
                        Shipping = await _context.ShippingTables.Where(p => p.Id == item.ShippingId).Select(p => p.Title).FirstOrDefaultAsync(),
                        Destination = await _context.DestinationTables.Where(p => p.Id == item.ShippingId).Select(p => p.Title).FirstOrDefaultAsync(),
                        InvoiceNo = string.Format("{0}", item.TokenNo)
                    });
                }

                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var wri = new PdfWriter(stream))
                using (var pdf = new PdfDocument(wri))
                {
                    using (var doc = new iText.Layout.Document(pdf, PageSize.A4))
                    {
                        PdfFont NormalBoldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        PdfFont NormalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                        PdfFont NewTimesFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                        PdfFont NewTimesBoldFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);

                        doc.SetFont(NewTimesFont).SetMargins(250, 40, 40, 40);
                        Table header_table = new Table(UnitValue.CreatePercentArray(new float[] { 100 }));
                        header_table.SetFont(NewTimesFont);
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Submitted Invoices").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(28)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph(string.Format("Name :{0}", CompanyTitle)).SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(12)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph(string.Format("Vendor Code : {0}", NewCode)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(12)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph(string.Format("Invoice ID :{0}", pbt.BillNo)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(12)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph(string.Format("Date : {0:dd-MMM-yyyy}", pbt.BillDate)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(12)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph(string.Format("Address : {0}", Address)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(12)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph(string.Format("NTN NO : {0}", NTN)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(12)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Shipment Status: Online Acknowledge").SetTextAlignment(TextAlignment.CENTER).SetFontSize(12)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("").SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("").SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER));


                        //Page Footer
                        Table footer_table = new Table(3);
                        footer_table.AddCell(new Cell(1, 3)
                            .Add(new Paragraph(string.Format("Printed By {0} on {1}", User.Identity.Name, DateTime.Now.AddHours(5)))
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(6)).SetBorder(Border.NO_BORDER));


                        // create a HeaderFooterEventHandler instance with the table as its parameter
                        IEventHandler handler = new HeaderFooterEventHandler(header_table, footer_table, 250, 40, 40, 40);

                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);
                        // Add data to the table
                        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 10, 10, 10, 10, 10, 10, 10, }));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Sr.").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(12)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Shipment Number").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(10)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Delivery Date").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(10)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Delivery Number").SetBold().SetFontSize(10).SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Vehicle Text").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(10)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Quantity").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(10)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Material").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(10)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Amount").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFontSize(10)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));

                        foreach (var item in tripList)
                        {
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.SNo)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.TokenNo)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:dd-MMM-yyyy}", item.InvoiceDate)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.InvoiceNo)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.LorryNo)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.Quantity)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.Product)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.Amount)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        }
                        table.AddCell(new Cell(1, 6).Add(new Paragraph()).SetBorder(Border.NO_BORDER));
                        table.AddCell(new Cell(1, 2).Add(new Paragraph(string.Format("Grand Total:{0}", SumFrieght)).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(Border.NO_BORDER));

                        // Add the table to the document
                        doc.Add(table);

                        doc.Close();
                        doc.Flush();
                        pdfBytes = stream.ToArray();
                        base64EncodedPDF = System.Convert.ToBase64String(pdfBytes);
                    }
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
            return Content(base64EncodedPDF);
        }


        //Finalize Company Bill
        public class ChangeCompanyBillStatusParam
        {
            public long? CompanyBillId { get; set; }
            public bool? IsFinalized { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,COMPANY_BILL_FREEZE")]
        public async Task<IActionResult> ChangeCompanyBillStatus([FromBody] ChangeCompanyBillStatusParam p2)
        {
            ChangeCompanyBillStatusReturn obj = new ChangeCompanyBillStatusReturn();
            try
            {
                PartyBillTable? pbt = await _context.PartyBillTables.Where(p => p.Id == p2.CompanyBillId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party Bill not found.");
                }
                if (p2.IsFinalized == true)
                {
                    pbt.IsFinalized = true;
                    //TransactionID calculation
                    long? transID = _context.JournalTables.Select(p => p.TransId).Max() + 1;
                    List<TripTable> TripList = await _context.TripTables.Where(p => p.PartyBillId == pbt.Id).ToListAsync();
                    decimal? totalFreight = TripList.Sum(p => p.Freight).GetValueOrDefault(0);
                    if (totalFreight != null & totalFreight != 0)
                    {
                        JournalTable j1 = new JournalTable();
                        j1.AccountId = pbt.PartyId;
                        j1.Credit = null;
                        j1.Debit = totalFreight;
                        j1.Description = string.Format("PARTY BILL # {0} FREIGHT", pbt.BillNo);
                        j1.EntryDate = pbt.BillDate;
                        j1.EntryType = "PARTY BILL";
                        j1.TransId = transID;
                        pbt.JournalTables.Add(j1);

                        foreach (var trip in TripList)
                        {
                            if (trip.Freight != null && trip.Freight != 0)
                            {
                                JournalTable j2 = new JournalTable();
                                j2.AccountId = trip.Lorry;
                                j2.Credit = trip.Freight;
                                j2.Debit = null;
                                j2.Description = string.Format("TRIP ID # {0} INVOICE DATE {1:dd/MM/yyyy} FREIGHT", trip.TripId, trip.InvoiceDate);
                                j2.EntryDate = pbt.BillDate;
                                j2.EntryType = "PARTY BILL";
                                j2.TransId = transID;
                                pbt.JournalTables.Add(j2);
                            }
                        }
                    }
                }
                else
                {
                    pbt.IsFinalized = false;
                    List<JournalTable> jt_list = await _context.JournalTables.Where(p => p.PartyBillId == pbt.Id && p.EntryType == "PARTY BILL")
                        .ToListAsync();
                    foreach (var item in jt_list)
                    {
                        _context.JournalTables.Remove(item);
                    }
                }
                await _context.SaveChangesAsync();
                obj.Message = "OK";


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
        public class ChangeCompanyBillStatusReturn
        {
            public string Message { get; set; }
        }
    }
}