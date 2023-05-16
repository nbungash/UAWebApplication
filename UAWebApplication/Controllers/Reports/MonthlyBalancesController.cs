
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
using Microsoft.JSInterop.Implementation;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;
using Document = iText.Layout.Document;
using Paragraph = iText.Layout.Element.Paragraph;
using Table = iText.Layout.Element.Table;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,MONTHLY_BALANCES_VIEW")]
    public class MonthlyBalancesController : Controller
    {
        private readonly UADbContext _context;
        public MonthlyBalancesController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,MONTHLY_BALANCES_VIEW")]
        public ActionResult MonthlyBalances()
        {
            return View("~/Views/Reports/MonthlyBalances.cshtml");
        }

        //View
        public class ViewRecordsParam
        {
            public DateTime? Month { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,MONTHLY_BALANCES_VIEW")]
        public async Task<IActionResult> ViewRecords([FromBody] ViewRecordsParam p1)
        {
            ViewRecordsReturn obj_return = new ViewRecordsReturn();
            try
            {
                int day1 = 0;
                int month1 = p1.Month.Value.Month;
                int year1 = p1.Month.Value.Year;
                if (month1 == 9 || month1 == 4 || month1 == 6 || month1 == 11)
                {
                    day1 = 30;
                }
                else if (month1 == 1 || month1 == 3 || month1 == 5 || month1 == 7 || month1 == 8 || month1 == 10 || month1 == 12)
                {
                    day1 = 31;
                }
                else if (month1 == 2)
                {
                    if (year1 % 4 == 0)
                    {
                        day1 = 29;
                    }
                    else
                    {
                        day1 = 28;
                    }
                }
                DateTime date1 = new DateTime(year1, month1, day1);

                List<AccountTable> lorryList =await _context.AccountTables.Where(p => p.GroupType == "LORRY" && p.Record == true)
                    .OrderBy(p => p.Title).ToListAsync();
                int count = 0;
                foreach (var item in lorryList)
                {
                    MonthlyBalanceClass obj = new MonthlyBalanceClass();
                    obj.sno = ++count;
                    obj.Lorry = item.Title;
                    ABCClass abc = GetMonthlyBalance(_context, date1, item);
                    decimal? monthbalance = abc.Balance;
                    if (monthbalance < 0)
                    {
                        obj.MonthBalananceDebit = Math.Abs(monthbalance.GetValueOrDefault(0));
                    }
                    else
                    {
                        obj.MonthBalananceCredit = monthbalance.GetValueOrDefault(0);
                    }
                    obj.Commission = abc.Commission.GetValueOrDefault(0);
                    obj_return.RecordList.Add(obj);
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
        public class ViewRecordsReturn
        {
            public ViewRecordsReturn()
            {
                RecordList = new List<MonthlyBalanceClass>();
            }
            public string Message { get; set; }
            public List<MonthlyBalanceClass> RecordList { get; set; }
        }
        private ABCClass GetMonthlyBalance(UADbContext context, DateTime? date1,
            AccountTable at)
        {
            ABCClass obj = new ABCClass();
            decimal bfAmount = 0;
            decimal? commission = 0;
            List<LorryBillTable> lbtList = context.LorryBillTables.Where(p => p.BillDate <= date1 && p.Lorry == at.AccountId).ToList();

            foreach (var item in lbtList)
            {
                List<TripTable> tripList = context.TripTables.Where(p => p.LorryBillNo == item.BillNo).ToList();
                bfAmount += tripList.Sum(p => p.Freight).GetValueOrDefault(0);
                bfAmount -= tripList.Sum(p => p.Commission).GetValueOrDefault(0);
                if (item.BillDate.Value.Month == date1.Value.Month)
                {
                    commission += tripList.Sum(p => p.Commission).GetValueOrDefault(0);
                }
                bfAmount -= tripList.Sum(p => p.Tax).GetValueOrDefault(0);
                bfAmount -= tripList.Sum(p => p.ShortAmount).GetValueOrDefault(0);
                foreach (var trip in tripList)
                {
                    bfAmount -= context.JournalTables
                    .Where(p => p.TripId == trip.TripId && (p.EntryType == "GV" || p.EntryType == "TM") && p.AccountId == trip.Lorry)
                    .Sum(p => p.Debit).GetValueOrDefault(0);
                }
                //bfAmount += creditdata.Sum(p => p.Amount).GetValueOrDefault(0);
                bfAmount -= context.JournalTables.Where(r => r.LorryBillNo == item.BillNo && r.EntryType == "GV" && r.TripId == null &&
                            r.Debit != null & r.Debit != 0 && r.AccountId == item.Lorry).Sum(p => p.Debit).GetValueOrDefault(0);
                bfAmount += context.JournalTables.Where(r => r.LorryBillNo == item.BillNo && r.EntryType == "GV" && r.TripId == null &&
                    r.Credit != null & r.Credit != 0 && r.AccountId == item.Lorry).Sum(p => p.Credit).GetValueOrDefault(0);

                //bfAmount -= context.JournalTables.Where(r => r.LorryBillNo == item.BillNo && r.EntryType == "GV" &&
                //    r.Debit != null & r.Debit != 0 && r.AccountId == item.Lorry).Sum(p => p.Debit).GetValueOrDefault(0);
                //bfAmount += context.JournalTables.Where(r => r.LorryBillNo == item.BillNo && r.EntryType == "GV" &&
                //    r.Credit != null & r.Credit != 0 && r.AccountId == item.Lorry).Sum(p => p.Credit).GetValueOrDefault(0);
                bfAmount -= item.BillCharges.GetValueOrDefault(0);
            }
            obj.Balance = bfAmount;
            obj.Commission = commission;
            return obj;
        }

        public class ABCClass
        {
            public decimal? Balance { get; set; }
            public decimal? Commission { get; set; }
        }
        public class MonthlyBalanceClass
        {
            public int? sno { get; set; }
            public string? Lorry { get; set; }
            public decimal? Commission { get; set; }
            public decimal? MonthBalananceDebit { get; set; }
            public decimal? MonthBalananceCredit { get; set; }
        }

        //Print General Ledger
        public class ReportPreviewParam
        {
            public ReportPreviewParam()
            {
                RecordsList = new List<MonthlyBalanceClass>();
            }
            public DateTime? Month { get; set; }
            public List<MonthlyBalanceClass> RecordsList { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,MONTHLY_BALANCES_PRINT")]
        public async Task<IActionResult> ReportPreview([FromBody] ReportPreviewParam p1)
        {
            string base64EncodedPDF = "";
            string message = "";
            try
            {
                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var wri = new PdfWriter(stream))
                using (var pdf = new PdfDocument(wri))
                {
                    using (var doc = new Document(pdf, PageSize.A4))
                    {
                        doc.SetMargins(90, 40, 40, 40);

                        //Page Header
                        Table header_table = new Table(UnitValue.CreatePercentArray(new float[] { 100 }));
                        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        header_table.AddCell(new Cell(1, 1).Add(new Paragraph("T/L MONTHLY LOAN BALANCES").SetTextAlignment(TextAlignment.CENTER).SetFont(font).SetFontSize(15)).SetBorder(Border.NO_BORDER));
                        header_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("Month {0:MMM,yyyy}", p1.Month)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(8)).SetBorder(Border.NO_BORDER));
                        header_table.SetBorder(Border.NO_BORDER);

                        //Page Footer
                        Table footer_table = new Table(3);
                        footer_table.AddCell(new Cell(1, 3)
                            .Add(new Paragraph(string.Format("Printed By {0} on {1}", User.Identity.Name, DateTime.Now.AddHours(5)))
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(6)).SetBorder(Border.NO_BORDER));
                        // create a HeaderFooterEventHandler instance with the table as its parameter
                        IEventHandler handler = new HeaderFooterEventHandler(header_table, footer_table,
                            90, 40, 40, 40);
                        // set the event handler to the document renderer
                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

                        //Ledger Table
                        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 20, 20, 20, 20, 20}));
                        table.SetWidth(UnitValue.CreatePercentValue(100)).SetFontSize(8);
                        table.SetBorder(new SolidBorder(0.1f));

                        table.AddHeaderCell(new Cell(1,1).Add(new Paragraph("S No").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Lorry").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Commission").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Loan").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Bachat").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        foreach (var item in p1.RecordsList)
                        {
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.sno))
                                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.Lorry))
                                .SetTextAlignment(TextAlignment.LEFT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n0}", item.Commission))
                                .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n0}", item.MonthBalananceDebit))
                                .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n0}", item.MonthBalananceCredit))
                                .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        }
                        table.AddCell(new Cell(1, 1).Add(new Paragraph("")
                                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddCell(new Cell(1, 1).Add(new Paragraph("TOTAL")
                            .SetTextAlignment(TextAlignment.LEFT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n0}", p1.RecordsList.Sum(p=>p.Commission).GetValueOrDefault(0)))
                            .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n0}", p1.RecordsList.Sum(p => p.MonthBalananceDebit).GetValueOrDefault(0)))
                            .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n0}", p1.RecordsList.Sum(p => p.MonthBalananceCredit).GetValueOrDefault(0)))
                            .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
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
        
    }
}
