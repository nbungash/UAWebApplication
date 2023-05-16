
$(document).ready(function () {

    $('#lb2_trip_table').dataTable({
        "pagingType": "simple",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": "185px",
        "scrollCollapse": true,
        bSort: true,
        searching: false,
        paging: false,
        info: false
    });
    $('#lb2_advance_table').dataTable({
        "pagingType": "simple",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": "270px",
        "scrollCollapse": true,
        bSort: true,
        searching: false,
        paging: false,
        info: false
    });
    $('#lb2_summary_table').dataTable({
        "pagingType": "simple",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": "270px",
        "scrollCollapse": true,
        bSort: false,
        searching: false,
        paging: false,
        info: false
    });

    GetAccountsByGroup("LORRY", true, ".lb2_lorry_select");

    $('.lb2_lorry_select').change(function () {

        RefreshLorryBillPage();

        let selectedLorryId = $(this).val();
        if (selectedLorryId != 0) {
            $.ajax({
                type: "POST",
                url: "/LorryBill/YearsList",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ SelectedLorryId: selectedLorryId }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                       $(".lb2_year_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.YearList, function (index, item) {
                            $(".lb2_year_select").append($("<option />").val(item).text(item));
                        });
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('.lb2_year_select').change(function () {
        let selectedYear = $(this).val();
        let selectedLorry = $('.lb2_lorry_select').val();
        if (selectedYear != 0 && selectedLorry != 0) {
            $.ajax({
                type: "POST",
                url: "/LorryBill/LorryBillList",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Year: selectedYear,
                        LorryId: selectedLorry
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".lb2_bill_select").empty();
                        $(".lb2_bill_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.LorryBillList, function (index, item) {
                            $(".lb2_bill_select").append($("<option />").val(item.BillNo).text(item.BillDateString));
                        });
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            ShowInformationDialog("Information","Oops! Selected Lorry and Year Missing.");
        }
    });

    $('.lb2_view_btn').on('click', function () {

        var error = false;
        var lorry = $('.lb2_lorry_select').val();
        if (lorry === null || lorry === "" || lorry == 0) {
            ShowInformationDialog('Error', "Lorry Missing");
            error = true;
        }
        var billId = $('.lb2_bill_select').val();
        if (billId === null || billId === "" || billId==0) {
            ShowInformationDialog('Error', "Bill Id Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/LorryBill/ViewLorryBill",
                beforeSend: function (xhr) {
                    $('.lb2_view_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        LorryId: lorry,
                        BillId: billId
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        PopulateLorryBillGrid(data.TripList, data.AdvanceList, data.SummaryList);
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $(".lb2_table").DataTable().clear().draw();
                    $('.lb2_view_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('.lb2_update_btn').on('click', function () {

        var error = false;
        var billId = $('.lb2_bill_select').val();
        if (billId === null || billId === "" || billId == 0) {
            ShowInformationDialog('Error', "Bill Id Missing");
            error = true;
        }
        if (error === false) {
            NewLorryBillLoaded(billId);
        }
    });

    $('.lb2_delete_btn').on('click', function () {

        var error = false;
        var billId = $('.lb2_bill_select').val();
        if (billId === null || billId === "" || billId == 0) {
            ShowInformationDialog('Error', "Bill Id Missing");
            error = true;
        }
        if (error === false) {
            var question = "Are You Sure You want to Delete ?";
            confirmation(question).then(function (answer) {
                var ansbool = (String(answer) == "true");
                if (ansbool) {
                    $.ajax({
                        type: "POST",
                        url: "/LorryBill/Delete",
                        beforeSend: function (xhr) { $('.lb2_view_ajax-loader').css("visibility", "visible"); },
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({ LorryBillId: billId }),
                        success: function (response) {
                            var data = JSON.parse(response);
                            if (data.Message == "OK") {
                                ShowInformationDialog('Information', "Deleted Successfully.");
                                $('.lb2_lorry_select').val(0);
                                RefreshLorryBillPage();
                                $('#dialog-confirm').dialog("close");
                            }
                            else {
                                ShowInformationDialog("Information", data.Message);
                            }
                        },
                        complete: function () {
                            $('.lb2_view_ajax-loader').css("visibility", "hidden");
                        },
                        failure: function (response) {
                            alert('Some thing wrong');
                        }
                    });
                }
            });
        }
    });

    $('.lb2_print_excel_btn').on('click', function () {

        var error = false;
        var lorry = $('.lb2_lorry_select').val();
        if (lorry === null || lorry === "" || lorry == 0) {
            ShowInformationDialog('Error', "Lorry Missing");
            error = true;
        }
        var billId = $('.lb2_bill_select').val();
        if (billId === null || billId === "" || billId == 0) {
            ShowInformationDialog('Error', "Bill Id Missing");
            error = true;
        }
        

        var tripList = [];
        $("#lb2_trip_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["TripID"] = text;
                    }
                }
                else if (ii === 1) {
                    xyz["EntryDateString"] = $(this).text();
                }
                else if (ii === 2) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["DestinationTitle"] = text;
                    }
                }
                else if (ii === 3) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["TokenNo"] = parseInt(text);
                    }
                }
                else if (ii === 4) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["ShortQty"] = parseFloat(text);
                    }
                }
                else if (ii === 5) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["ShortAmount"] = parseFloat(text);
                    }
                }
                else if (ii === 6) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["TripAdvance"] = parseFloat(text);
                    }
                }
                else if (ii ===7) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Tax"] = parseFloat(text);
                    }
                }
                else if (ii === 8) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Commission"] = parseFloat(text);
                    }
                }
                else if (ii === 9) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Freight"] = parseFloat(text);
                    }
                }
            });
            tripList.push(xyz);
        });

        var advanceList = [];
        $("#lb2_advance_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Id"] = text;
                    }
                }
                else if (ii === 1) {
                    xyz["EntryDateString"] = $(this).text();
                }
                else if (ii === 2) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Description"] = text;
                    }
                }
                else if (ii === 3) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Debit"] = parseFloat(text);
                    }
                }
                else if (ii === 4) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Credit"] = parseFloat(text);
                    }
                }
            });
            advanceList.push(xyz);
        });

        var summaryList = [];
        $("#lb2_summary_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Amount"] = parseFloat(text);
                    }
                }
                else if (ii === 1) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Description"] = text;
                    }
                }
                
            });
            summaryList.push(xyz);
        });


        if (error === false) {
            $.ajax({
                url: "/LorryBill/LorryBillInExcell",
                type: "POST",
                beforeSend: function (xhr) {
                    $('.lb2_view_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        LorryBillId: billId,
                        LorryId: lorry,
                        TripList: tripList,
                        AdvanceList: advanceList,
                        SummaryList:summaryList
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        var byteCharacters = atob(data.Content);
                        var byteNumbers = new Array(byteCharacters.length);
                        for (var i = 0; i < byteCharacters.length; i++) {
                            byteNumbers[i] = byteCharacters.charCodeAt(i);
                        }
                        var byteArray = new Uint8Array(byteNumbers);
                        var blob = new Blob([byteArray], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
                        var downloadUrl = window.URL.createObjectURL(blob);
                        var anchor = document.createElement("a");
                        anchor.href = downloadUrl;
                        anchor.download = data.FileName;// "myExcelFile.xlsx";
                        document.body.appendChild(anchor);
                        anchor.click();
                        setTimeout(function () {
                            document.body.removeChild(anchor);
                            window.URL.revokeObjectURL(downloadUrl);
                        }, 100);
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.lb2_view_ajax-loader').css("visibility", "hidden");
                }
            });
        }
    });
});

