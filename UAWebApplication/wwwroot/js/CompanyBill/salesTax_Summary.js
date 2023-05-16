$(document).ready(function () {
    //viewTable
    $('#sts_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All", 10, 25, 50]],
        "select": false,
        "bSort": false,
        "scrollY": "300px",
        "scrollCollapse": true,
    });

    //Get Year
    $.ajax({
        type: "GET",
        async: false,
        url: "/SalesTaxSummary/YearList",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
        },
        contentType: "application/json; charset=utf-8",
        dataType: "json", 
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $(".sts_year_combo_box").empty();
                $(".sts_year_combo_box").prepend($("<option />").val("").text("Select Year"));
                $.each(data.yearText, function (index, item) {
                    $(".sts_year_combo_box").append($("<option />").val(item).text(item));
                });
            }
            else {
                ShowInformationDialog('Error', data.Message);
            }

        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });
    
    //Get Year
    $('.sts_year_combo_box').change(function () {

        var selectedYear = $(".sts_year_combo_box").val();
        $.ajax({
            type: "POST",
            async: false,
            url: "/SalesTaxSummary/BillDateList",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    BillYear: selectedYear
                }
            ),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {

                    $("#sts_table").DataTable().clear();
                    $(".sts_BillDate_combo_box").empty();
                    $(".sts_BillDate_combo_box").prepend($("<option />").val("").text("Select Bill Date"));
                    $.each(data.DateList, function (index, item) {
                        $(".sts_BillDate_combo_box").append($("<option />").val(item).text(item));
                    });
                    $(".sts_BillDate_combo_box").val("");
                }
                else {
                    ShowInformationDialog('Error', data.Message);
                }

            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });
    });


    //Get SummaryList
    $('.sts_view_by_type_btn').on('click', function () {
        $('.sts_view_by_type_btn').prop('disabled', true);

        let error = false;
        var billDate = $('.sts_BillDate_combo_box').val();
        if (billDate === null || billDate === "") {
            ShowInformationDialog('Error', "Bill NO. Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/SalesTaxSummary/SummaryView",
                beforeSend: function (xhr) {
                    $('.sts_View_ajax_loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        BillDate:billDate,
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#sts_table").DataTable().clear();
                        PopulateSummaryTable(data.SummaryView);
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.sts_view_by_type_btn').prop('disabled', false);
                    $('.sts_View_ajax_loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.sts_view_by_type_btn').prop('disabled', false);

        }
    });

    //OPen search Dialog
    $('.sts_Search_btn').on('click', function () {
        $("#sts_search_form").dialog({
            title: "Search",
            width: 600,
            height: 220,
            open: function () {
                // On open, hide the original submit button
                $(this).find("[type=submit]").hide();
            },
            complete: function () {
                $('.sts_search_fromtodate_btn').prop('disabled', false);
            },
        });
    });


    // Search to and from date filter  filter
    $('.sts_search_fromtodate_btn').on('click', function () {

        $('.sts_search_fromtodate_btn').prop('disabled', true);

        var error3 = false;
      
        var from_date = $('.sts_search_fromdate_dte').val();
        if (from_date === null || from_date === "") {
            ShowInformationDialog('Error', "From Date Missing");
            error3 = true;
        }
        var to_date = $('.sts_search_todate_dte').val();
        if (to_date === null || to_date === "") {
            ShowInformationDialog('Error', "To Date Missing");
            error3 = true;
        }
        if (error3 === false) {
            $.ajax({
                type: "POST",
                url: "/SalesTaxSummary/SummaryViewByDateDuration",
                beforeSend: function (xhr) {
                    $('.sts_search_form_ajax_loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        FromDate: from_date,
                        ToDate: to_date
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#sts_table").DataTable().clear();
                        PopulateSummaryTable(data.SummaryView);
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.sts_search_fromtodate_btn').prop('disabled', false);
                    $('.sts_search_form_ajax_loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.sts_search_fromtodate_btn').prop('disabled', false);

        }
    });

    //Date Token NO
    $('.sts_search_date_btn').on('click', function () {

        $('.sts_search_date_btn').prop('disabled', true);

        var error4 = false;

        var date = $('.sts_search_date_txt').val();
        if (date === null || date === "") {
            ShowInformationDialog('Error', "Token No. is Missing");
            error4 = true;
        }
        if (error4 === false) {
            $.ajax({
                type: "POST",
                url: "/SalesTaxSummary/SummaryViewByDate",
                beforeSend: function (xhr) {
                    $('.sts_search_form_ajax_loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Date: date,
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#sts_table").DataTable().clear();
                        PopulateSummaryTable(data.SummaryView);
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.sts_search_date_btn').prop('disabled', false);
                    $('.sts_search_form_ajax_loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.sts_search_date_btn').prop('disabled', false);

        }
    });

    //Search invoice No
    $('.sts_search_InvoiceNo_btn').on('click', function () {

        $('.sts_search_InvoiceNo_btn').prop('disabled', true);
        var error5 = false;

        var invoice = $('.sts_search_InvoiceNo_txt').val();
        if (invoice === null || invoice === "") {
            ShowInformationDialog('Error', "Invoice No. is Missing");
            error5 = true;
        }
        if (error5 === false) {
            $.ajax({
                type: "POST",
                url: "/SalesTaxSummary/SummaryViewByInvoice",
                beforeSend: function (xhr) {
                    $('.sts_search_form_ajax_loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        InvoiceNo: invoice,
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#sts_table").DataTable().clear();
                        PopulateSummaryTable(data.SummaryView);
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.sts_search_InvoiceNo_btn').prop('disabled', false);
                    $('.sts_search_form_ajax_loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.sts_search_InvoiceNo_btn').prop('disabled', false);

        }
    });


})

function PopulateSummaryTable(SummaryList) {
    var table = $('#sts_table').DataTable();
    $.each(SummaryList, function (index, item) {
        var jRow = $('<tr>\
        <td style= "text-align: left;padding:5px;" > ' + ((item.Sno === null) ? "" : item.Sno) + '</td>\
        <td style= "text-align: left;padding:5px;" > ' + ((item.BillDate === null) ? "" : item.BillDate) + '</td>\
        <td style= "text-align: left;padding:5px;" > ' + ((item.BillNo === null) ? "" : item.BillNo) + '</td>\
        <td style= "text-align: left;padding:5px;" > ' + ((item.InvoiceNo === null) ? "" : item.InvoiceNo) + '</td>\
        <td style= "text-align: left;padding:5px;" > ' + ((item.FromToPrvince === null) ? "" : item.FromToPrvince) + '</td>\
        <td style= "text-align: left;padding:5px;" > ' + ((item.InvoiceProvince === null) ? "" : item.InvoiceProvince) + '</td>\
        <td style= "text-align: left;padding:5px;" > ' + ((item.Amount === null) ? "" : item.Amount) + '</td>\
        <td style= "text-align: left;padding:5px;" > ' + ((item.SalesTaxAmount === null) ? "" : item.SalesTaxAmount) + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:PrintSalesTax(' + item.BillId + ',\'' + item.Type +'\');">Print</a></td>\
        </tr>');
        table.row.add(jRow);
    });
    table.draw();
};

function PrintSalesTax(billid, Type) {

    $.ajax({
        url: "/NewSalesTaxInvoice/ReportPreview",
        type: "POST",
        beforeSend: function (xhr) {
            $('.sts_View_ajax_loader').css("visibility", "visible");
        },
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify(
            {
                PartyBillId: billid,
                Type: Type,
            }),
        success: function (data) {
            var window1 = window.open('', '_blank');
            window1.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(data) + "'></iframe>");
        },
        complete: function () {
            $('.sts_View_ajax_loader').css("visibility", "hidden");
        }
    });
}