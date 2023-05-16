
using Bytescout.PDFExtractor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,EFI")]

    public class EFIController : Controller
    {
        private readonly UADbContext _context;
        public EFIController(UADbContext context)
        {
            this._context = context;
        }
        public ActionResult EFI()
        {
            return View("~/Views/CompanyBill/EFI.cshtml");
        }

        //[HttpPost]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            UploadReturnClass obj_return = new UploadReturnClass();
            try
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);

                    // this is the index of the page containing the targeted table
                    int targetPageNumber = 1;

                    // Create Bytescout.PDFExtractor.StructuredExtractor instance (former TableExtractor)
                    StructuredExtractor extractor = new StructuredExtractor();
                    extractor.RegistrationName = "demo";
                    extractor.RegistrationKey = "demo";

                    // Load sample PDF document
                    //extractor.LoadDocumentFromFile(pdfFile);
                    extractor.LoadDocumentFromStream(ms);

                    //Define the extraction area
                    //extractor.SetExtractionArea(new System.Drawing.RectangleF(20, 380, 600, 200));

                    //int pageCount99 = extractor.GetPageCount();

                    for (int ipage = 0; ipage < extractor.GetPageCount(); ipage++)
                    {
                        //In the current program, we only need the first page
                        if ((ipage + 1) != targetPageNumber) continue;

                        //Prepare the page structure
                        extractor.PrepareStructure(ipage);

                        //Count the actual table rows 
                        int rowCount = extractor.GetRowCount(ipage);

                        List<TripClassForDataExtraction> trip_list_after_extraction = new List<TripClassForDataExtraction>();
                        int companyid = 0;
                        //Loop over the row count...
                        for (int row = 10; row < rowCount - 1; row++)
                        {
                            // ...then loop over the column
                            int columnCount = extractor.GetColumnCount(ipage, row);
                            TripClassForDataExtraction obj = new TripClassForDataExtraction();
                            int count = 0;
                            for (int col = 0; col < columnCount; col++)
                            {
                                //The text of the table cell[row,col]
                                var cellValue = extractor.GetCellValue(ipage, row, col);
                                string cell_value = RemoveTrialString(string.Format("{0}", cellValue));
                                if (cell_value != "")
                                {
                                    ++count;
                                }
                                else
                                {
                                    continue;
                                }
                                if (count == 2) { obj.TokenNo = cell_value; }
                                if (count == 3) { obj.InvoiceDate = cell_value; }
                                if (count == 4) { obj.DeliveryNumber = cell_value; }
                                if (count == 5) { obj.Lorry = cell_value; }
                                if (count == 6) { obj.Quantity = cell_value; }
                                if (count == 8) { obj.Freight = cell_value; }

                            }
                            if (obj.TokenNo != "" && obj.TokenNo != null)
                            {
                                trip_list_after_extraction.Add(obj);
                            }
                        }

                        obj_return.TripList.AddRange(FormatExtractionData(trip_list_after_extraction, _context));
                        obj_return.Message = "OK";
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
        public class UploadReturnClass
        {
            public UploadReturnClass()
            {
                TripList = new List<TripClassToShowInTripGrid>();
            }
            public string? Message { get; set; }
            public List<TripClassToShowInTripGrid> TripList { get; set; }
        }

        private string RemoveTrialString(string string_to_remove_trial_string)
        {
            string ok = "";

            int l = string_to_remove_trial_string.IndexOf("(");

            if (l > 0)
            {
                ok = string_to_remove_trial_string.Substring(0, l);

            }
            else if (l == 0)
            {
                ok = "";
            }
            else
            {
                ok = string_to_remove_trial_string;
            }
            return ok;
        }
        private string FormatLorry(string v)
        {
            string lorry = "";

            int count = 0;
            foreach (char c in v)
            {
                if (Char.IsDigit(c) && count == 0)
                {
                    lorry += '-';
                    count++;
                }
                lorry += c;
            }
            return lorry;
        }

        private List<TripClassToShowInTripGrid> FormatExtractionData(List<TripClassForDataExtraction> trip_list_after_extraction,
            UADbContext context)
        {
            List<TripClassToShowInTripGrid> update_trip_list = new List<TripClassToShowInTripGrid>();
            int? SNo = 0;
            foreach (var item in trip_list_after_extraction)
            {
                //invoice date
                int day = 0;
                int month = 0;
                int year = 0;
                int.TryParse(item.InvoiceDate.Substring(0, 2), out day);
                int.TryParse(item.InvoiceDate.Substring(3, 2), out month);
                int.TryParse(item.InvoiceDate.Substring(6, 4), out year);
                if (day != 0)
                {
                    DateTime invoice_date = new DateTime(year, month, day);
                    string lorry = FormatLorry(item.Lorry.Substring(3));
                    AccountTable? lorry_account = context.AccountTables.Where(p => p.Title == lorry).FirstOrDefault();
                    if (lorry_account == null)
                    {
                        throw new Exception(string.Format("Lorry {0} is not Registered in Chart of Account", lorry));
                    }
                    string account = string.Format("{0}", lorry_account.AccountId);
                    TripTable? tt = context.TripTables.Where(p => p.Lorry == lorry_account.AccountId && p.EntryDate <= invoice_date)
                        .OrderByDescending(p => p.EntryDate).FirstOrDefault();
                    if (tt == null)
                    {
                        throw new Exception(string.Format("Oops! {0} Records not held.", lorry));
                    }
                    int token_no = 0;
                    if (item.TokenNo != null && item.TokenNo != "")
                    {
                        if (!int.TryParse(item.TokenNo, out token_no))
                        {
                            throw new Exception("Token is not in Digit Format");
                        }
                    }
                    string delivery_no = "";
                    if (item.DeliveryNumber != null && item.DeliveryNumber != "")
                    {
                        delivery_no = item.DeliveryNumber;
                    }
                    string qty = "";
                    if (item.Quantity != null && item.Quantity != "")
                    {
                        qty = item.Quantity;
                    }
                    decimal freight;
                    decimal.TryParse(item.Freight, out freight);
                    if (tt.TokenNo != null && tt.TokenNo != token_no)
                    {
                        TripTable new_trip = new TripTable();
                        new_trip.TokenNo = token_no;
                        //new_trip.DeliveryNumber = delivery_no;
                        new_trip.Lorry = lorry_account.AccountId;
                        new_trip.InvoiceDate = invoice_date;
                        new_trip.Quantity = double.Parse(qty);
                        new_trip.Freight = freight;
                        update_trip_list.Add(new TripClassToShowInTripGrid(new_trip, lorry, ++SNo));
                    }
                    else if (tt.TokenNo == null || tt.TokenNo == token_no)
                    {
                        tt.Lorry = lorry_account.AccountId;
                        tt.TokenNo = token_no;
                        //tt.DeliveryNumber = delivery_no;
                        tt.InvoiceDate = invoice_date;
                        tt.Quantity = double.Parse(qty);
                        tt.Freight = freight;
                        update_trip_list.Add(new TripClassToShowInTripGrid(tt, lorry, ++SNo));
                    }
                }
            }
            return update_trip_list;
        }
        public class TripClassToShowInTripGrid : TripTable
        {
            public TripClassToShowInTripGrid() { }
            public TripClassToShowInTripGrid(TripTable j,string lorry, int? SNo)
            {
                this.TripId = j.TripId;
                this.EntryDate = j.EntryDate;
                this.Lorry = j.Lorry;
                this.TokenNo = j.TokenNo;
                this.LorryBillNo = j.LorryBillNo;
                this.InvoiceDate = j.InvoiceDate;
                this.ShippingId = j.ShippingId;
                this.DestinationId = j.DestinationId;
                this.Quantity = j.Quantity;
                this.Freight = j.Freight;
                this.ShortQtyString = string.Format("{0}",j.ShortQty);
                this.ShortAmountString = string.Format("{0}", j.ShortAmount);
                this.PartyBillId = j.PartyBillId;
                this.ProductId = j.ProductId;
                this.SNo = SNo;
                this.LorryTitle = lorry;
            }
            public string LorryTitle { get; set; }
            public string? ShortQtyString { get; set; }
            public string? ShortAmountString { get; set; }
            public int? InvoiceDateDay { get; set; }
            public int? InvoiceDateMonth { get; set; }
            public int? InvoiceDateYear { get; set; }
            public int? SNo { get; set; }
        }
        public class TripClassForDataExtraction
        {
            public int SNo { get; set; }
            public string? TokenNo { get; set; }
            public string? DeliveryNumber { get; set; }
            public string? InvoiceDate { get; set; }
            public string? Lorry { get; set; }
            public string? Quantity { get; set; }
            public string? Freight { get; set; }
        }
        public class TripClassToShowInHistoryGrid:TripTable
        {
            public TripClassToShowInHistoryGrid()
            { }
            public TripClassToShowInHistoryGrid(TripTable tt,string? LorryTitle, UADbContext context)
            {
                this.TripId = tt.TripId;
                this.Lorry = tt.Lorry;
                this.EntryDate = tt.EntryDate;
                this.TokenNo = tt.TokenNo;
                this.IsChecked = false;
                this.LorryTitle=LorryTitle;
            }
            public string? LorryTitle { get; set; }
            public bool? IsChecked { get; set; }
        }

        //Save
        public class SaveBillParamClass
        {
            public SaveBillParamClass()
            {
                TripList = new List<TripClassToShowInTripGrid>();
            }
            public int? CompanyId { get; set; }
            public DateTime? BillDate { get; set; }
            public string BillNo { get; set; }
            public List<TripClassToShowInTripGrid> TripList { get; set; }
        }
        public async Task<ActionResult> SaveBill([FromBody] SaveBillParamClass p1)
        {
            SaveBillReturnClass obj_return = new SaveBillReturnClass();
            try
            {
                if (p1.TripList.Count == 0)
                {
                    throw new Exception("Oops! No Record to Save.");
                }
                if (p1.CompanyId == null)
                {
                    throw new Exception("Oops! Company Missing.");
                }
                if (p1.BillDate == null)
                {
                    throw new Exception("Oops! Bill Date Missing.");
                }

                //check duplicate bill
                //string Type = string.Format("{0}", p1.CompanyId);
                PartyBillTable? pbt2 = await _context.PartyBillTables.Where(p => p.BillNo == p1.BillNo && p.PartyId==p1.CompanyId)
                    .FirstOrDefaultAsync();
                if (pbt2 != null)
                {
                    throw new Exception(string.Format("Oops! Bill No {0} already exists", p1.BillNo));

                }

                TripTable tt = p1.TripList.First();
                
                PartyBillTable pbt = new PartyBillTable();
                pbt.BillNo =p1.BillNo;
                pbt.BillDate = p1.BillDate;
                pbt.PartyId = p1.CompanyId;

                foreach (var item in p1.TripList)
                {
                    TripTable? db2 =await _context.TripTables.FirstOrDefaultAsync(p => p.TripId == item.TripId);
                    if (db2 != null)
                    {
                        db2.TokenNo = item.TokenNo;
                        db2.InvoiceDate =new DateTime(item.InvoiceDateYear.Value,
                            item.InvoiceDateMonth.Value,item.InvoiceDateDay.Value);
                        db2.Quantity = item.Quantity;
                        db2.Freight = item.Freight;
                        if (item.ShortQty != null)
                        {
                            db2.ShortQty =item.ShortQty;
                        }
                        if (item.ShortAmount != null)
                        {
                            db2.ShortAmount =item.ShortAmount;
                        }
                        pbt.TripTables.Add(db2);
                    }
                    else
                    {
                        throw new Exception("Attach Record with a Trip");
                    }
                }
                _context.PartyBillTables.Add(pbt);
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
        public class SaveBillReturnClass
        {
            public SaveBillReturnClass()
            {
            }
            public string? Message { get; set; }
        }

        //View History
        public class ViewHistoryParamClass
        {
            public ViewHistoryParamClass()
            {
            }
            public long? LorryAccountId { get; set; }
            public string InvoiceDateString { get; set; }
        }
        public async Task<ActionResult> ViewHistory([FromBody] ViewHistoryParamClass p1)
        {
            ViewHistoryReturnClass obj_return = new ViewHistoryReturnClass();
            try
            {
                _context.Database.SetCommandTimeout(300);
                int day =int.Parse(p1.InvoiceDateString.Substring(0, 2));
                int month = int.Parse(p1.InvoiceDateString.Substring(3,2));
                int year = int.Parse(p1.InvoiceDateString.Substring(6, 4));
                DateTime? InvoiceDate = new DateTime(year,month,day);
                List<TripTable> tt_list1 =await _context.TripTables.Where(p => p.Lorry == p1.LorryAccountId && p.EntryDate <= InvoiceDate &&
                    p.TokenNo == null).OrderByDescending(p => p.EntryDate).Take(5).ToListAsync();
                List<TripTable> tt_list2 =await _context.TripTables.Where(p => p.Lorry == p1.LorryAccountId &&
                    p.EntryDate > InvoiceDate && p.TokenNo == null).ToListAsync();
                tt_list1.AddRange(tt_list2);
                foreach (var item in tt_list1)
                {
                    string LorryTitle =await _context.AccountTables.Where(p => p.AccountId == item.Lorry).Select(p => p.Title).FirstOrDefaultAsync();
                    obj_return.TripList.Add(new TripClassToShowInHistoryGrid(item,LorryTitle,_context));
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
        public class ViewHistoryReturnClass
        {
            public ViewHistoryReturnClass()
            {
                TripList = new List<TripClassToShowInHistoryGrid>();
            }
            public string Message { get; set; }

            public List<TripClassToShowInHistoryGrid> TripList { get; set; }
        }

        //Update Main Table
        public class UpdateHistoryParamClass
        {
            public long? TripIDHistoryGrid { get; set; }
            public int? SNoInTripGrid { get; set; }
        }
        public async Task<ActionResult> UpdateHistory([FromBody] UpdateHistoryParamClass p1)
        {
            UpdateHistoryReturnClass obj_return = new UpdateHistoryReturnClass();
            try
            {
                TripTable history_grid =await _context.TripTables.Where(p => p.TripId == p1.TripIDHistoryGrid).FirstAsync();
                string? LorryTitle = await _context.AccountTables.Where(p => p.AccountId == history_grid.Lorry).Select(p => p.Title).FirstOrDefaultAsync();
                obj_return.TripClass = new TripClassToShowInTripGrid(history_grid, LorryTitle, p1.SNoInTripGrid);
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
        public class UpdateHistoryReturnClass
        {
            public UpdateHistoryReturnClass()
            {
                TripClass = new TripClassToShowInTripGrid();
            }
            public string? Message { get; set; }

            public TripClassToShowInTripGrid TripClass { get; set; }
        }

    }
}
