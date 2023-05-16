
$(document).ready(function () {

    $('#attach_trans_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": "250px",
        "scrollCollapse": true,
        "bSort": false
    });

    $('.attach_trans_view_btn').on('click', function () {

        var error = false;
        var fromdate = $(".attach_trans_from_date_dp").val();
        if (fromdate === '' || fromdate === null) {
            ShowInformationDialog('Error', "From Date Missing");
            error = true;
        }
        var todate = $(".attach_trans_to_date_dp").val();
        if (todate === '' || todate === null) {
            ShowInformationDialog('Error', "To Date Missing");
            error = true;
        }
        if (error === false) {

            $.ajax({
                type: "POST",
                url: "/CompanyPayment/AttachTransView",
                beforeSend: function (xhr) {
                    $('.attach_trans_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        PaymentId: payment_id_for_attach_transactions,
                        FromDate: fromdate,
                        ToDate: todate
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#attach_trans_table").DataTable().clear().draw();
                        if (data.TransactionList.length == 0) {
                            ShowInformationDialog("Information", "Oops! No Record to Display");
                        }
                        else {
                            PopulateAttachTransactionsTable(data.TransactionList);
                        }
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.attach_trans_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('.attach_trans_save_btn').on('click', function () {

        var transactionList = [];
        $("#attach_trans_table > tbody > tr").each(function (i, v) {
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
                transactionList.push(xyz);
            }
        });
        $.ajax({
            type: "POST",
            url: "/CompanyPayment/AttachTransSave",
            beforeSend: function (xhr) {
                $('.attach_trans_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    PaymentId: payment_id_for_attach_transactions,
                    TransactionList: transactionList
                }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    $("#attach_trans_table").DataTable().clear().draw();
                    ShowInformationDialog('Information', "Saved Successfully.");
                }
                else {
                    ShowInformationDialog('Error', data.Message);
                }
            },
            complete: function () {
                $('.attach_trans_ajax-loader').css("visibility", "hidden");
            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });

    });

    $('#attach_transactions_form').bind('dialogclose', function (event, ui) {

        $(".attach_trans_from_date_dp").val("");
        $(".attach_trans_to_date_dp").val("");
        $(".attach_trans_table").DataTable().clear().draw();
        payment_id_for_attach_transactions = 0;

    });
    
});

function PopulateAttachTransactionsTable(TransactionList) {
    var table = $(".attach_trans_table").DataTable();
    $.each(TransactionList, function (index, item) {
        var row = $('<tr><td style="text-align: center;padding:5px;"><input type="checkbox" ' + item.IsChecked +'/></td>\
            <td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style="text-align: center;padding:5px;">' + item.TransId + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + GetFormatedDate(item.EntryDate) + '</td>\
            <td style="text-align: left;padding:5px;" >' + item.Description + '</td>\
            <td style="text-align: right;padding:5px;">' + ((item.Debit === null) ? "" : item.Debit) + '</td>\
            <td style="text-align: right;padding:5px;">' + ((item.Credit === null) ? "" : item.Credit) + '</td>\
            </tr>');
        table.row.add(row);
    });
    table.draw();
}

var payment_id_for_attach_transactions = 0;

function AttachTransactionsWindowLoaded(payment_id)
{
    payment_id_for_attach_transactions = payment_id;
    $("#attach_transactions_form").dialog({
        title: "ATTACH TRANSACTIONS",
        width: 1200,
        height: 500,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
        }
    });

}

