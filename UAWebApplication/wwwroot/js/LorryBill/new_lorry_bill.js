
$(document).ready(function () {

    $('#nlb_trip_table').dataTable({
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
    $('#nlb_advance_table').dataTable({
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
    $('#nlb_summary_table').dataTable({
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

    $('.lb2_new_btn').on('click', function () {

        NewLorryBillLoaded(0);
    });

    $('.nlb_view_btn').on('click', function () {
        var error = false;
        var lorry = $('.nlb_lorry_select').val();
        if (lorry === null || lorry === "" || lorry == 0) {
            ShowInformationDialog('Error', "Lorry Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/NewLorryBill/ViewRecords",
                beforeSend: function (xhr) {
                    $('.nlb_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ LorryId: lorry }),
                success: function (response) {
                    var data = JSON.parse(response);

                    if (data.Message === "OK") {
                        
                        //Trip List
                        $(".nlb_trip_table").DataTable().clear().draw();
                        var tripTable = $(".nlb_trip_table").DataTable();
                        let total_trip_advance = 0;
                        let total_shortage = 0;
                        let total_wh_tax = 0;
                        let total_commission = 0;
                        let total_freight = 0;
                        $.each(data.TripList, function (index, item) {
                            var row = $('<tr style="min-height:20px;">\
                                <td style="text-align: center;padding:2px;"><input id="nlb_trip_check" type="checkbox" /></td>\
                                <td style="text-align: center;padding:2px;">' + ((item.TripID === null || item.TripID === 0) ? "" : item.TripID) + '</td>\
                                <td style= "text-align: center;padding:2px;" > ' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate)) + '</td>\
                                <td style="text-align: left;padding:2px;" >' + ((item.DestinationTitle === null) ? "" : item.DestinationTitle) + '</td>\
                                <td style="text-align: center;padding:2px;">' + ((item.TokenNo === null) ? "" : item.TokenNo) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.ShortQty === null) ? "" : item.ShortQty) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.ShortAmount === null) ? "" : item.ShortAmount.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.TripAdvance === null) ? "" : item.TripAdvance.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.Tax === null) ? "" : item.Tax.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.Commission === null) ? "" : item.Commission.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.Freight === null) ? "" : item.Freight.toFixed(0)) + '</td>\
                                <td style="text-align: center;padding:5px;">\
                                    <a href="javascript:NewTripWindowLoaded('+ item.TripID + ');"><i class="fa fa-pencil" title="Update"></i></a></td>\</tr>');
                            tripTable.row.add(row);
                        });
                        tripTable.draw();
                        $("#nlb_total_trip_advance").text(total_trip_advance.toFixed(0));
                        $("#nlb_total_shortages").text(total_shortage.toFixed(0));
                        $("#nlb_total_wh_tax").text(total_wh_tax.toFixed(0));
                        $("#nlb_total_commission").text(total_commission.toFixed(0));
                        $("#nlb_total_freight").text(total_freight.toFixed(0));


                        //Advance List
                        $(".nlb_advance_table").DataTable().clear().draw();
                        var advanceTable = $(".nlb_advance_table").DataTable();
                        let total_debit = 0;
                        let total_credit = 0;
                        $.each(data.AdvanceList, function (index, item) {
                            var row = $('<tr><td style="text-align: center;padding:2px;"><input id="nlb_advance_check" type="checkbox" /></td>\
                                <td style="text-align: center;padding:2px;">' + ((item.Id === null) ? "" : item.Id) + '</td>\
                                <td style= "text-align: center;padding:2px;" > ' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate)) + '</td>\
                                <td style="text-align: left;padding:2px;">' + item.Description + '</td>\
                                <td style="text-align: right;padding:2px;" >' + ((item.Debit === null) ? "" : item.Debit.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;" >' + ((item.Credit === null) ? "" : item.Credit.toFixed(0)) + '</td>\
                                <td style="text-align: center;padding:5px;">\
                                    <a href="javascript:NewTransactionWindowLoaded('+ item.TransId + ');"><i class="fa fa-pencil" title="Update"></i></a></td>\</tr>');
                            advanceTable.row.add(row);
                        });
                        advanceTable.draw();
                        $("#nlb_advance_total_debit").text(total_debit.toFixed(0));
                        $("#nlb_advance_total_credit").text(total_credit.toFixed(0));

                    }
                    else {
                        $(".nlb_trip_table").DataTable().clear().draw();
                        $(".nlb_advance_table").DataTable().clear().draw();
                        $(".nlb_summary_table").DataTable().clear().draw();
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.nlb_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $(document).on('change', '#nlb_trip_all_check', function () {
        var check1 = $('#nlb_trip_all_check').is(":checked");
        $("#nlb_trip_table > tbody > tr").each(function (i, v) {
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    $(this).find('input[type="checkbox"]').prop('checked', check1);

                }

            });
        });
        ViewTotal();
    });

    $(document).on('change', '#nlb_advance_all_check', function () {
        var check1 = $('#nlb_advance_all_check').is(":checked");
        $("#nlb_advance_table > tbody > tr").each(function (i, v) {
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    $(this).find('input[type="checkbox"]').prop('checked', check1);

                }

            });
        });
        ViewTotal();
    });

    $(document).on('change', '#nlb_trip_check', function () {
        ViewTotal();
    });

    $(document).on('change', '#nlb_advance_check', function () {
        ViewTotal();
    });

    $(".nlb_bill_charges_txt").bind('input change keypress paste', function () {
        ViewTotal();
    });

    $('.nlb_save_btn').on('click', function () {
        var error = false;
        var lorry = $('.nlb_lorry_select').val();
        if (lorry === null || lorry === "" || lorry == 0) {
            ShowInformationDialog('Error', "Lorry Missing");
            error = true;
        }
        var bill_date = $('.nlb_bill_date_dp').val();
        if (bill_date === null || bill_date === "") {
            ShowInformationDialog('Error', "Bill Date Missing");
            error = true;
        }
        var billCharges_amount = 0;
        if ($('.nlb_bill_charges_txt').val() !== "") {
            billCharges_amount = parseFloat($('.nlb_bill_charges_txt').val());
        }
        var tripList = [];
        $("#nlb_trip_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            var check1 = false;
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    check1 = false;
                    var $chkbox = $(this).find('input[type="checkbox"]');
                    if ($chkbox.length) {
                        var status = $chkbox.prop('checked');
                        if (status === true) {
                            check1 = true;
                        }
                    }
                }
                if (ii === 1) {
                    xyz["TripID"] = $(this).text();
                }
            });
            if (check1 === true) {
                tripList.push(xyz);
            }
        });

        var advanceList = [];
        $("#nlb_advance_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            var check1 = false;
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    check1 = false;
                    var $chkbox = $(this).find('input[type="checkbox"]');
                    if ($chkbox.length) {
                        var status = $chkbox.prop('checked');
                        if (status === true) {
                            check1 = true;
                        }
                    }
                }
                if (ii === 1) {
                    xyz["Id"] = $(this).text();
                }
            });
            if (check1 === true) {
                advanceList.push(xyz);
            }
        });


        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/NewLorryBill/Save",
                beforeSend: function (xhr) {
                    $('.nlb_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        LorryBillId: bill_id_for_new_lorry_bill,
                        LorryId: lorry,
                        BillDate: bill_date,
                        BillCharges: billCharges_amount,
                        TripList: tripList,
                        AdvanceList: advanceList
                    }),
                success: function (response) {
                    var data = JSON.parse(response);

                    if (data.Message === "OK") {
                        if (bill_id_for_new_lorry_bill == 0) {
                            ShowInformationDialog("Information", "Saved Successfully.");

                            $('.nlb_lorry_select').val(0);
                            $('.nlb_bill_charges_txt').val("");
                            $('.nlb_bill_date_dp').val("");
                            //Trip List
                            $(".nlb_trip_table").DataTable().clear().draw();
                            $("#nlb_total_trip_advance").text("");
                            $("#nlb_total_shortages").text("");
                            $("#nlb_total_wh_tax").text("");
                            $("#nlb_total_commission").text("");
                            $("#nlb_total_freight").text("");

                            //Advance List
                            $(".nlb_advance_table").DataTable().clear().draw();
                            $("#nlb_advance_total_debit").text("");
                            $("#nlb_advance_total_credit").text("");

                            $(".nlb_summary_table").DataTable().clear().draw();
                        }
                        else {
                            ShowInformationDialog("Information", "Updated Successfully.");
                            $('.new_lorry_bill_form').dialog('close');
                        }
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.nlb_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('#new_lorry_bill_form').bind('dialogclose', function (event, ui) {

        bill_id_for_new_lorry_bill = 0;
        $(".nlb_trip_table").DataTable().clear().draw();
        $("#nlb_total_trip_advance").text("");
        $("#nlb_total_shortages").text("");
        $("#nlb_total_wh_tax").text("");
        $("#nlb_total_commission").text("");
        $("#nlb_total_freight").text("");
        $(".nlb_advance_table").DataTable().clear().draw();
        $("#nlb_advance_total_debit").text("");
        $("#nlb_advance_total_credit").text("");
        $(".nlb_summary_table").DataTable().clear().draw();

        $(".nlb_bill_charges_txt").val("");
        $(".nlb_bill_date_dp").val("");
        $(".nlb_lorry_select").val(0);
    });
    
    
});

