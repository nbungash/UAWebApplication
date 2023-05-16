
$(document).ready(function () {

    $('#nt_advance_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All", 10, 25, 50]],
        selecte: true,
        'sDom': 't',
        selecte: true,
        "scrollY": "100px",
        "scrollCollapse": true,
        bSort: false
    });

    $('.trip_new_btn').on('click', function () {
        NewTripWindowLoaded(0);
    });

    $('.nt_company_select').change(function () {
        var company_id = parseInt($(this).val());
        $(".nt_shipping_select").empty();
        $(".nt_shipping_select").append($("<option />").val(0).text("Select..."));
        $.ajax({
            type: "POST",
            url: "/Shipping/ShippingsByCompanyList",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN", $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ CompanyId: company_id }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    $.each(data.ShippingList, function (index, item) {
                        $(".nt_shipping_select").append($("<option />").val(item.Id).text(item.Title));
                    });
                }
                else {
                    ShowInformationDialog("");
                }
            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });
        $(".nt_destination_select").empty();
        $(".nt_destination_select").append($("<option />").val(0).text("Select..."));
        $.ajax({
            type: "POST",
            url: "/Destination/DestinationsByCompanyList",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN", $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ CompanyId: company_id }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    $.each(data.DestinationList, function (index, item) {
                        $(".nt_destination_select").append($("<option />").val(item.Id).text(item.Title));
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
        $(".nt_product_select").empty();
        $(".nt_product_select").append($("<option />").val(0).text("Select..."));
        $.ajax({
            type: "POST",
            url: "/Product/ProductsByCompanyList",
            beforeSend: function (xhr) { },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ CompanyId: company_id }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    $.each(data.ProductList, function (index, item) {
                        $(".nt_product_select").append($("<option />").val(item.Id).text(item.Title));
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
    });

    $('.nt_account_group_select').change(function () {
        var selected_item_text = $(".nt_account_group_select option:selected").text();
        if (selected_item_text !== "Select...") {
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/AccountsByGroupList",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN", $('input:hidden[name="__RequestVerificationToken"]').val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        GroupType: selected_item_text,
                        IsActive: true
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".nt_account_select").empty();
                        $(".nt_account_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.AccountList, function (index, item) {
                            $(".nt_account_select").append($("<option />").val(item.AccountId).text(item.Title));
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

    var freight_value = '';
    $(".nt_freight_txt").bind('input change keypress paste', function () {
        if ($(this).val() !== freight_value) {
            freight_value = $(this).val();
            TaxCalculation();
            CommissionCalculation();
        }
    });
    var tax_percentage = '';
    $(".nt_tax_percentage_txt").bind('input change keypress paste', function () {
        if ($(this).val() !== tax_percentage) {
            tax_percentage = $(this).val();
            TaxCalculation();
        }
    });

    var commission_percentage = '';
    $(".nt_commission_percentage_txt").bind('input change keypress paste', function () {
        if ($(this).val() !== commission_percentage) {
            commission_percentage = $(this).val();
            CommissionCalculation();
        }
    });

    //add rows to table events on press Enter button
    $(".nt_description_txt").keypress(function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode === 13) {
            AddRowsToTripAdvanceTable();
        }
    });
    $(".nt_amount_txt").keypress(function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode === 13) {
            AddRowsToTripAdvanceTable();
        }
    });
    $(".nt_cheque_no_txt").keypress(function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode === 13) {
            AddRowsToTripAdvanceTable();
        }
    });

    $.contextMenu({
        selector: '#nt_advance_table tr',
        trigger: 'right',
        callback: async function (key, options) {
            var row = $("#nt_advance_table").DataTable().row(options.$trigger);
            switch (key) {
                case 'delete':
                    row.remove().draw();
                    break;
                default:
                    break
            }
        },
        items: {
            "delete": { name: "Delete", icon: "delete" },
        }
    }) 

    $('.nt_save_btn').on('click', function () {

        $('.nt_save_btn').prop('disabled', true);

        var error = false;
        var entrydate = $(".nt_entry_date_dp").val();
        if (entrydate === '' || entrydate === null) {
            ShowInformationDialog('Error', "Entry Date Missing");
            $('.nt_save_btn').prop('disabled', false);
            error = true;
        }
        var lorry = $(".nt_lorry_select").val();
        if (lorry === '' || lorry === null || lorry==0) {
            ShowInformationDialog('Error', "Lorry Missing");
            $('.nt_save_btn').prop('disabled', false);
            error = true;
        }
        var company_id = $(".nt_company_select").val();
        if (company_id === '' || company_id === null || company_id == 0) {
            ShowInformationDialog('Error', "Company Missing");
            $('.nt_save_btn').prop('disabled', false);
            error = true;
        }
        var invoicedate = $(".nt_invoice_date_dp").val();

        var quantity = $(".nt_quantity_txt").val();
        if (quantity != "") {
            quantity = parseFloat(quantity.replace(",", ""))
        }
        else {
            quantity = null;
        }
        var freight = $(".nt_freight_txt").val();
        if (freight != "") {
            freight = parseFloat(freight.replace(",", ""))
        }
        else {
            freight = null;
        }
        var commission_amount = $(".nt_commission_amount_txt").val();
        if (commission_amount != "") {
            commission_amount = parseFloat(commission_amount.replace(",", ""))
        }
        else {
            commission_amount = null;
        }
        var commission_percentage = $(".nt_commission_percentage_txt").val();
        if (commission_percentage != "") {
            commission_percentage = parseFloat(commission_percentage.replace(",", ""))
        }
        else {
            commission_percentage = null;
        }
        var tax_amount = $(".nt_tax_amount_txt").val();
        if (tax_amount != "") {
            tax_amount = parseFloat(tax_amount.replace(",", ""))
        }
        else {
            tax_amount = null;
        }
        var tax_percentage = $(".nt_tax_percentage_txt").val();
        if (tax_percentage != "") {
            tax_percentage = parseFloat(tax_percentage.replace(",", ""))
        }
        else {
            tax_percentage = null;
        }
        var shortage_qty = $(".nt_shortage_qty_txt").val();
        if (shortage_qty != "") {
            shortage_qty = parseFloat(shortage_qty.replace(",", ""))
        }
        else {
            shortage_qty = null;
        }
        var shortage_amount = $(".nt_shortage_amount_txt").val();
        if (shortage_amount != "") {
            shortage_amount = parseFloat(shortage_amount.replace(",", ""))
        }
        else {
            shortage_amount = null;
        }
        var munshiana = $(".nt_munshiana_txt").val();
        if (munshiana != "") {
            munshiana = parseFloat(munshiana.replace(",", ""))
        }
        else {
            munshiana = null;
        }

        var trip_advance_list = [];
        $("#nt_advance_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            var first_column_text = "";
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    first_column_text = $(this).text();
                }
                if (ii === 3) {
                    xyz["Description"] = $(this).text();
                }
                if (ii === 4) {
                    xyz["Credit"] = parseFloat($(this).text());
                }
                if (ii === 5) {
                    xyz["ChequeNo"] = $(this).text();
                }
                if (ii === 6) {
                    xyz["AccountId"] = $(this).text();
                }
                if (ii === 8) {
                    xyz["EntryDate"] = $(this).text();
                }
            });
            if (first_column_text != "No data available in table") {
                trip_advance_list.push(xyz);
            }
        });

        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/NewTrip/Save",
                beforeSend: function (xhr) {
                    $('.nt_save_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        TripId: trip_id_for_new_trip,
                        TokenNo: parseInt($(".nt_token_no_txt").val()),
                        Lorry: lorry,
                        LorryString: $(".nt_lorry_select option:selected").text(),
                        InvoiceDate: invoicedate == '' ? null : invoicedate,
                        EntryDate: entrydate,
                        Quantity: quantity,
                        Freight: freight,
                        Commission: commission_amount,
                        ShortQty: shortage_qty,
                        ShortAmount: shortage_amount,
                        ShippingId: $(".nt_shipping_select").val(),
                        DestinationId: $(".nt_destination_select").val(),
                        ProductId: $(".nt_product_select").val() ,
                        CommissionPercent: commission_percentage,
                        TaxPercent: tax_percentage,
                        Tax: tax_amount,
                        PartyId: company_id,
                        Munshiana: munshiana,
                        TripAdvanceList: trip_advance_list
}                ),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        if (trip_id_for_new_trip == 0) {
                            ShowInformationDialog("Information", "Saved Successfully.");
                            RefreshPage();
                        }
                        else {
                            ShowInformationDialog("Information","Updated Successfully.");
                            $('#new_trip_form').dialog('close');
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.nt_save_ajax-loader').css("visibility", "hidden");
                    $('.nt_save_btn').prop('disabled', false);
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });

    $('#new_trip_form').bind('dialogclose', function (event, ui) {
        RefreshPage();
        
    });
});

