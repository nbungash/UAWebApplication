using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace UAWebApplication.Controllers
{
    [Authorize]
    public class NewSalesTaxInvoiceController : Controller
    {
        private readonly UADbContext _context;
        public NewSalesTaxInvoiceController(UADbContext context)
        {
            _context = context;
        }

        public class GetSalesTaxPerecentageParam
        {
            public int? ProvinceId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SALES_TAX_INVOICE_ADD")]
        public async Task<ActionResult> GetSalesTaxPerecentage([FromBody] GetSalesTaxPerecentageParam p1)
        {
            GetSalesTaxPerecentageReturn obj_return = new GetSalesTaxPerecentageReturn();
            try
            {
                ProvincesTable? pt = await _context.ProvincesTables.Where(p => p.Id == p1.ProvinceId).FirstOrDefaultAsync();
                if (pt == null)
                {
                    throw new Exception("Oops! Province not found.");
                }
                obj_return.SalesTaxPercentage = pt.InterProvinceSalesTax;
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
        public class GetSalesTaxPerecentageReturn
        {
            public GetSalesTaxPerecentageReturn()
            {
            }
            public string Message { get; set; }
            public decimal? SalesTaxPercentage { get; set; }
        }

        public class GenerateSalesTaxParam
        {
            public long? CompanyBillId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SALES_TAX_INVOICE_ADD")]
        public async Task<ActionResult> GenerateSalesTax([FromBody] GenerateSalesTaxParam p1)
        {
            GenerateSalesTaxReturn obj_return = new GenerateSalesTaxReturn();
            try
            {
                PartyBillTable? pbt = await _context.PartyBillTables.Where(p => p.Id == p1.CompanyBillId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party bill not found.");
                }
                decimal? total_Freight = await _context.TripTables.Where(p => p.PartyBillId == pbt.Id).SumAsync(p => p.Freight);

                obj_return.InvoiceDate = pbt.BillDate;
                List<SalesTaxInvoicesTable> list1 =await _context.SalesTaxInvoicesTables
                    .Where(p => p.PartyBillId == p1.CompanyBillId).ToListAsync();
                if (list1.Count != 0)
                {
                    SalesTaxInvoicesTable shippingInvoice = list1.Where(p => p.InvoiceProvinceId == p.ShippingProvinceId).First();
                    obj_return.InvoiceDate = shippingInvoice.InvoiceDate;
                    obj_return.ShippingProvinceId = shippingInvoice.ShippingProvinceId;
                    obj_return.ShippingInvoiceNo = shippingInvoice.InvoiceNo;
                    obj_return.ShippingSalesTaxPercentage = shippingInvoice.SalesTaxPercent;
                    if (list1.Count == 2)
                    {
                        SalesTaxInvoicesTable destinationInvoice = list1.Where(p => p.InvoiceProvinceId == p.DestinationProvinceId).First();
                        obj_return.DestinationInvoiceNo = destinationInvoice.InvoiceNo;
                        obj_return.DestinationProvinceId = destinationInvoice.DestinationProvinceId;
                        obj_return.DestinationSalesTaxPercentage = destinationInvoice.SalesTaxPercent;
                    }
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
        public class GenerateSalesTaxReturn
        {
            public GenerateSalesTaxReturn()
            {
            }
            public string Message { get; set; }
            public DateTime? InvoiceDate { get; set; }
            public int? CompanyId { get; set; }
            public int? ShippingProvinceId { get; set; }
            public int? DestinationProvinceId { get; set; }

            public string ShippingInvoiceNo { get; set; }
            public decimal? ShippingSalesTaxPercentage { get; set; }

            public string DestinationInvoiceNo { get; set; }
            public decimal? DestinationSalesTaxPercentage { get; set; }
        }

        public class SaveSalesTaxInvoiceParam
        {
            public DateTime? InvoiceDate { get; set; }
            public long? PartyBillId { get; set; }
            public int? ShippingProvince { get; set; }
            public string? ShippingInvoiceNo { get; set; }
            public decimal? ShippingSalesTaxPercentage { get; set; }
            public int? DestinationProvince { get; set; }
            public string? DestinationInvoiceNo { get; set; }
            public decimal? DestinationSalesTaxPercentage { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SALES_TAX_INVOICE_ADD")]
        public async Task<ActionResult> SaveSalesTaxInvoice([FromBody]SaveSalesTaxInvoiceParam p2)
        {
            SaveSalesTaxInvoiceReturn obj_return = new SaveSalesTaxInvoiceReturn();
            try
            {
                PartyBillTable? pbt = await _context.PartyBillTables
                      .Where(p => p.Id == p2.PartyBillId).FirstOrDefaultAsync();
                if (pbt == null)
                {
                    throw new Exception("Oops! Party bill not found.");
                }
                List<SalesTaxInvoicesTable> list1 =await _context.SalesTaxInvoicesTables
                    .Where(p => p.PartyBillId == p2.PartyBillId).ToListAsync();
                if (list1.Count == 0)
                {
                    SalesTaxInvoicesTable obj1 = new SalesTaxInvoicesTable();
                    obj1.InvoiceNo = p2.ShippingInvoiceNo;
                    obj1.InvoiceDate = p2.InvoiceDate;
                    obj1.ShippingProvinceId = p2.ShippingProvince;
                    obj1.DestinationProvinceId = p2.DestinationProvince;
                    obj1.InvoiceProvinceId = p2.ShippingProvince;
                    obj1.PartyId = pbt.PartyId;
                    obj1.PartyBillId = p2.PartyBillId;
                    obj1.SalesTaxPercent = p2.ShippingSalesTaxPercentage;
                    _context.SalesTaxInvoicesTables.Add(obj1);

                    if (p2.ShippingProvince != p2.DestinationProvince)
                    {
                        SalesTaxInvoicesTable obj2 = new SalesTaxInvoicesTable();
                        obj2.InvoiceNo = p2.DestinationInvoiceNo;
                        obj2.InvoiceDate = p2.InvoiceDate;
                        obj2.ShippingProvinceId = p2.ShippingProvince;
                        obj2.DestinationProvinceId = p2.DestinationProvince;
                        obj2.InvoiceProvinceId = p2.DestinationProvince;
                        obj2.PartyId = pbt.PartyId;
                        obj2.PartyBillId = p2.PartyBillId;
                        obj2.SalesTaxPercent = p2.DestinationSalesTaxPercentage;
                        _context.SalesTaxInvoicesTables.Add(obj2);
                    }
                }
                else
                {
                    SalesTaxInvoicesTable shippingInvoice = list1.Where(p => p.InvoiceProvinceId == p.ShippingProvinceId).First();
                    shippingInvoice.InvoiceNo = p2.ShippingInvoiceNo;
                    shippingInvoice.InvoiceDate = p2.InvoiceDate;
                    shippingInvoice.ShippingProvinceId = p2.ShippingProvince;
                    shippingInvoice.DestinationProvinceId = p2.DestinationProvince;
                    shippingInvoice.InvoiceProvinceId = p2.ShippingProvince;
                    shippingInvoice.PartyId = pbt.PartyId;
                    shippingInvoice.PartyBillId = p2.PartyBillId;
                    shippingInvoice.SalesTaxPercent = p2.ShippingSalesTaxPercentage;
                    if (list1.Count == 2)
                    {
                        SalesTaxInvoicesTable destinationInvoice = list1
                            .Where(p => p.InvoiceProvinceId == p.DestinationProvinceId).First();
                        destinationInvoice.InvoiceNo = p2.DestinationInvoiceNo;
                        destinationInvoice.InvoiceDate = p2.InvoiceDate;
                        destinationInvoice.ShippingProvinceId = p2.ShippingProvince;
                        destinationInvoice.DestinationProvinceId = p2.DestinationProvince;
                        destinationInvoice.InvoiceProvinceId = p2.DestinationProvince;
                        destinationInvoice.PartyId = pbt.PartyId;
                        destinationInvoice.PartyBillId = p2.PartyBillId;
                        destinationInvoice.SalesTaxPercent = p2.DestinationSalesTaxPercentage;
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
        public class SaveSalesTaxInvoiceReturn
        {
            public string Message { get; set; }
        }

        // Print
        public class SalesTaxPrintClass
        {
            public string? TokenNO { get; set; }
            public DateTime? InvoiceDate { get; set; }
            public string? LorryNO { get; set; }
            public string? FuelQty { get; set; }
            public string? Material { get; set; }
            public string? Origin { get; set; }
            public string? Destination { get; set; }
            public string TaxPercentage { get; set; }
            public decimal? FreightAmount { get; set; }
            public decimal? FreightAmount50Percentage { get; set; }
        }
        public class PrintSalesTaxParam
        {
            public int? PartyBillId { get; set; }
            public string Type { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,SALES_TAX_PRINT")]
        public async Task<IActionResult> ReportPreview([FromBody] PrintSalesTaxParam p1)
        {
            string base64EncodedPDF = "";
            string message = "";
            try
            {
                PartyBillTable? pbt = _context.PartyBillTables.Where(p => p.Id ==p1.PartyBillId).FirstOrDefault();

                SalesTaxInvoicesTable? stit = new SalesTaxInvoicesTable();
                if (p1.Type == "Shipping")
                {
                    stit = _context.SalesTaxInvoicesTables.Include(p => p.InvoiceProvince)
                    .Include(p => p.ShippingProvince).Include(p => p.DestinationProvince)
                    .Where(p => p.PartyBillId == p1.PartyBillId && p.InvoiceProvinceId == p.ShippingProvinceId).FirstOrDefault();
                }
                else
                {
                    stit = _context.SalesTaxInvoicesTables.Include(p => p.InvoiceProvince)
                    .Include(p => p.ShippingProvince).Include(p => p.DestinationProvince)
                    .Where(p => p.PartyBillId == p1.PartyBillId && p.InvoiceProvinceId == p.DestinationProvinceId).FirstOrDefault();
                }
                if (stit == null)
                {
                    throw new Exception("Oops! Sales Tax not found.");
                }
                var Province = stit.InvoiceProvince.Name.ToUpper();
                List<SalesTaxPrintClass> sales_tax_list = new List<SalesTaxPrintClass>();
                List<TripTable> tripList = _context.TripTables.Where(p => p.PartyBillId == stit.PartyBillId).ToList();
                foreach (var trip in tripList)
                {
                    SalesTaxPrintClass obj = new SalesTaxPrintClass();
                    obj.Destination =stit.DestinationProvince.Name;
                    obj.FreightAmount = trip.Freight;
                    obj.FreightAmount50Percentage = (trip.Freight) / 2;
                    obj.FuelQty = string.Format("{0}",trip.Quantity);
                    obj.InvoiceDate = trip.InvoiceDate;
                    obj.TaxPercentage = string.Format("{0:f0}%", stit.SalesTaxPercent);
                    //int accountId = int.Parse(trip.Account);
                    obj.LorryNO = string.Format("UNK{0}", await _context.AccountTables
                        .Where(p => p.AccountId == trip.Lorry).Select(p => p.Title).FirstOrDefaultAsync()) ;
                    obj.Material = _context.ProductTables.Where(p => p.Id == trip.ProductId)
                        .Select(p => p.Title).FirstOrDefault();
                    obj.Origin = stit.ShippingProvince.Name;
                    obj.TokenNO = string.Format("{0}",trip.TokenNo);
                    sales_tax_list.Add(obj);
                }
                decimal? rate_of_salestax = stit.SalesTaxPercent;

                var EXCLUSIVE_ST = sales_tax_list.Select(p => p.FreightAmount50Percentage).Sum().GetValueOrDefault();
                var SALES_TAX_AMOUNT = (EXCLUSIVE_ST * rate_of_salestax) / 100;
                var TOTAL = EXCLUSIVE_ST + SALES_TAX_AMOUNT;
                
                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var wri = new PdfWriter(stream))
                using (var pdf = new PdfDocument(wri))
                {
                    using (var doc = new iText.Layout.Document(pdf, PageSize.A4))
                    {
                        doc.SetMargins(20, 40, 40, 40);
                        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                        PdfFont font_Bold = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);

                        //page Header
                        Table header_table = new Table(UnitValue.CreatePercentArray(new float[] { 100 }));
                        header_table.AddHeaderCell(new Cell(1, 8).Add(new Paragraph("")).SetBorder(Border.NO_BORDER));

                        //Page Footer
                        Table footer_table = new Table(3);
                        footer_table.AddCell(new Cell(1, 3)
                            .Add(new Paragraph(string.Format("Printed By {0} on {1}", User.Identity.Name, DateTime.Now.AddHours(5)))
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(6)).SetBorder(Border.NO_BORDER));
                        // create a HeaderFooterEventHandler instance with the table as its parameter-
                        IEventHandler handler = new HeaderFooterEventHandler(header_table, footer_table, 20, 40, 40, 40);

                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

                        // Top information Table 
                        Table header_InfoTable = new Table(UnitValue.CreatePercentArray(new float[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, }));

                        header_InfoTable.AddHeaderCell(new Cell(1, 20).Add(new Paragraph(string.Format("{0} SALES TAX INVOICE", Province)).SetFont(font_Bold).SetFontSize(18)).SetTextAlignment(TextAlignment.CENTER).SetBorder(Border.NO_BORDER));

                        header_InfoTable.AddHeaderCell(new Cell(1, 15).Add(new Paragraph(string.Format("Service Providor: \t {0} ", "UNITED AZAD TR")).SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 5).Add(new Paragraph(string.Format("Date:{0:dd/MM/yyyy}", pbt.BillDate)).SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));

                        header_InfoTable.AddHeaderCell(new Cell(1, 3).Add(new Paragraph("Address:").SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 12).Add(new Paragraph("LS-60/61").SetFont(font).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 3).Add(new Paragraph(string.Format("ST Invoice number : ")).SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 2).Add(new Paragraph(string.Format("{0}{1}", stit.InvoiceNo, "")).SetFont(font).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));

                        header_InfoTable.AddHeaderCell(new Cell(1, 12).Add(new Paragraph(string.Format("NTN NO : {0}","0293748-4")).SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 8).Add(new Paragraph("")).SetBorder(Border.NO_BORDER));

                        header_InfoTable.AddHeaderCell(new Cell(1, 4).Add(new Paragraph(string.Format("{0}", "Service Recipent: ")).SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 16).Add(new Paragraph(string.Format("PAKISTAN STATE OIL COMPANY LIMITED")).SetFont(font).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));

                        header_InfoTable.AddHeaderCell(new Cell(1, 3).Add(new Paragraph(string.Format("Address: ")).SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 17).Add(new Paragraph(string.Format("PSO HOUSE Khayaban-e-Iqbal Clifton Karachi,75600 ")).SetFont(font).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));

                        header_InfoTable.AddHeaderCell(new Cell(1, 3).Add(new Paragraph(string.Format("NTN NO :")).SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 17).Add(new Paragraph(string.Format("0711554-7")).SetFont(font).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));

                        header_InfoTable.AddHeaderCell(new Cell(1, 5).Add(new Paragraph(string.Format("{0}", "Shipment Status : ")).SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 15).Add(new Paragraph(string.Format("Online Acknowledge")).SetFont(font).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));

                        header_InfoTable.AddHeaderCell(new Cell(1, 4).Add(new Paragraph("INVOICE ID : ").SetFont(font_Bold).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        header_InfoTable.AddHeaderCell(new Cell(1, 16).Add(new Paragraph(string.Format("{0}", pbt.BillNo)).SetFont(font).SetTextAlignment(TextAlignment.LEFT).SetFontSize(11)).SetBorder(Border.NO_BORDER));
                        doc.Add(header_InfoTable);

                        // Add data to the table
                        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 10, 5, 13, 11, 11, 10, 10, 5, 13, 15 }));
                        table.SetMargins(20, 0, 0, 0);
                        table.SetWidth(UnitValue.CreatePercentValue(100));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Shipment#").SetFont(font_Bold).SetTextAlignment(TextAlignment.CENTER).SetFontSize(9)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Date").SetFont(font_Bold).SetTextAlignment(TextAlignment.CENTER).SetFontSize(9)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Tank Lorry").SetFont(font_Bold).SetTextAlignment(TextAlignment.CENTER).SetFontSize(9)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Quantity").SetFont(font_Bold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Material").SetFont(font_Bold).SetTextAlignment(TextAlignment.CENTER).SetFontSize(9)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Origin").SetFont(font_Bold).SetTextAlignment(TextAlignment.CENTER).SetFontSize(9)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Destination").SetFont(font_Bold).SetTextAlignment(TextAlignment.CENTER).SetFontSize(9)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Tax Rate").SetFont(font_Bold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Freight Amount(Rs)").SetFont(font_Bold).SetTextAlignment(TextAlignment.CENTER).SetFontSize(9)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("50% Freight Amount(Rs)").SetFont(font_Bold).SetTextAlignment(TextAlignment.CENTER).SetFontSize(9)).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));

                        foreach (var item in sales_tax_list)
                        {
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.TokenNO)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:dd/MM/yyyy}", item.InvoiceDate)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.LorryNO)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.FuelQty)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.Material)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.Origin)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.Destination)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", item.TaxPercentage)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n2}", item.FreightAmount)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                            table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n2}", item.FreightAmount50Percentage)).SetFont(font).SetTextAlignment(TextAlignment.CENTER)).SetFontSize(8).SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
                        }
                        doc.Add(table);


                        Table Calc_table = new Table(UnitValue.CreatePercentArray(new float[] { 80, 20 }));
                        Calc_table.SetWidth(UnitValue.CreatePercentValue(100));
                        Calc_table.SetMargins(10, 0, 10, 0);
                        Calc_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", "VALUE EXCLUSIVE OF ST :")).SetFont(font).SetTextAlignment(TextAlignment.RIGHT)).SetFontSize(9).SetBorder(Border.NO_BORDER));
                        Calc_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n2}", EXCLUSIVE_ST)).SetFont(font).SetTextAlignment(TextAlignment.RIGHT)).SetFontSize(8).SetBorder(Border.NO_BORDER));

                        Calc_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", "RATE OF SALES TAX :")).SetFont(font).SetTextAlignment(TextAlignment.RIGHT)).SetFontSize(9).SetBorder(Border.NO_BORDER));
                        Calc_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n2}", stit.SalesTaxPercent)).SetFont(font).SetTextAlignment(TextAlignment.RIGHT)).SetFontSize(8).SetBorder(Border.NO_BORDER));

                        Calc_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", "SALES TAX AMOUNT : ")).SetFont(font).SetTextAlignment(TextAlignment.RIGHT)).SetFontSize(9).SetBorder(Border.NO_BORDER));
                        Calc_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n2}", SALES_TAX_AMOUNT)).SetFont(font).SetTextAlignment(TextAlignment.RIGHT)).SetFontSize(8).SetBorder(Border.NO_BORDER));

                        Calc_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0}", "TOTAL :")).SetFont(font).SetTextAlignment(TextAlignment.RIGHT)).SetFontSize(9).SetBorder(Border.NO_BORDER));
                        Calc_table.AddCell(new Cell(1, 1).Add(new Paragraph(string.Format("{0:n2}", TOTAL)).SetFont(font).SetTextAlignment(TextAlignment.RIGHT)).SetFontSize(8).SetBorder(Border.NO_BORDER));

                        doc.Add(Calc_table);

                        //Signature Tabl
                        Table signature_table = new Table(UnitValue.CreatePercentArray(new float[] { 15, 25, 60 }));
                        signature_table.SetWidth(UnitValue.CreatePercentValue(100)).SetFontSize(11);
                        signature_table.SetMargins(10, 0, 0, 0);

                        signature_table.AddCell(new Cell(1, 1).Add(new Paragraph("Signature:")
                            .SetTextAlignment(TextAlignment.RIGHT)).SetBorder(Border.NO_BORDER));
                        signature_table.AddCell(new Cell(1, 1).Add(new Paragraph("")).SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(1)));
                        doc.Add(signature_table);
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