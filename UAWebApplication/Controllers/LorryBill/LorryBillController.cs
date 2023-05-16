
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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Asn1.Esf;
using UAWebApplication.Data;
using UAWebApplication.Models;

namespace UAWebApplication.Controllers
{
    [Authorize]
    public class LorryBillController : Controller
    {
        private readonly UADbContext _context;
        public LorryBillController(UADbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,LORRY_BILL_VIEW")]
        public ActionResult LorryBill()
        {
            return View("~/Views/LorryBill/LorryBill.cshtml");
        }

        public class YearsListParam
        {
            public long? SelectedLorryId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,LORRY_BILL_VIEW")]
        public async Task<IActionResult> YearsList([FromBody]YearsListParam p1)
        {
            YearsListReturn obj_return = new YearsListReturn();
            try
            {
                List<int> year_list =await _context.LorryBillTables.Where(p => p.Lorry == p1.SelectedLorryId)
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
        public class YearsListReturn
        {
            public YearsListReturn()
            {
                YearList = new List<int>();
            }
            public string? Message { get; set; }
            public List<int> YearList { get; set; }
        }

        public class LorryBillListParam
        {
            public long? LorryId { get; set; }
            public int? Year { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,LORRY_BILL_VIEW")]
        public async Task<IActionResult> LorryBillList([FromBody] LorryBillListParam p1)
        {
            LorryBillListReturn obj_return = new LorryBillListReturn();
            try
            {
                obj_return.LorryBillList = await _context.LorryBillTables
                    .Where(p => p.Lorry == p1.LorryId && p.BillDate.Value.Year == p1.Year).OrderBy(p => p.BillDate).ToListAsync();
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
        public class LorryBillListReturn
        {
            public LorryBillListReturn()
            {
                LorryBillList = new List<LorryBillTable>();
            }
            public string? Message { get; set; }
            public List<LorryBillTable> LorryBillList { get; set; }
        }

        public class ViewLorryBillParam
        {
            public long? LorryId { get; set; }
            public long? BillId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,LORRY_BILL_VIEW")]
        public async Task<IActionResult> ViewLorryBill([FromBody] ViewLorryBillParam p1)
        {
            ViewLorryBillReturn obj_return = new ViewLorryBillReturn();
            try
            {
                LorryBillTable? lbt =await _context.LorryBillTables.FirstOrDefaultAsync(p => p.BillNo ==p1.BillId);
                if (lbt == null)
                {
                    throw new Exception("Oops! Lorry Bill not found.");
                }
                //Trip Data
                List<LorryTripClass> tripListToShowInTable = new List<LorryTripClass>();
                List<TripTable> tripListInDb =await _context.TripTables.Where(p => p.LorryBillNo == lbt.BillNo & p.Lorry == lbt.Lorry)
                    .OrderBy(p => p.InvoiceDate).ToListAsync();
                foreach (var item in tripListInDb)
                {
                    tripListToShowInTable.AddRange(ConvertRange(_context,item));
                }
                obj_return.TripList.AddRange(tripListToShowInTable);
                
                //Advance Data
                List<JournalClass> advanceListToShowInTable = new List<JournalClass>();
                List<JournalTable> journalList =await _context.JournalTables.Where(r => r.LorryBillNo == lbt.BillNo &&
                    r.EntryType == "GV" && r.TripId == null && r.Debit != null & r.Debit != 0 && r.AccountId == lbt.Lorry)
                    .OrderBy(p => p.EntryDate).OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in journalList)
                {
                    advanceListToShowInTable.Add(ConvertAdvanceClass(item));
                }
                JournalClass jc = new JournalClass();
                jc.Debit = tripListToShowInTable.Sum(p => p.TripMunshiana).GetValueOrDefault();
                jc.Description = "لوڈ منشیانہ";
                advanceListToShowInTable.Add(jc);

                //Credit Data
                List<JournalTable> creditList =await _context.JournalTables.Where(r => r.LorryBillNo == lbt.BillNo &&
                    r.EntryType == "GV" && r.TripId == null && r.Credit != null & r.Credit != 0 && r.AccountId == lbt.Lorry)
                    .OrderBy(p => p.EntryDate).ToListAsync();
                foreach (var item in creditList)
                {
                    advanceListToShowInTable.Add(ConvertCreditClass(item));
                }
                obj_return.AdvanceList.AddRange(advanceListToShowInTable);
                
                AccountTable? account =await _context.AccountTables.Where(p => p.AccountId == lbt.Lorry).FirstAsync();
                LorryTable? lorry =await _context.LorryTables.Where(p => p.AccountId == account.AccountId).FirstOrDefaultAsync();
                
                //Lorry Bill Summary
                List<LorryBillSummaryDto> summaryList = new List<LorryBillSummaryDto>();
                decimal? totalTripAdvance = tripListToShowInTable.Sum(p => p.TripAdvance).GetValueOrDefault(0);
                summaryList.Add(new LorryBillSummaryDto {Description = "ٹوٹل ٹرپ ایڈوانس", Amount =totalTripAdvance });
                decimal? totalCommission = tripListToShowInTable.Sum(p => p.Commission).GetValueOrDefault(0);
                summaryList.Add(new LorryBillSummaryDto{Description = "کمیشن", Amount = totalCommission});
                decimal? total_wh_tax = tripListToShowInTable.Sum(p => p.Tax).GetValueOrDefault(0);
                summaryList.Add(new LorryBillSummaryDto{Description = "انکم ٹیکس",Amount = total_wh_tax});
                decimal? totalShortAmount = tripListToShowInTable.Sum(p => p.ShortAmount).GetValueOrDefault(0);
                summaryList.Add(new LorryBillSummaryDto { Description = "شارٹیج", Amount = totalShortAmount });
                decimal? billCharges = lbt.BillCharges.GetValueOrDefault(0);
                summaryList.Add(new LorryBillSummaryDto{Description = "بل چارجز",Amount = billCharges});
                decimal? totalDeductions = totalTripAdvance + totalCommission + total_wh_tax + totalShortAmount + billCharges;
                summaryList.Add(new LorryBillSummaryDto { Description = "کل خرچہ", Amount = totalDeductions });
                decimal? totalFreight = tripListToShowInTable.Sum(p => p.Freight).GetValueOrDefault(0);
                summaryList.Add(new LorryBillSummaryDto { Description = "کل کرایہ", Amount = totalFreight });

                string current_month_lbl = "";
                decimal? current_month_amount =totalFreight - totalDeductions;
                if (current_month_amount >= 0)
                {
                    current_month_lbl = string.Format("ماہ {0} کی بچت", GetMonthInUrdu(lbt.BillDate));
                }
                else
                {
                    current_month_lbl = string.Format("ماہ {0} کا بنام", GetMonthInUrdu(lbt.BillDate));
                }
                summaryList.Add(new LorryBillSummaryDto { Description = current_month_lbl, Amount = Math.Abs(current_month_amount.Value) });

                decimal? totalAdvance = advanceListToShowInTable.Sum(p => p.Debit).GetValueOrDefault(0)
                    -advanceListToShowInTable.Sum(p=>p.Credit).GetValueOrDefault(0);
                summaryList.Add(new LorryBillSummaryDto 
                {
                    Description = string.Format("ماہ {0} کے دیگر اخراجات", GetMonthInUrdu(lbt.BillDate)), Amount = totalAdvance
                });

                decimal? balance = current_month_amount - totalAdvance;
                string? balance_lbl = "";
                if (balance >= 0)
                {
                    balance_lbl = "بقایا جمع";
                }
                else
                {
                    balance_lbl = "بقایا بنام";
                }
                summaryList.Add(new LorryBillSummaryDto { Description = balance_lbl, Amount = balance });

                //Calculation of Brought Forward Amount
                string? bflbl = "Previous Month Saving";
                decimal? bfAmount = 0;
                List<LorryBillTable> lbtList =await _context.LorryBillTables
                    .Where(p => p.BillDate < lbt.BillDate && p.Lorry == lbt.Lorry).ToListAsync();
                if (lbtList.Count != 0) 
                { 
                    foreach (var item in lbtList)
                    {
                        List<TripTable> tripList = await _context.TripTables.Where(p => p.LorryBillNo == item.BillNo).ToListAsync();
                        bfAmount += tripList.Sum(p => p.Freight).GetValueOrDefault(0);
                        bfAmount -= tripList.Sum(p => p.Commission).GetValueOrDefault(0);
                        bfAmount -= tripList.Sum(p => p.Tax).GetValueOrDefault(0);
                        bfAmount -= tripList.Sum(p => p.ShortAmount).GetValueOrDefault(0);
                        foreach (var trip in tripList)
                        {
                            bfAmount -= await _context.JournalTables.Where(p => p.TripId == trip.TripId && (p.EntryType == "GV" || p.EntryType == "TM") &&
                                p.AccountId == trip.Lorry).SumAsync(p => p.Debit);
                        }
                        //bfAmount += creditdata.Sum(p => p.Amount).GetValueOrDefault(0);
                        bfAmount -= await _context.JournalTables.Where(r => r.LorryBillNo == item.BillNo && r.EntryType == "GV" && r.TripId == null &&
                            r.Debit != null & r.Debit != 0 && r.AccountId == item.Lorry).SumAsync(p => p.Debit);
                        bfAmount += await _context.JournalTables.Where(r => r.LorryBillNo == item.BillNo && r.EntryType == "GV" && r.TripId == null &&
                            r.Credit != null & r.Credit != 0 && r.AccountId == item.Lorry).SumAsync(p => p.Credit);
                        bfAmount -= item.BillCharges.GetValueOrDefault(0);
                    }
                    LorryBillTable? lbtForMonthOfBF = lbtList.OrderByDescending(p => p.BillDate).FirstOrDefault();
                    if (bfAmount < 0)
                    {
                        bflbl = string.Format("{0} کا قرضہ", ConvertDateInUrdu(lbtForMonthOfBF.BillDate));
                        //bflbl = string.Format("Banam of {0}", string.Format("{0:MMM,yyyy}", lbtForMonthOfBF.BillDate));
                    }
                    else
                    {
                        bflbl = string.Format("{0} کاجمع", ConvertDateInUrdu(lbtForMonthOfBF.BillDate));
                        //bflbl = string.Format("Saving of {0}", string.Format("{0:MMM,yyyy}", lbtForMonthOfBF.BillDate));
                    }
                }
                summaryList.Add(new LorryBillSummaryDto { Description = bflbl, Amount = bfAmount });
                decimal? lorry_cf_amount = balance + bfAmount;
                string? lorry_cf_lbl = "";
                if (lorry_cf_amount < 0)
                {
                    lorry_cf_lbl = "بنام گاڑی";
                }
                else
                {
                    lorry_cf_lbl = "گاڑی جمع";
                }
                summaryList.Add(new LorryBillSummaryDto { Description = lorry_cf_lbl, Amount = lorry_cf_amount });
                obj_return.SummaryList.AddRange(summaryList);
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
        public class ViewLorryBillReturn
        {
            public ViewLorryBillReturn()
            {
                TripList = new List<LorryTripClass>();
                AdvanceList = new List<JournalClass>();
                SummaryList = new List<LorryBillSummaryDto>();
            }
            public string Message { get; set; }
            public List<LorryTripClass> TripList{ get; set; }
            public List<JournalClass> AdvanceList { get; set; }
            public List<LorryBillSummaryDto> SummaryList { get; set; }
        }
        public class LorryTripClass
        {
            public long TripID { get; set; }
            public DateTime? EntryDate { get; set; }
            public string? EntryDateString { get; set; }
            public int? TokenNo { get; set; }
            public DateTime? InvoiceDate { get; set; }
            public long? Lorry { get; set; }
            public string? Quantity { get; set; }
            public double? Rate { get; set; }
            public decimal? Freight { get; set; }
            public decimal? Commission { get; set; }
            public decimal? Tax { get; set; }
            public double? ShortQty { get; set; }
            public decimal? ShortRate { get; set; }
            public decimal? ShortAmount { get; set; }
            public decimal? TripAdvance { get; set; }
            public bool? Billed { get; set; }
            public string? ShippingTitle { get; set; }
            public string? DestinationTitle { get; set; }
            public string? ProductTitle { get; set; }
            public decimal? TripMunshiana { get; set; }
        }
        public class JournalClass
        {
            public long? Id { get; set; }
            public long? TransId { get; set; }
            public long? AccountID { get; set; }
            public DateTime? EntryDate { get; set; }
            public string? EntryDateString { get; set; }
            public string? Description { get; set; }
            public decimal? Debit { get; set; }
            public decimal? Credit { get; set; }
        }
        public class LorryBillSummaryDto
        {
            public string? Description { get; set; }
            public decimal? Amount { get; set; }
        }
        public static LorryTripClass Convert(UADbContext context, TripTable item)
        {
            LorryTripClass obj = new LorryTripClass();
            obj.TripID = item.TripId;
            obj.EntryDate = item.EntryDate;
            obj.TokenNo = item.TokenNo;
            obj.InvoiceDate = item.InvoiceDate;
            obj.Lorry = item.Lorry;
            if (item.Quantity < 100)
            {
                obj.Quantity = string.Format("{0:f2}", item.Quantity);

            }
            else
            {
                obj.Quantity = string.Format("{0:f0}", item.Quantity);
            }
            obj.Rate = item.Rate ?? 0;
            obj.Freight = item.Freight ?? 0;
            obj.Commission = item.Commission ?? 0;
            obj.Tax = item.Tax ?? 0;
            obj.ShortQty = item.ShortQty;
            obj.ShortRate = item.ShortRate ?? 0;
            obj.ShortAmount = item.ShortAmount ?? 0;
            obj.ShippingTitle= context.ShippingTables.Where(p => p.Id == item.ShippingId).Select(p => p.TitleUrdu).FirstOrDefault();
            obj.DestinationTitle = context.DestinationTables.Where(p => p.Id == item.DestinationId).Select(p => p.TitleUrdu).FirstOrDefault();
            obj.ProductTitle = context.ProductTables.Where(p => p.Id == item.ProductId).Select(p => p.TitleUrdu).FirstOrDefault();
            
            //Get trip advance amount from JournalTable
            decimal? journaladvance = context.JournalTables.Where(p => p.TripId == item.TripId && p.EntryType == "GV" && p.AccountId == item.Lorry)
                .Sum(p => p.Debit).GetValueOrDefault(0);
            obj.TripAdvance = journaladvance;
            decimal? trip_munshiana = context.JournalTables.Where(p => p.TripId == item.TripId && p.EntryType == "TM" &&
                p.AccountId == item.Lorry).Sum(p => p.Debit).GetValueOrDefault(0);
            obj.TripMunshiana = trip_munshiana;

            return obj;
        }
        public static List<LorryTripClass> ConvertRange(UADbContext context, TripTable item)
        {
            List<LorryTripClass> obj_list = new List<LorryTripClass>();
            LorryTripClass obj = new LorryTripClass();
            obj.TripID = item.TripId;
            obj.EntryDate = item.EntryDate;
            obj.TokenNo = item.TokenNo;
            obj.InvoiceDate = item.InvoiceDate;
            obj.Lorry = item.Lorry;
            if (item.Quantity < 100)
            {
                obj.Quantity = string.Format("{0:f2}", item.Quantity);

            }
            else
            {
                obj.Quantity = string.Format("{0:f0}", item.Quantity);
            }
            obj.Rate = item.Rate ?? 0;
            obj.Freight = item.Freight ?? 0;
            obj.Commission = item.Commission ?? 0;
            obj.Tax = item.Tax ?? 0;
            obj.ShortQty = item.ShortQty;
            obj.ShortRate = item.ShortRate ?? 0;
            obj.ShortAmount = item.ShortAmount ?? 0;
            string? shipping = context.ShippingTables.Where(p => p.Id == item.ShippingId).Select(p => p.TitleUrdu).FirstOrDefault();
            obj.ShippingTitle = shipping;
            string? destination = context.DestinationTables.Where(p => p.Id == item.DestinationId).Select(p => p.TitleUrdu).FirstOrDefault(); ;
            obj.DestinationTitle = string.Format("{0} {1} {2}", shipping, "سے", destination);
            obj.ProductTitle = context.ProductTables.Where(p => p.Id == item.ProductId).Select(p => p.TitleUrdu).FirstOrDefault();
            
            //Get trip advance amount from JournalTable
            decimal? trip_advance_cash_amount = 0;
            List<LorryTripClass> TripAdvanceList = new List<LorryTripClass>();
            List<JournalTable> trip_advance_records = context.JournalTables.Where(p => p.TripId == item.TripId && p.EntryType == "GV" &&
                p.AccountId == item.Lorry && p.Debit != null).ToList();

            foreach (var advance in trip_advance_records)
            {
                JournalTable? credit_record = context.JournalTables.Include(p=>p.Account)
                    .Where(p => p.TransId == advance.TransId && p.Credit != null).FirstOrDefault();
                if (credit_record == null)
                {
                    throw new Exception(string.Format("Oops! Credit Record of Transaction Id {0} Missing", advance.TransId));
                }
                if (credit_record.Account.Title == "CASH IN HAND")
                {
                    trip_advance_cash_amount += credit_record.Credit.GetValueOrDefault(0);
                }
                else
                {
                    LorryTripClass advance_record = new LorryTripClass();
                    advance_record.EntryDate = advance.EntryDate;
                    advance_record.InvoiceDate = advance.EntryDate;
                    advance_record.DestinationTitle = credit_record.Account.Title;
                    advance_record.TripAdvance = advance.Debit;
                    TripAdvanceList.Add(advance_record);
                }
            }
            obj.TripAdvance = trip_advance_cash_amount;
            decimal? trip_munshiana = context.JournalTables.Where(p => p.TripId == item.TripId && p.EntryType == "TM" &&
                p.AccountId == item.Lorry).Sum(p => p.Debit).GetValueOrDefault(0);
            obj.TripMunshiana = trip_munshiana;
            obj_list.Add(obj);

            foreach (var advance in TripAdvanceList)
            {
                obj_list.Add(advance);
            }
            //add empty row
            if (TripAdvanceList.Count != 0)
            {
                obj_list.Add(new LorryTripClass());
            }

            return obj_list;
        }
        public static JournalClass ConvertAdvanceClass(JournalTable item)
        {
            JournalClass obj = new JournalClass();
            obj.AccountID = item.AccountId;
            obj.Id = item.Id;
            obj.TransId = item.TransId;
            obj.Debit = item.Debit;
            obj.Description = item.Description;
            obj.EntryDate = item.EntryDate;
            return obj;
        }
        public static JournalClass ConvertCreditClass(JournalTable item)
        {
            JournalClass obj = new JournalClass();
            obj.AccountID = item.AccountId;
            obj.Id = item.Id;
            obj.TransId = item.TransId;
            obj.Credit = item.Credit;
            obj.Description = item.Description;
            obj.EntryDate = item.EntryDate;
            return obj;
        }
        public string GetMonthInUrdu(DateTime? DateToConvert)
        {
            string ConvertedDate = "";
            if (DateToConvert.Value.Month == 1)
            {
                ConvertedDate = string.Format("{0}", "جنوری");
            }
            else if (DateToConvert.Value.Month == 2)
            {
                ConvertedDate = string.Format("{0}", "فروری");
            }
            else if (DateToConvert.Value.Month == 3)
            {
                ConvertedDate = string.Format("{0}", "مارچ");
            }
            else if (DateToConvert.Value.Month == 4)
            {
                ConvertedDate = string.Format("{0}", "اپریل");
            }
            else if (DateToConvert.Value.Month == 5)
            {
                ConvertedDate = string.Format("{0}", "مئ");
            }
            else if (DateToConvert.Value.Month == 6)
            {
                ConvertedDate = string.Format("{0}", "جون");
            }
            else if (DateToConvert.Value.Month == 7)
            {
                ConvertedDate = string.Format("{0}", "جولائی");
            }
            else if (DateToConvert.Value.Month == 8)
            {
                ConvertedDate = string.Format("{0}", "اگست");
            }
            else if (DateToConvert.Value.Month == 9)
            {
                ConvertedDate = string.Format("{0}", "ستمبر");
            }
            else if (DateToConvert.Value.Month == 10)
            {
                ConvertedDate = string.Format("{0}", "اکتوبر");
            }
            else if (DateToConvert.Value.Month == 11)
            {
                ConvertedDate = string.Format("{0}", "نومبر");
            }
            else if (DateToConvert.Value.Month == 12)
            {
                ConvertedDate = string.Format("{0}", "دسمبر");
            }
            return ConvertedDate;
        }

        //Delete
        public class DeleteParam
        {
            public long? LorryBillId { get; set; }
        }
        [Authorize(Roles = "DEVELOPER,ADMINISTRATOR,LORRY_BILL_DELETE")]
        public async Task<IActionResult> Delete([FromBody] DeleteParam p1)
        {
            DeleteReturn obj_return = new DeleteReturn();
            try
            {
                LorryBillTable? lbtToDelete =await _context.LorryBillTables.Where(p => p.BillNo == p1.LorryBillId).FirstOrDefaultAsync();
                if (lbtToDelete == null)
                {
                    throw new Exception("Oops! Lorry Bill not found.");
                }
                //Update Trips Data
                List<TripTable> tripsdata =await _context.TripTables.Where(p => p.LorryBillNo == p1.LorryBillId).ToListAsync();
                foreach (var item in tripsdata)
                {
                    item.LorryBillNo = null;
                }

                //Update Journal Data
                List<JournalTable> journaleditdata =await _context.JournalTables
                    .Where(p => p.LorryBillNo == p1.LorryBillId && p.EntryType == "GV" && p.TripId == null).ToListAsync();
                foreach (JournalTable item in journaleditdata)
                {
                    item.LorryBillNo = null;
                }

                //Delete Journal Data
                List<JournalTable> journaldeletedata = await _context.JournalTables.Where(p => p.LorryBillNo == p1.LorryBillId &&
                    p.EntryType != "GV").ToListAsync();
                foreach (JournalTable item in journaldeletedata)
                {
                    _context.JournalTables.Remove(item);
                }

                _context.LorryBillTables.Remove(lbtToDelete);
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
        public class DeleteReturn
        {
            public string? Message { get; set; }
        }

        //Print Excel
        public class LorryBillInExcellParam
        {
            public LorryBillInExcellParam()
            {
                TripList = new List<LorryTripClass>();
                AdvanceList = new List<JournalClass>();
                SummaryList = new List<LorryBillSummaryDto>();
            }
            public long? LorryBillId { get; set; }
            public long? LorryId { get; set; }
            public List<LorryTripClass> TripList { get; set; }
            public List<JournalClass> AdvanceList { get; set; }

            public List<LorryBillSummaryDto> SummaryList { get; set; }
        }
        public IActionResult LorryBillInExcell([FromBody] LorryBillInExcellParam p1)
        {
            LorryBillInExcellReturn obj_return = new LorryBillInExcellReturn();
            try
            {
                LorryBillTable? lbt = _context.LorryBillTables.Where(p => p.BillNo == p1.LorryBillId).FirstOrDefault();
                if (lbt == null)
                {
                    throw new Exception("Oops! Lorry Bill not found.");
                }
                AccountTable? account = _context.AccountTables.Where(p => p.AccountId == lbt.Lorry).FirstOrDefault();
                LorryTable? lorry = _context.LorryTables.Where(p => p.AccountId == lbt.Lorry).FirstOrDefault();
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    // ------------------------------------------------
                    // Creation of Title cells
                    // ------------------------------------------------
                    worksheet.Cells["A1:K1"].Merge = true;
                    worksheet.Cells["A1:K1"].Value = "یونائیٹڈ آزاد ٹرانسپورٹ کارپوریشن";
                    worksheet.Cells["A1:K1"].Style.Font.Size = 25;
                    worksheet.Cells["A1:K4"].Style.Font.Bold = true;
                    worksheet.Cells["A1:K1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["K3"].Value = "گاڑی نمبر";
                    worksheet.Cells["H3:J3"].Merge = true;
                    worksheet.Cells["H3:J3"].Value =_context.AccountTables.Where(p=>p.AccountId==p1.LorryId).Select(p=>p.Title).FirstOrDefault();
                    worksheet.Cells["H3:J3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells["F3"].Value = "برائے ماہ";
                    worksheet.Cells["E3"].Value = ConvertDateInUrdu(lbt.BillDate);
                    worksheet.Cells["C3"].Value = "گنجائش";
                    worksheet.Cells["B3"].Value = lorry.Capacity;
                    worksheet.Cells["A3"].Value = "لیٹرز";

                    worksheet.Column(1).Width = 9;
                    worksheet.Column(2).Width = 7;
                    worksheet.Column(3).Width = 7;
                    worksheet.Column(4).Width = 8;
                    worksheet.Column(5).Width = 10;
                    worksheet.Column(6).Width = 6;
                    worksheet.Column(7).Width = 10;
                    worksheet.Column(8).Width = 5;
                    worksheet.Column(9).Width = 10;
                    worksheet.Column(10).Width = 5;
                    worksheet.Column(11).Width = 12;

                    // ------------------------------------------------
                    // Creation of Trip header cells
                    // ------------------------------------------------
                    worksheet.Cells["A4"].Value = "کرایہ";
                    worksheet.Cells["B4"].Value = "کمیشن";
                    worksheet.Cells["C4"].Value = "ٹیکس";
                    worksheet.Cells["D4"].Value = "ایڈوانس";
                    worksheet.Cells["E4"].Value = "شارٹیج رقم";
                    worksheet.Cells["F4"].Value = "شارٹیج";
                    worksheet.Cells["G4"].Value = "ٹوکن نمبر";
                    worksheet.Cells["H4:J4"].Merge = true;
                    worksheet.Cells["H4:J4"].Value = "اسٹیشن";
                    worksheet.Cells["K4"].Value = "تاریخ";
                    worksheet.Cells["A4:K4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // ------------------------------------------------
                    // Populate Trip Table
                    // ------------------------------------------------
                    int row = 5; // start row
                    foreach (var item in p1.TripList)
                    {
                        worksheet.Cells[string.Format("A{0}", row)].Value = item.Freight;
                        worksheet.Cells[string.Format("A{0}:F{1}", row, row)].Style.Numberformat.Format = "0";
                        worksheet.Cells[string.Format("A{0}:F{1}", row,row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        worksheet.Cells[string.Format("B{0}", row)].Value = item.Commission;
                        
                        worksheet.Cells[string.Format("C{0}", row)].Value = item.Tax;
                        
                        worksheet.Cells[string.Format("D{0}", row)].Value = item.TripAdvance;
                        
                        worksheet.Cells[string.Format("E{0}", row)].Value = item.ShortAmount;
                        
                        worksheet.Cells[string.Format("F{0}", row)].Value = item.ShortQty;
                        
                        worksheet.Cells[string.Format("G{0}", row)].Value = item.TokenNo;
                        worksheet.Cells[string.Format("G{0}:I{1}", row,row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        worksheet.Cells[string.Format("H{0}:J{1}", row, row)].Merge = true;
                        worksheet.Cells[string.Format("H{0}:J{1}", row, row)].Value = item.DestinationTitle;
                        
                        worksheet.Cells[string.Format("K{0}", row)].Value = item.EntryDateString;
                        
                        row++;
                    }

                    worksheet.Cells[string.Format("A{0}", row)].Value = p1.TripList.Sum(p=>p.Freight).GetValueOrDefault(0);
                    worksheet.Cells[string.Format("A{0}:F{1}", row, row)].Style.Numberformat.Format = "0";
                    worksheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[string.Format("B{0}", row)].Value = p1.TripList.Sum(p => p.Commission).GetValueOrDefault(0);
                    worksheet.Cells[string.Format("C{0}", row)].Value = p1.TripList.Sum(p => p.Tax).GetValueOrDefault(0);
                    worksheet.Cells[string.Format("D{0}", row)].Value = p1.TripList.Sum(p => p.TripAdvance).GetValueOrDefault(0);
                    worksheet.Cells[string.Format("E{0}", row)].Value = p1.TripList.Sum(p => p.ShortAmount).GetValueOrDefault(0);
                    worksheet.Cells[string.Format("F{0}:K{1}",row,row)].Merge = true;
                    worksheet.Cells[string.Format("F{0}:K{1}", row, row)].Value = "ٹوٹل";
                    worksheet.Cells[string.Format("F{0}:K{1}", row, row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[string.Format("A{0}:K{1}", row, row)].Style.Font.Bold = true;
                    
                    var border = worksheet.Cells[string.Format("A4:K{0}", row)].Style.Border;
                    border.Top.Style = ExcelBorderStyle.Medium;
                    border.Bottom.Style = ExcelBorderStyle.Medium;
                    border.Left.Style = ExcelBorderStyle.Medium;
                    border.Right.Style = ExcelBorderStyle.Medium;
                    //worksheet.Cells[string.Format("A1:I{0}", row)].Style.Font.Name = "Jameel Noori Nastaleeq";
                    //worksheet.Cells[string.Format("A3:I{0}",row)].Style.Font.Size = 13;

                    ++row;
                    ++row;
                    int row_at_start_of_advance = row;

                    // ------------------------------------------------
                    // Creation of Advance header cells
                    // ------------------------------------------------
                    worksheet.Cells[string.Format("A{0}",row)].Value = "کریڈٹ";
                    worksheet.Cells[string.Format("B{0}", row)].Value = "ڈیبٹ";
                    worksheet.Cells[string.Format("C{0}:E{1}", row, row)].Merge = true;
                    worksheet.Cells[string.Format("C{0}:E{1}", row, row)].Value = "دیگر اخراجات";
                    worksheet.Cells[string.Format("F{0}:G{1}", row, row)].Merge = true;
                    worksheet.Cells[string.Format("F{0}:G{1}", row, row)].Value = "تاریخ";
                    worksheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Font.Bold = true;
                    // ------------------------------------------------
                    // Populate Advance Table
                    // ------------------------------------------------
                    foreach (var item in p1.AdvanceList)
                    {
                        ++row;
                        worksheet.Cells[string.Format("A{0}", row)].Value = item.Credit;
                        worksheet.Cells[string.Format("A{0}:B{1}", row, row)].Style.Numberformat.Format = "0";
                        worksheet.Cells[string.Format("A{0}:E{1}", row, row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        worksheet.Cells[string.Format("B{0}", row)].Value = item.Debit;

                        worksheet.Cells[string.Format("C{0}:E{1}", row, row)].Merge = true;
                        worksheet.Cells[string.Format("C{0}:E{1}", row, row)].Value = item.Description;
                        worksheet.Cells[string.Format("F{0}:G{1}", row, row)].Merge = true;
                        worksheet.Cells[string.Format("F{0}:G{1}", row, row)].Value = item.EntryDateString;
                        worksheet.Cells[string.Format("F{0}:G{1}", row, row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    ++row;
                    worksheet.Cells[string.Format("A{0}", row)].Value = p1.AdvanceList.Sum(p => p.Credit).GetValueOrDefault(0);
                    worksheet.Cells[string.Format("B{0}", row)].Value = p1.AdvanceList.Sum(p => p.Debit).GetValueOrDefault(0);
                    worksheet.Cells[string.Format("A{0}:B{1}", row, row)].Style.Numberformat.Format = "0";
                    worksheet.Cells[string.Format("A{0}:B{1}", row, row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    worksheet.Cells[string.Format("C{0}:G{1}", row, row)].Merge = true;
                    worksheet.Cells[string.Format("C{0}:G{1}", row, row)].Value = "ٹوٹل";
                    worksheet.Cells[string.Format("C{0}:G{1}", row, row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[string.Format("A{0}:G{1}", row, row)].Style.Font.Bold = true;

                    var border2 = worksheet.Cells[string.Format("A{0}:G{1}",row_at_start_of_advance, row)].Style.Border;
                    border2.Top.Style = ExcelBorderStyle.Medium;
                    border2.Bottom.Style = ExcelBorderStyle.Medium;
                    border2.Left.Style = ExcelBorderStyle.Medium;
                    border2.Right.Style = ExcelBorderStyle.Medium;


                    // ------------------------------------------------
                    // Populate Summary Table
                    // ------------------------------------------------
                    row = row_at_start_of_advance-1;
                    foreach (var item in p1.SummaryList)
                    {
                        ++row;
                        worksheet.Cells[string.Format("I{0}", row)].Value = item.Amount;
                        worksheet.Cells[string.Format("I{0}", row)].Style.Numberformat.Format = "0";
                        worksheet.Cells[string.Format("I{0}", row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        worksheet.Cells[string.Format("J{0}:K{1}", row, row)].Merge = true;
                        worksheet.Cells[string.Format("J{0}:K{1}", row, row)].Value = item.Description;
                        worksheet.Cells[string.Format("J{0}:K{1}", row, row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }
                    var border3 = worksheet.Cells[string.Format("I{0}:K{1}", row_at_start_of_advance, row)].Style.Border;
                    border3.Top.Style = ExcelBorderStyle.Medium;
                    border3.Bottom.Style = ExcelBorderStyle.Medium;
                    border3.Left.Style = ExcelBorderStyle.Medium;
                    border3.Right.Style = ExcelBorderStyle.Medium;

                    worksheet.Cells[string.Format("A1:K{0}", row)].Style.Font.Name = "Jameel Noori Nastaleeq";
                    worksheet.Cells[string.Format("A3:K{0}", row)].Style.Font.Size = 13;

                    

                    var fileContents = package.GetAsByteArray();
                    obj_return.Content = System.Convert.ToBase64String(fileContents);

                    string? LorryTitle = _context.AccountTables.Where(p => p.AccountId == lbt.Lorry).Select(p => p.Title).FirstOrDefault();
                    obj_return.FileName = string.Format("{0} {1:MMM,yyyy}", LorryTitle, lbt.BillDate);
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
        public class LorryBillInExcellReturn
        {
            public string? Message { get; set; }
            public string? Content { get; set; }
            public string? FileName { get; set; }
        }
        public string ConvertDateInUrdu(DateTime? DateToConvert)
        {
            string ConvertedDate = "";
            if (DateToConvert.Value.Month == 1)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "جنوری");
            }
            else if (DateToConvert.Value.Month == 2)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "فروری");
            }
            else if (DateToConvert.Value.Month == 3)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "مارچ");
            }
            else if (DateToConvert.Value.Month == 4)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "اپریل");
            }
            else if (DateToConvert.Value.Month == 5)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "مئ");
            }
            else if (DateToConvert.Value.Month == 6)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "جون");
            }
            else if (DateToConvert.Value.Month == 7)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "جولائی");
            }
            else if (DateToConvert.Value.Month == 8)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "اگست");
            }
            else if (DateToConvert.Value.Month == 9)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "ستمبر");
            }
            else if (DateToConvert.Value.Month == 10)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "اکتوبر");
            }
            else if (DateToConvert.Value.Month == 11)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "نومبر");
            }
            else if (DateToConvert.Value.Month == 12)
            {
                ConvertedDate = string.Format("{1} {0}", DateToConvert.Value.Year, "دسمبر");
            }
            return ConvertedDate;
        }

    }
}