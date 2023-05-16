
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
using Document = iText.Layout.Document;
using Paragraph = iText.Layout.Element.Paragraph;
using Table = iText.Layout.Element.Table;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,GENERAL_LEDGER_VIEW")]
    public class GeneralLedgerController : Controller
    {
        private readonly UADbContext _context;
        public GeneralLedgerController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,GENERAL_LEDGER_VIEW")]
        public ActionResult GeneralLedger()
        {
            return View("~/Views/Books/GeneralLedger.cshtml");
        }

        //View
        public class ViewRecordsParam
        {
            public string? Group { get; set; }
            public long? AccountId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,GENERAL_LEDGER_VIEW")]
        public async Task<IActionResult> ViewRecords([FromBody] ViewRecordsParam p1)
        {
            ViewRecordsReturn obj_return = new ViewRecordsReturn();
            try
            {
                //Get Debit AccountId's
                var debitAccounts =await _context.AccountTables.Where(p => p.AccountType == "ASSET" ||
                    p.AccountType == "EXPENSE").Select(p => p.AccountId).ToListAsync();
                //Get Credit AccountId's
                var creditAccounts =await _context.AccountTables.Where(p => p.AccountType == "LIABILITY" ||
                    p.AccountType == "REVENUE" | p.AccountType == "CAPITAL").Select(p => p.AccountId).ToListAsync();
                if (debitAccounts.Contains(p1.AccountId.Value))
                {
                    obj_return.LedgerList.AddRange(GetDebitLedgerRecords(_context, p1.AccountId.Value,p1.FromDate,p1.ToDate));
                }
                else if (creditAccounts.Contains(p1.AccountId.Value))
                {
                    obj_return.LedgerList.AddRange(GetCreditLedgerRecords(_context,p1.AccountId.Value,p1.FromDate,p1.ToDate));
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
                LedgerList = new List<LedgerClass>();
            }
            public string Message { get; set; }
            public List<LedgerClass> LedgerList { get; set; }
        }

        public class LedgerClass
        {
            public long JournalId { get; set; }
            public long? TransID { get; set; }
            public long? TripID { get; set; }
            public string EntryType { get; set; }
            public DateTime? EntryDate { get; set; }
            public string Lorry { get; set; }
            public string Description { get; set; }
            public string ChequeNo { get; set; }
            public decimal? Debit { get; set; }
            public decimal? Credit { get; set; }
            public decimal? Balance { get; set; }
            public string DebitCredit { get; set; }
            public long? VoucherNo { get; set; }
        }
        public List<LedgerClass> GetDebitLedgerRecords(UADbContext context, long AccountId, DateTime? DateFrom, DateTime? DateTo)
        {
            List<LedgerClass> ledgerdata = new List<LedgerClass>();

            //Get Brought Forward Balance
            decimal openingBalance = 0;
            List<JournalTable> previousdata = context.JournalTables
                .Where(p => p.AccountId == AccountId & p.EntryDate < DateFrom).ToList();
            foreach (JournalTable item in previousdata)
            {
                openingBalance += (item.Debit ?? 0);
                openingBalance -= (item.Credit ?? 0);
            }
            LedgerClass openingBalanceObject = new LedgerClass();
            openingBalanceObject.EntryDate = DateFrom;
            openingBalanceObject.Description = "Balance Brought Forward";
            if (openingBalance < 0)
            {
                openingBalanceObject.Balance = Math.Abs(openingBalance);
                openingBalanceObject.DebitCredit = "Cr";
            }
            else if (openingBalance >= 0)
            {
                openingBalanceObject.Balance = openingBalance;
                openingBalanceObject.DebitCredit = "Dr";
            }
            ledgerdata.Add(openingBalanceObject);


            var journaldata = context.JournalTables.Where(p => p.AccountId == AccountId &&
                p.EntryDate >= DateFrom && p.EntryDate <= DateTo).OrderBy(p => p.EntryDate).ToList();
            foreach (var item in journaldata)
            {
                LedgerClass obj = new LedgerClass();
                obj.JournalId = item.Id;
                obj.TripID = item.TripId;
                obj.TransID = item.TransId;
                obj.EntryType = item.EntryType;
                obj.EntryDate = item.EntryDate;
                string lorry = "";
                if (item.Lorry != null)
                {
                    obj.Lorry = item.Lorry;
                }
                else if (item.TripId != null)
                {
                    TripTable? tt = context.TripTables
                        .Where(p => p.TripId == item.TripId).FirstOrDefault();
                    obj.Lorry = context.AccountTables.Where(p => p.AccountId == tt.Lorry)
                        .Select(p => p.Title).FirstOrDefault();
                }
                else
                {
                    List<long?> lorryList = context.JournalTables.Where(p => p.Id != item.Id &&
                          p.TransId == item.TransId).Select(p => p.AccountId).ToList();
                    int count = 0;
                    foreach (var item2 in lorryList)
                    {
                        AccountTable? at = context.AccountTables.Where(p => p.AccountId == item2).FirstOrDefault();
                        if (count == 0)
                        {
                            lorry = at.Title;
                        }
                        else
                        {
                            lorry = String.Format("{0},{1}", lorry, at.Title);
                        }
                        count++;
                    }
                    obj.Lorry = lorry;
                }
                obj.Description = item.Description;
                obj.Debit = item.Debit;
                obj.Credit = item.Credit;
                obj.ChequeNo = item.ChequeNo;
                obj.VoucherNo = item.VoucherNo;
                openingBalance += ((item.Debit ?? 0) - (item.Credit ?? 0));
                if (openingBalance < 0)
                {
                    obj.Balance = Math.Abs(openingBalance);
                    obj.DebitCredit = "Cr";
                }
                else if (openingBalance >= 0)
                {
                    obj.Balance = openingBalance;
                    obj.DebitCredit = "Dr";
                }
                ledgerdata.Add(obj);
            }
            LedgerClass totalBalanceObject = new LedgerClass();
            totalBalanceObject.Description = "TOTAL";
            decimal? totalCredit = ledgerdata.Sum(p => p.Credit).GetValueOrDefault(0);
            decimal? totalDebit = ledgerdata.Sum(p => p.Debit).GetValueOrDefault(0);
            totalBalanceObject.Debit = totalDebit;
            totalBalanceObject.Credit = totalCredit;
            ledgerdata.Add(totalBalanceObject);

            return ledgerdata;
        }
        public List<LedgerClass> GetCreditLedgerRecords(UADbContext context, long AccountId, DateTime? DateFrom, DateTime? DateTo)
        {
            List<LedgerClass> ledgerdata = new List<LedgerClass>();

            //Get Brought Forward Balance
            decimal openingBalance = 0;
            List<JournalTable> previousdata = context.JournalTables
                .Where(p => p.AccountId == AccountId & p.EntryDate < DateFrom).ToList();
            foreach (JournalTable item in previousdata)
            {
                openingBalance += (item.Credit ?? 0);
                openingBalance -= (item.Debit ?? 0);
            }
            LedgerClass openingBalanceObject = new LedgerClass();
            openingBalanceObject.EntryDate = DateFrom;
            openingBalanceObject.Description = "Balance Brought Forward";
            if (openingBalance < 0)
            {
                openingBalanceObject.Balance = Math.Abs(openingBalance);
                openingBalanceObject.DebitCredit = "Dr";
            }
            else if (openingBalance >= 0)
            {
                openingBalanceObject.Balance = openingBalance;
                openingBalanceObject.DebitCredit = "Cr";
            }
            ledgerdata.Add(openingBalanceObject);

            var journaldata = context.JournalTables
                        .Where(p => p.AccountId == AccountId && p.EntryDate >= DateFrom && p.EntryDate <= DateTo)
                        .OrderBy(p => p.EntryDate).ToList();
            foreach (var item in journaldata)
            {
                LedgerClass obj = new LedgerClass();
                obj.JournalId = item.Id;
                obj.TripID = item.TripId;
                obj.TransID = item.TransId;
                obj.EntryType = item.EntryType;
                obj.EntryDate = item.EntryDate;
                string lorry = "";
                if (item.Lorry != null)
                {
                    obj.Lorry = item.Lorry;
                }
                else if (item.TripId != null)
                {
                    TripTable tt = context.TripTables
                        .Where(p => p.TripId == item.TripId).FirstOrDefault();
                    obj.Lorry = context.AccountTables.Where(p => p.AccountId == tt.Lorry)
                        .Select(p => p.Title).FirstOrDefault();
                }
                else
                {
                    List<long?> lorryList = context.JournalTables.Where(p => p.Id != item.Id &&
                          p.TransId == item.TransId).Select(p => p.AccountId).ToList();
                    int count = 0;
                    foreach (var item2 in lorryList)
                    {
                        AccountTable at = context.AccountTables
                            .Where(p => p.AccountId == item2).FirstOrDefault();
                        if (count == 0)
                        {
                            lorry = at.Title;
                        }
                        else
                        {
                            lorry = String.Format("{0},{1}", lorry, at.Title);
                        }
                        count++;
                    }
                    obj.Lorry = lorry;
                }
                obj.Description = item.Description;
                obj.Debit = item.Debit;
                obj.Credit = item.Credit;
                obj.ChequeNo = item.ChequeNo;
                obj.VoucherNo = item.VoucherNo;
                openingBalance += ((item.Credit ?? 0) - (item.Debit ?? 0));
                if (openingBalance < 0)
                {
                    obj.Balance = Math.Abs(openingBalance);
                    obj.DebitCredit = "Dr";
                }
                else if (openingBalance >= 0)
                {
                    obj.Balance = openingBalance;
                    obj.DebitCredit = "Cr";
                }
                ledgerdata.Add(obj);
            }
            LedgerClass totalBalanceObject = new LedgerClass();
            totalBalanceObject.Description = "TOTAL";
            decimal? totalCredit = ledgerdata.Sum(p => p.Credit).GetValueOrDefault(0);
            decimal? totalDebit = ledgerdata.Sum(p => p.Debit).GetValueOrDefault(0);
            totalBalanceObject.Debit = totalDebit;
            totalBalanceObject.Credit = totalCredit;
            ledgerdata.Add(totalBalanceObject);

            return ledgerdata;
        }

        //Print General Ledger
        public class ReportPreviewParam
        {
            public int? AccountId { get; set; }
            public string? AccountTitle { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,GENERAL_LEDGER_PRINT")]
        public async Task<IActionResult> ReportPreview([FromBody] ReportPreviewParam p1)
        {
            string base64EncodedPDF = "";
            string message = "";
            try
            {
                List<LedgerClass> LedgerList = new List<LedgerClass>();
                List<LedgerSummaryClass> lsc_list = new List<LedgerSummaryClass>();

                
                    //Get Debit AccountId's
                    List<long> debitAccounts =await _context.AccountTables
                        .Where(p => p.AccountType == "ASSET" || p.AccountType == "EXPENSE").Select(p => p.AccountId).ToListAsync();
                    //Get Credit AccountId's
                    List<long> creditAccounts =await _context.AccountTables
                        .Where(p => p.AccountType == "LIABILITY" || p.AccountType == "REVENUE" || p.AccountType == "CAPITAL")
                        .Select(p => p.AccountId).ToListAsync();
                    if (debitAccounts.Contains(p1.AccountId.Value))
                    {
                        LedgerList = GetDebitLedgerRecords(_context, p1.AccountId.Value, p1.FromDate, p1.ToDate);
                    }
                    else if (creditAccounts.Contains(p1.AccountId.Value))
                    {
                        LedgerList = GetCreditLedgerRecords(_context, p1.AccountId.Value, p1.FromDate, p1.ToDate);
                    }

                    LedgerClass first_record = LedgerList.Where(p => p.Description == "Balance Brought Forward").First();
                    LedgerSummaryClass opening_balance = new LedgerSummaryClass();
                    opening_balance.Detail = "Opening Balance : ";
                    opening_balance.Balance = first_record.Balance.GetValueOrDefault(0);
                    opening_balance.DrCr = first_record.DebitCredit;
                    lsc_list.Add(opening_balance);

                    LedgerClass last_record = LedgerList
                        .Where(p => p.Description == "TOTAL").First();
                    LedgerSummaryClass total_debit_balance = new LedgerSummaryClass();
                    total_debit_balance.Detail = "Total Debit : ";
                    total_debit_balance.Balance = last_record.Debit.GetValueOrDefault(0);
                    total_debit_balance.DrCr = "Dr";
                    lsc_list.Add(total_debit_balance);

                    LedgerSummaryClass total_credit_balance = new LedgerSummaryClass();
                    total_credit_balance.Detail = "Total Credit : ";
                    total_credit_balance.Balance = last_record.Credit.GetValueOrDefault(0);
                    total_credit_balance.DrCr = "Cr";
                    lsc_list.Add(total_credit_balance);

                    LedgerClass? second_last_record = LedgerList
                        .Where(p => p.Description != "B/F" && p.Description != "TOTAL")
                        .LastOrDefault();
                    if (second_last_record != null)
                    {
                        LedgerSummaryClass balance = new LedgerSummaryClass();
                        balance.Detail = "Running Balance : ";
                        balance.Balance = second_last_record.Balance.GetValueOrDefault(0);
                        balance.DrCr = second_last_record.DebitCredit;
                        lsc_list.Add(balance);
                    }
                

                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var wri = new PdfWriter(stream))
                using (var pdf = new PdfDocument(wri))
                {
                    using (var doc = new Document(pdf, PageSize.A4))
                    {
                        doc.SetMargins(90, 40, 40, 40);

                        //Page Header
                        Table header_table = new Table(UnitValue.CreatePercentArray(new float[] { 10, 30, 60 }));
                        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        header_table.AddCell(new Cell(1, 3).Add(new Paragraph("UNITED AZAD TRANSPORT CO").SetFont(font).SetFontSize(15)).SetBorder(Border.NO_BORDER));
                        header_table.AddCell(new Cell(2, 1).Add(new Paragraph("GENERAL LEDGER").SetFont(font).SetFontSize(15)).SetBorder(Border.NO_BORDER));
                        header_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("Account Title : {0}", p1.AccountTitle)).SetFontSize(8)).SetBorder(Border.NO_BORDER));
                        header_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("Date From {0:dd MMM,yyyy} To {1:dd MMM,yyyy}", p1.FromDate, p1.ToDate)).SetFontSize(8)).SetBorder(Border.NO_BORDER));
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
                        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 9, 30, 9, 9, 9, 9, 9, 11, 5 }));
                        table.SetWidth(UnitValue.CreatePercentValue(100)).SetFontSize(8);
                        table.SetBorder(new SolidBorder(0.1f));

                        table.AddHeaderCell(new Cell().Add(new Paragraph("Date").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Description").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("T/L").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Cheque").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Voucher").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Debit").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Credit").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Balance").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("").SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetBold().SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        foreach (var item in LedgerList)
                        {
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0:dd-MM-yy}", item.EntryDate))
                                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0}", item.Description))
                                .SetTextAlignment(TextAlignment.LEFT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0}", item.Lorry))
                                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0}", item.ChequeNo))
                                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0}", item.VoucherNo))
                                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0:n0}", item.Debit))
                                .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0:n0}", item.Credit))
                                .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0:n0}", item.Balance))
                                .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell().Add(new Paragraph(string.Format("{0}", item.DebitCredit))
                                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        }
                        doc.Add(table);

                        //Summary Table
                        Table summary_table = new Table(UnitValue.CreatePercentArray(new float[] { 60, 20, 10 }));
                        summary_table.SetWidth(UnitValue.CreatePercentValue(50)).SetFontSize(8).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                        summary_table.SetMargins(5, 0, 0, 0);

                        summary_table.AddHeaderCell(new Cell().Add(new Paragraph("Summary Detail").SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)).SetBold());
                        summary_table.AddHeaderCell(new Cell().Add(new Paragraph("Balance").SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)).SetBold());
                        summary_table.AddHeaderCell(new Cell().Add(new Paragraph("Dr Cr").SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)).SetBold());
                        foreach (var item in lsc_list)
                        {
                            summary_table.AddCell(new Cell().Add(new Paragraph(string.Format("{0}", item.Detail)).SetTextAlignment(TextAlignment.RIGHT)
                                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1))));
                            summary_table.AddCell(new Cell().Add(new Paragraph(string.Format("{0:n0}", item.Balance)).SetTextAlignment(TextAlignment.RIGHT)
                                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1))));
                            summary_table.AddCell(new Cell().Add(new Paragraph(string.Format("{0}", item.DrCr)).SetTextAlignment(TextAlignment.CENTER)
                                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1))));
                        }
                        doc.Add(summary_table);

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
        public class LedgerSummaryClass
        {
            public string Detail { get; set; }
            public decimal? Balance { get; set; }
            public string DrCr { get; set; }
        }
    }
}