var bill_id_for_new_lorry_bill = 0;
    
function NewLorryBillLoaded(lorry_bill_id) {

    bill_id_for_new_lorry_bill = lorry_bill_id;
    $("#new_lorry_bill_form").dialog({
        title: "New Lorry Bill",
        width: 1200,
        height: 560,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $(".nlb_trip_table").DataTable().clear().draw();
            $(".nlb_advance_table").DataTable().clear().draw();
            $(".nlb_summary_table").DataTable().clear().draw();
            GetAccountsByGroup("LORRY", true, ".nlb_lorry_select");
            $(".nlb_lorry_select").prop('disabled', false);
            $(".nlb_view_btn").prop('disabled', false);
            if (lorry_bill_id != 0) {
                $.ajax({
                    type: "POST",
                    url: "/NewLorryBill/WindowLoaded",
                    beforeSend: function (xhr) {
                        $('.nlb_ajax-loader').css("visibility", "visible");
                    },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ LorryBillId: lorry_bill_id }),
                    success: function (response) {
                        var data = JSON.parse(response);

                        if (data.Message === "OK") {
                            $(".nlb_lorry_select").prop('disabled', true);
                            $(".nlb_view_btn").prop('disabled', true);
                            $(".nlb_lorry_select").val(data.LorryId);
                            $(".nlb_bill_date_dp").val(GetFormatedDate2(data.BillDate));
                            $(".nlb_bill_charges_txt").val(data.BillCharges);

                            //Trip List
                            var tripTable = $(".nlb_trip_table").DataTable();
                            $.each(data.TripList, function (index, item) {
                                var row = $('<tr style="min-height:20px;">\
                                <td style="text-align: center;padding:2px;"><input id="nlb_trip_check" type="checkbox" '+ item.BilledChecked +' /></td>\
                                <td style="text-align: center;padding:2px;">' + ((item.TripID === null || item.TripID === 0) ? "" : item.TripID) + '</td>\
                                <td style= "text-align: center;padding:2px;" > ' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate)) + '</td>\
                                <td style="text-align: left;padding:2px;" >' + ((item.DestinationTitle === null) ? "" : item.DestinationTitle) + '</td>\
                                <td style="text-align: center;padding:2px;">' + ((item.TokenNo === null) ? "" : item.TokenNo) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.ShortQty === null) ? "" : item.ShortQty) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.ShortAmount === null) ? "" : item.ShortAmount.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.TripAdvance === null) ? "" : item.TripAdvance.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.Tax === null) ? "" : item.Tax.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.Commission === null) ? "" : item.Commission.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;">' + ((item.Freight === null) ? "" : item.Freight.toFixed(0)) + '</td>\
                                </tr>');
                                tripTable.row.add(row);
                            });
                            tripTable.draw();
                            
                            //Advance List
                            var advanceTable = $(".nlb_advance_table").DataTable();
                            $.each(data.AdvanceList, function (index, item) {
                                var row = $('<tr><td style="text-align: center;padding:2px;"><input id="nlb_advance_check" type="checkbox" ' + item.BilledChecked +' /></td>\
                                <td style="text-align: center;padding:2px;">' + ((item.Id === null) ? "" : item.Id) + '</td>\
                                <td style= "text-align: center;padding:2px;" > ' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate)) + '</td>\
                                <td style="text-align: left;padding:2px;">' + item.Description + '</td>\
                                <td style="text-align: right;padding:2px;" >' + ((item.Debit === null) ? "" : item.Debit.toFixed(0)) + '</td>\
                                <td style="text-align: right;padding:2px;" >' + ((item.Credit === null) ? "" : item.Credit.toFixed(0)) + '</td></tr>');
                                advanceTable.row.add(row);
                            });
                            advanceTable.draw();

                            ViewTotal();
                        }
                        else {
                            
                            ShowInformationDialog('Error', data.Message);
                        }
                    },
                    complete: function () {
                        $('.nlb_ajax-loader').css("visibility", "hidden");
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}