function AddRowsToTripAdvanceTable() {
    var error = false;

    var table = $('#nt_advance_table').DataTable();
    var date = $('.nt_date_dp').val();
    if (date == null || date == "") {
        ShowInformationDialog("Information","Date Missing.");
        error = true;
    }
    var account_group = $(".nt_account_group_select option:selected").text();
    if (account_group == null || account_group == "" || account_group=="Select...") {
        ShowInformationDialog("Information", "Account Group Missing.");
        error = true;
    }
    var account_id = $(".nt_account_select").val();
    if (account_id == null || account_id == "" || account_id==0) {
        ShowInformationDialog("Information", "Account Missing.");
        error = true;
    }
    var account_title = $(".nt_account_select option:selected").text();
    var description = $(".nt_description_txt").val();
    if (description == null || description == "") {
        ShowInformationDialog("Information", "Description Missing.");
        error = true;
    }
    var amount = $(".nt_amount_txt").val();
    if (amount == null || amount == "") {
        ShowInformationDialog("Information", "Amount Missing.");
        error = true;
    }
    var cheque_no = $(".nt_cheque_no_txt").val();

    if (error == false) {
        var sno = 0;
        $("#nt_advance_table > tbody > tr").each(function (i, v) {
            $(this).children('td').each(function (ii, vv) {
                if (ii === 7) {
                    var sno_float = parseFloat($(this).text());
                    if (sno < sno_float) {
                        sno = sno_float;
                    }
                }
            });
        });

        var row = $('<tr><td style= "text-align: center;padding:5px;" > ' + GetFormatedDate(date) + '</td >\
            <td style= "text-align: center;padding:5px;" > ' + account_group + '</td >\
            <td style= "text-align: left;padding:5px;" > ' + account_title + '</td >\
            <td style= "text-align: left;padding:5px;" >' + description + '</td>\
            <td style= "text-align: right;padding:5px;" > ' + amount + '</td >\
            <td style= "text-align: center;padding:5px;" >' + cheque_no + '</td>\
            <td class="d-none">' + account_id + '</td>\
            <td class="d-none">' + (++sno) + '</td>\
            <td class="d-none">' + date + '</td>\
            </tr>');
        table.row.add(row).draw();

        $(".nt_date_dp").val("");
        $(".nt_account_group_select").val("Select...");
        $(".nt_account_select").empty();
        $(".nt_description_txt").val("");
        $(".nt_amount_txt").val('');
        $(".nt_cheque_no_txt").val('');
        $('.nt_date_dp').focus();
    }
}

