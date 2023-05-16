using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.Kernel.Events;
using iText.Kernel.Colors;
using iText.Layout.Renderer;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,PSO_SUMMARY_VIEW")]
    public class PSOSummaryController : Controller
    {
        private readonly UADbContext _context;
        public PSOSummaryController(UADbContext context)
        {
            this._context = context;
        }

        // View
        [Authorize]
        public ActionResult PSOSummary()
        {
            return View("~/Views/CompanyBill/PSOSummary.cshtml");
        }
              
        public class PSOBillSummaryDateParamClass
        {
            public int? BillYear { get; set; }
        }
        public async Task<ActionResult> BillDateList([FromBody] PSOBillSummaryDateParamClass p1)
        {
            DateListReturnClass obj_return = new DateListReturnClass();
            try
            {
                _context.Database.SetCommandTimeout(300);
                var BillDate =await _context.PartyBillTables.Where(p => p.BillDate.Value.Year == p1.BillYear)
                    .Select(p => string.Format("{0:dd-MMM-yyyy}",p.BillDate)).Distinct().ToListAsync();

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
        [Authorize]
        public async Task<ActionResult> YearList()
        {
            YearListReturnClass obj_return = new YearListReturnClass();
            try
            {
                List<int> years = _context.PartyBillTables.Where(p => p.BillDate.Value.Year <= DateTime.Today.Year)
                    .Select(p => p.BillDate.Value.Year).Distinct().OrderByDescending(p => p).ToList();

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

        public class PSOBillSummaryClass
        {
            public string? BillDate { get; set; }
        }
        public async Task<ActionResult> SummaryView([FromBody] PSOBillSummaryClass p1)
        {
            SummaryViewReturnClass obj_return = new SummaryViewReturnClass();
            try
            {
                _context.Database.SetCommandTimeout(300);
                DateTime date = DateTime.ParseExact(p1.BillDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                List<PartyBillTable> Billist = await _context.PartyBillTables.Where(p => p.BillDate == date).ToListAsync();
                
                int count = 0;
                foreach(var item in Billist)
                {
                    SummaryViewClass obj = new SummaryViewClass();
                    obj.Sno = ++count;
                    obj.BillDate = string.Format("{0:dd-MMM-yyyy}", item.BillDate);
                    obj.BillNo = item.BillNo;
                    obj.Amount = _context.TripTables.Where(p => p.PartyBillId == item.Id).Sum(p => p.Freight).GetValueOrDefault(0);
                    obj_return.SummaryView.Add(obj);
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
        }




        public class PrintPSOSummaryParam
        {
            public string BillDate { get; set; }

        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,PSO_SUMMARY_PRINT")]
        public async Task<ContentResult> PSOSummaryPreview([FromBody] PrintPSOSummaryParam p1)
        {
            string base64EncodedPDF = "";
            string message = "";
            try
            {
                _context.Database.SetCommandTimeout(300);
                DateTime date = DateTime.ParseExact(p1.BillDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                string first_second = "";
                if (date.Day <= 15)
                {
                    first_second = "1st";
                }
                else
                {
                    first_second = "2nd";
                }
                List<SummaryPrintClass> summarydata = new List<SummaryPrintClass>();
                var list =await _context.PartyBillTables.Where(p => p.BillDate == date ).OrderBy(p => p.BillNo).ToListAsync();
                if (list.Count() != 0)
                {
                    foreach (var item in list)
                    {
                        SummaryPrintClass obj = new SummaryPrintClass();
                        obj.BillDate = item.BillDate;
                        obj.Billno = item.BillNo;
                        obj.BillAmount =await _context.TripTables.Where(p => p.PartyBillId == item.Id).SumAsync(p => p.Freight);
                        summarydata.Add(obj);
                    }
                }
                var SummBillAmount = summarydata.Sum(p => p.BillAmount).GetValueOrDefault(0);

                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var wri = new PdfWriter(stream))
                using (var pdf = new PdfDocument(wri))
                {
                    using (var doc = new iText.Layout.Document(pdf, PageSize.A4))
                    {
                        doc.SetMargins(200, 60, 40, 60);
                        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                        
                        Table header_table = new Table(UnitValue.CreatePercentArray(new float[] { 100 }));
                        header_table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("UNITED AZAD TRANSPORT CORPORATION").SetFont(font).SetBold()
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(20)).
                            Add(new Paragraph("8341").SetFont(font).SetBold()
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(13))
                            .Add(new Paragraph("7773001405").SetFont(font).SetBold()
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(13)
                            ).SetBorder(new SolidBorder(1)));
                        header_table.AddHeaderCell(new Cell(1, 1)
                            .Add(new Paragraph(string.Format("Fort Night : {0} FN {1:MMM yyyy}", first_second, date))
                                .SetFont(font)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20)
                                .SetBackgroundColor(ColorConstants.BLACK)
                                .SetFontColor(ColorConstants.WHITE))
                            .SetBorder(new SolidBorder(1)));

                        header_table.AddCell(new Cell(1, 1)).SetBorder(Border.NO_BORDER);
                        header_table.AddHeaderCell(new Cell(1, 1)
                            .Add(new Paragraph("BILL SUMMARY SHEET")
                                .SetFont(font)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20))
                            .SetBorder(new SolidBorder(1)));


                        //Page Footer
                        Table footer_table = new Table(3);
                        footer_table.AddCell(new Cell(1, 3)
                            .Add(new Paragraph(string.Format("Printed By {0} on {1}", User.Identity.Name, DateTime.Now.AddHours(5)))
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(6)).SetBorder(Border.NO_BORDER));


                        // create a HeaderFooterEventHandler instance with the table as its parameter
                        IEventHandler handler = new HeaderFooterEventHandler(header_table, footer_table, 200, 60, 40, 60);

                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);
                        // Add data to the table
                        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 33}));
                        table.SetWidth(UnitValue.CreatePercentValue(100));
                        table.SetMarginTop(10).SetBorder(Border.NO_BORDER);
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Bill Date").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font).SetFontSize(13)).SetBorder(new SolidBorder(1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Bill No").SetBold().SetTextAlignment(TextAlignment.CENTER).SetFont(font).SetFontSize(13)).SetBorder(new SolidBorder(1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph(" Amount").SetBold().SetFont(font).SetFontSize(13).SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));

                        foreach (var item in summarydata)
                        {
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:dd-MMM-yyyy}", item.BillDate)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10).SetBorder(new SolidBorder(1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.Billno)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10).SetBorder(new SolidBorder(1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n2}", item.BillAmount)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10).SetBorder(new SolidBorder(1)));
                        }

                        table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", "Total")).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10).SetBorder(new SolidBorder(1)));
                        table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", summarydata.Count())).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10).SetBorder(new SolidBorder(1)));
                        table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n2}", SummBillAmount)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(10).SetBorder(new SolidBorder(1)));

                        table.AddCell(new Cell(1, 3)).SetBorder(Border.NO_BORDER);

                        table.AddCell(new Cell(1, 3).Add(new Paragraph("Cartage Contractor Rubber Stamp & Signature").SetFont(font).SetVerticalAlignment(VerticalAlignment.BOTTOM).SetTextAlignment(TextAlignment.CENTER).SetHeight(150).SetFontSize(8)).SetBorder(new SolidBorder(1)));
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

        public class SummaryPrintClass
        {
            public DateTime? BillDate { get; set; }
            public string? Billno { get; set; }
            public decimal? BillAmount { get; set; }
        }
    }
}