function ViewTotal() {

    let total_trip_advance = 0;
    let total_shortage = 0;
    let total_wh_tax = 0;
    let total_commission = 0;
    let total_freight = 0;
    $("#nlb_trip_table > tbody > tr").each(function (i, v) {
        var check1 = false;
        let shortage = 0;
        let trip_advance = 0;
        let wh_tax = 0;
        let commission = 0;
        let freight = 0;
        $(this).children('td').each(function (ii, vv) {
            if (ii === 0) {
                check1 = false;
                var $chkbox = $(this).find('input[type="checkbox"]');
                if ($chkbox.length) {
                    var status = $chkbox.prop('checked');
                    if (status === true) {
                        check1 = true;
                    }
                }
            }
            if (ii === 6) {
                let text = $(this).text();
                if (text != null && text != "null" && text != "") {
                    shortage = parseFloat($(this).text());
                }
            }
            if (ii === 7) {
                let text = $(this).text();
                if (text != null && text != "null" && text != "") {
                    trip_advance = parseFloat($(this).text());
                }
            }
            if (ii === 8) {
                let text = $(this).text();
                if (text != null && text != "null" && text != "") {
                    wh_tax = parseFloat($(this).text());
                }
            }
            if (ii === 9) {
                let text = $(this).text();
                if (text != null && text != "null" && text != "") {
                    commission = parseFloat($(this).text());
                }
            }
            if (ii === 10) {
                let text = $(this).text();
                if (text != null && text != "null" && text != "") {
                    freight = parseFloat($(this).text());
                }
            }
        });
        if (check1 === true) {
            total_shortage += shortage;
            total_trip_advance += trip_advance;
            total_wh_tax += wh_tax;
            total_commission += commission;
            total_freight += freight;
        }
    });
    $("#nlb_total_trip_advance").text(total_trip_advance.toFixed(0));
    $("#nlb_total_shortages").text(total_shortage.toFixed(0));
    $("#nlb_total_wh_tax").text(total_wh_tax.toFixed(0));
    $("#nlb_total_commission").text(total_commission.toFixed(0));
    $("#nlb_total_freight").text(total_freight.toFixed(0));

    let total_debit = 0;
    let total_credit = 0;
    $("#nlb_advance_table > tbody > tr").each(function (i, v) {
        var check1 = false;
        let debit = 0;
        let credit = 0;
        $(this).children('td').each(function (ii, vv) {
            if (ii === 0) {
                check1 = false;
                var $chkbox = $(this).find('input[type="checkbox"]');
                if ($chkbox.length) {
                    var status = $chkbox.prop('checked');
                    if (status === true) {
                        check1 = true;
                    }
                }
            }
            if (ii === 4) {
                let text = $(this).text();
                if (text != null && text != "null" && text != "") {
                    debit = parseFloat($(this).text());
                }
            }
            if (ii === 5) {
                let text = $(this).text();
                if (text != null && text != "null" && text != "") {
                    credit = parseFloat($(this).text());
                }
            }
            
        });
        if (check1 === true) {
            total_debit += debit;
            total_credit += credit;
        }
    });
    $("#nlb_advance_total_debit").text(total_debit.toFixed(0));
    $("#nlb_advance_total_credit").text(total_credit.toFixed(0));

    let billChargesAmount = 0;
    if ($('.nlb_bill_charges_txt').val() != "") {
        billChargesAmount = parseFloat($('.nlb_bill_charges_txt').val());
    }
    let payableAmount = total_freight + total_credit - total_commission - total_wh_tax - total_shortage - total_trip_advance
        - billChargesAmount - total_debit;

    var summaryList = [];
    summaryList.push({ Description: "Total Freight", Amount: total_freight });
    summaryList.push({ Description: "Other Credit", Amount: total_credit });
    summaryList.push({ Description: "Less Total Commission", Amount: total_commission + total_wh_tax });
    summaryList.push({ Description: "Less Total Shortage Amount", Amount: total_shortage });
    summaryList.push({ Description: "Less Total Trip Advance", Amount: total_trip_advance });
    summaryList.push({ Description: "Less Bill Charges", Amount: billChargesAmount });
    summaryList.push({ Description: "Less Other Advance", Amount: total_debit });
    summaryList.push({Description: "Balance Payable(Current Month)", Amount: payableAmount });

    $(".nlb_summary_table").DataTable().clear().draw();
    var summaryTable = $(".nlb_summary_table").DataTable();
    $.each(summaryList, function (index, item) {
        var row = $('<tr><td style="text-align: right;padding:2px;">' + item.Description + '</td>\
            <td style= "text-align: right;padding:2px;" > ' + item.Amount + '</td></tr>');
        summaryTable.row.add(row);
    });
    summaryTable.draw();

}