var trip_id_for_new_trip = 0;

function NewTripWindowLoaded(trip_id)
{
    trip_id_for_new_trip = trip_id;
    $("#new_trip_form").dialog({
        title: "New Trip",
        width: 1050,
        height: 570,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/AccountsByGroupList",
                beforeSend: function (xhr) { },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        GroupType: "PARTY",
                        IsActive: true
                    }),
                async: false,
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".nt_company_select").empty();
                        $(".nt_company_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.AccountList, function (index, item) {
                            $(".nt_company_select").append($("<option />").val(item.AccountId).text(item.Title));
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
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/AccountsByGroupList",
                beforeSend: function (xhr) { },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        GroupType: "LORRY",
                        IsActive: true
                    }),
                async: false,
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".nt_lorry_select").empty();
                        $(".nt_lorry_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.AccountList, function (index, item) {
                            $(".nt_lorry_select").append($("<option />").val(item.AccountId).text(item.Title));
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
            $.ajax({
                type: "GET",
                url: "/ChartOfAccount/AccountGroupList",
                beforeSend: function (xhr) { },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                async: false,
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".nt_account_group_select").empty();
                        $(".nt_account_group_select").append($("<option />").text("Select..."));
                        $.each(data.GroupList, function (index, item) {
                            $(".nt_account_group_select").append($("<option />").text(item));
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
            if (trip_id != 0) {
                $.ajax({
                    type: "POST",
                    url: "/NewTrip/NewTripWindowLoaded",
                    beforeSend: function (xhr) {
                        $('.nt_save_ajax-loader').css("visibility", "visible");
                    },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify(
                        {
                            TripId: trip_id
                        }),
                    success: function (response) {
                        var data = JSON.parse(response);
                        if (data.Message == "OK") {
                            jQuery.ajaxSetup({ async: false });
                            $('.nt_company_select').val(data.TripObj.PartyId).trigger("change");
                            jQuery.ajaxSetup({ async: true });

                            $('.nt_entry_date_dp').val(GetFormatedDate2(data.TripObj.EntryDate));
                            $('.nt_lorry_select').val(data.TripObj.Lorry);
                            $('.nt_shipping_select').val(data.TripObj.ShippingId);
                            $('.nt_destination_select').val(data.TripObj.DestinationId);
                            $('.nt_product_select').val(data.TripObj.ProductId);
                            $('.nt_munshiana_txt').val(data.Munshiana);

                            $('.nt_invoice_date_dp').val(GetFormatedDate2(data.TripObj.InvoiceDate));
                            $('.nt_token_no_txt').val(data.TripObj.TokenNo);
                            $('.nt_quantity_txt').val(data.TripObj.Quantity);
                            $('.nt_freight_txt').val(data.TripObj.Freight);
                            $('.nt_tax_percentage_txt').val(data.TripObj.TaxPercent);
                            $('.nt_tax_amount_txt').val(data.TripObj.Tax);
                            $('.nt_commission_percentage_txt').val(data.TripObj.CommissionPercent);
                            $('.nt_commission_amount_txt').val(data.TripObj.Commission);
                            $('.nt_shortage_qty_txt').val(data.TripObj.ShortQty);
                            $('.nt_shortage_amount_txt').val(data.TripObj.ShortAmount);

                            $(".nt_advance_table").DataTable().clear().draw();
                            var table = $(".nt_advance_table").DataTable();
                            $.each(data.TripAdvanceList, function (index, item) {
                                var row = $('<tr><td style= "text-align: center;padding:5px;" > ' + GetFormatedDate(item.EntryDate) + '</td >\
                                    <td style= "text-align: center;padding:5px;" > ' + item.AccountGroup + '</td >\
                                    <td style= "text-align: left;padding:5px;" > ' + item.AccountTitle + '</td >\
                                    <td style= "text-align: left;padding:5px;" >' + item.Description + '</td>\
                                    <td style= "text-align: right;padding:5px;" > ' + item.Credit + '</td >\
                                    <td style= "text-align: center;padding:5px;" >' + item.ChequeNo + '</td>\
                                    <td class="d-none">' + item.AccountId + '</td>\
                                    <td class="d-none">' + item.SNo + '</td>\
                                    <td class="d-none">' + item.EntryDate + '</td>\
                                    </tr>');
                                table.row.add(row);

                            });
                            table.draw();
                        }
                        else {
                            ShowInformationDialog("Error", data.Message);
                        }
                    },
                    complete: function () {
                        $('.nt_save_ajax-loader').css("visibility", "hidden");
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}

function TaxCalculation() {
    var freight = $('.nt_freight_txt').val();
    var percentage = $('.nt_tax_percentage_txt').val();
    var tax = 0;
    if (freight !== null && freight != "" && percentage !== null && percentage != "") {
        tax = (parseFloat(freight.replace(",", "")) * parseFloat(percentage.replace(",", "")))/100;
        $('.nt_tax_amount_txt').val(tax.toFixed(2));
    }
    else {
        $('.nt_tax_amount_txt').val("");
    }
}
function CommissionCalculation() {
    var freight = $('.nt_freight_txt').val();
    var percentage = $('.nt_commission_percentage_txt').val();
    var commission = 0;
    if (freight !== null && freight != "" && percentage !== null && percentage != "") {
        commission = (parseFloat(freight.replace(",", "")) * parseFloat(percentage.replace(",", ""))) / 100;
        $('.nt_commission_amount_txt').val(commission.toFixed(2));
    }
    else {
        $('.nt_commission_amount_txt').val("");
    }
}

function RefreshPage() {
    $(".nt_company_select").val(0);
    $(".nt_entry_date_dp").val("");
    $(".nt_lorry_select").val(0);
    $(".nt_shipping_select").val("");
    $(".nt_shipping_select").empty();
    $(".nt_destination_select").val("");
    $(".nt_destination_select").empty();
    $(".nt_product_select").val("");
    $(".nt_product_select").empty();
    $(".nt_munshiana_txt").val("");
    $(".nt_invoice_date_dp").val("");
    $(".nt_token_no_txt").val("");
    $(".nt_quantity_txt").val("");
    $(".nt_freight_txt").val("");

    $(".nt_tax_percentage_txt").val("");
    $(".nt_tax_amount_txt").val("");
    $(".nt_commission_percentage_txt").val("");
    $(".nt_commission_amount_txt").val("");
    $(".nt_shortage_qty_txt").val("");
    $(".nt_shortage_amount_txt").val("");

    $(".nt_date_dp").val("");
    $(".nt_account_group_select").val(0);
    $(".nt_account_select").val("");
    $(".nt_account_select").empty();
    $(".nt_description_txt").val("");
    $(".nt_amount_txt").val("");
    $(".nt_cheque_no_txt").val("");

    $("#nt_advance_table").DataTable().clear().draw();

    trip_id_for_new_trip = 0;
}