function PopulateLorryBillGrid(TripList, AdvanceList, SummaryList) {
    //Trip List
    $(".lb2_trip_table").DataTable().clear().draw();
    var tripTable = $(".lb2_trip_table").DataTable();
    let total_trip_advance = 0;
    let total_shortage = 0;
    let total_wh_tax = 0;
    let total_commission = 0;
    let total_freight = 0;
    $.each(TripList, function (index, item) {
        var row = $('<tr style="min-height:20px;"><td style="text-align: center;padding:0px;">' + ((item.TripID === null || item.TripID === 0) ? "" : item.TripID) + '</td>\
            <td style= "text-align: center;padding:0px;" > ' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate)) + '</td>\
            <td style="text-align: left;padding:0px;" >' + ((item.DestinationTitle === null) ? "" : item.DestinationTitle) + '</td>\
            <td style="text-align: center;padding:0px;">' + ((item.TokenNo === null) ? "" : item.TokenNo)  + '</td>\
            <td style="text-align: right;padding:0px;">' + ((item.ShortQty === null) ? "" : item.ShortQty.toFixed(2)) + '</td>\
            <td style="text-align: right;padding:0px;">' + ((item.ShortAmount === null) ? "" : item.ShortAmount.toFixed(0)) + '</td>\
            <td style="text-align: right;padding:0px;">' + ((item.TripAdvance === null) ? "" : item.TripAdvance.toFixed(0)) + '</td>\
            <td style="text-align: right;padding:0px;">' + ((item.Tax === null) ? "" : item.Tax.toFixed(0)) + '</td>\
            <td style="text-align: right;padding:0px;">' + ((item.Commission === null) ? "" : item.Commission.toFixed(0)) + '</td>\
            <td style="text-align: right;padding:0px;">' + ((item.Freight === null) ? "" : item.Freight.toFixed(0)) + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:NewTripWindowLoaded('+ item.TripID + ');"><i class="fa fa-pencil" title="Update"></i></a></td>\</tr>');
        tripTable.row.add(row);
        if (item.TripAdvance != null && item.TripAdvance != "") {
            total_trip_advance += parseFloat(item.TripAdvance);
        }
        if (item.ShortAmount != null && item.ShortAmount != "") {
            total_shortage += parseFloat(item.ShortAmount);
        }
        if (item.Tax != null && item.Tax != "") {
            total_wh_tax += parseFloat(item.Tax);
        }
        if (item.Commission != null && item.Commission != "") {
            total_commission += parseFloat(item.Commission);
        }
        if (item.Freight != null && item.Freight != "") {
            total_freight += parseFloat(item.Freight);
        }
    });
    tripTable.draw();
    $("#lb2_total_trip_advance").text(total_trip_advance.toFixed(0));
    $("#lb2_total_shortages").text(total_shortage.toFixed(0));
    $("#lb2_total_wh_tax").text(total_wh_tax.toFixed(0));
    $("#lb2_total_commission").text(total_commission.toFixed(0));
    $("#lb2_total_freight").text(total_freight.toFixed(0));


    //Advance List
    $(".lb2_advance_table").DataTable().clear().draw();
    var advanceTable = $(".lb2_advance_table").DataTable();
    let total_debit = 0;
    let total_credit = 0;
    $.each(AdvanceList, function (index, item) {
        var row = $('<tr><td style="text-align: center;padding:2px;">' + ((item.Id === null) ? "" : item.Id)  + '</td>\
            <td style= "text-align: center;padding:2px;" > ' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate))  + '</td>\
            <td style="text-align: left;padding:2px;">' + item.Description + '</td>\
            <td style="text-align: right;padding:2px;" >' + ((item.Debit === null) ? "" : item.Debit.toFixed(0)) + '</td>\
            <td style="text-align: right;padding:2px;" >' + ((item.Credit === null) ? "" : item.Credit.toFixed(0)) + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:NewTransactionWindowLoaded('+ item.TransId + ');"><i class="fa fa-pencil" title="Update"></i></a></td>\</tr>');
        advanceTable.row.add(row);
        if (item.Debit != null && item.Debit != "") {
            total_debit += parseFloat(item.Debit);
        }
        if (item.Credit != null && item.Credit != "") {
            total_credit += parseFloat(item.Credit);
        }
    });
    advanceTable.draw();
    $("#lb2_advance_total_debit").text(total_debit.toFixed(0));
    $("#lb2_advance_total_credit").text(total_credit.toFixed(0));

    //Summary List
    $(".lb2_summary_table").DataTable().clear().draw();
    var summaryTable = $(".lb2_summary_table").DataTable();
    $.each(SummaryList, function (index, item) {
        var row4 = $('<tr><td style= "text-align: right;padding:0px;" > ' + item.Amount.toFixed(0) + '</td>\
            <td style = "text-align: right;padding:0px;" > ' + item.Description + '</td ></tr>');
        summaryTable.row.add(row4);
    });
    summaryTable.draw();
}

function RefreshLorryBillPage() {

    
    $('.lb2_year_select').val(0);
    $('.lb2_year_select').empty();

    $('.lb2_bill_select').val(0);
    $('.lb2_bill_select').empty();

    $(".lb2_trip_table").DataTable().clear().draw();
    $("#lb2_total_trip_advance").text("");
    $("#lb2_total_shortages").text("");
    $("#lb2_total_wh_tax").text("");
    $("#lb2_total_commission").text("");
    $("#lb2_total_freight").text("");

    $(".lb2_advance_table").DataTable().clear().draw();
    $("#lb2_advance_total_debit").text("");
    $("#lb2_advance_total_credit").text("");

    $(".lb2_summary_table").DataTable().clear().draw();
}
