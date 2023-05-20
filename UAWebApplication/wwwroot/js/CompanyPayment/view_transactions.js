
$(document).ready(function () {

    $('#view_trans_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": "250px",
        "scrollCollapse": true,
        "bSort": false
    });

    $('.view_trans_save_btn').on('click', function () {

        var transactionList = [];
        $("#view_trans_table > tbody > tr").each(function (i, v) {
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
                if (ii === 7) {
                    xyz["CompanyPaymentType"] = parseInt($(this).find('select').val());
                }
            });
            if (check1 === true) {
                transactionList.push(xyz);
            }
        });
        $.ajax({
            type: "POST",
            url: "/CompanyPayment/ViewTransSave",
            beforeSend: function (xhr) {
                $('.view_trans_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    PaymentId: payment_id_for_view_transactions,
                    TransactionList: transactionList
                }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    ShowInformationDialog('Information', "Saved Successfully.");
                }
                else {
                    ShowInformationDialog('Error', data.Message);
                }
            },
            complete: function () {
                $('.view_trans_ajax-loader').css("visibility", "hidden");
            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });

    });

    $('#view_transactions_form').bind('dialogclose', function (event, ui) {

        $(".view_trans_table").DataTable().clear().draw();
        payment_id_for_view_transactions = 0;

    });
    
});

function PopulateViewTransactionTable(TransactionList) {
    var table = $(".view_trans_table").DataTable();
    $.each(TransactionList, function (index, item) {
        var row = $('<tr><td style="text-align: center;padding:5px;"><input type="checkbox" ' + item.IsChecked + '/></td>\
            <td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style="text-align: center;padding:5px;">' + item.TransId + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + GetFormatedDate(item.EntryDate) + '</td>\
            <td style="text-align: left;padding:5px;" >' + item.Description + '</td>\
            <td style="text-align: right;padding:5px;">' + ((item.Debit === null) ? "" : item.Debit) + '</td>\
            <td style="text-align: right;padding:5px;">' + ((item.Credit === null) ? "" : item.Credit) + '</td>\
            <td style="text-align: center;padding:5px;">\
                <select class="view_trans_type_select" value="'+ item.CompanyPaymentType +'">\
                <option>Select...</option><option>W/H Tax</option><option>Sales Tax</option><option>Penalty</option>\
                <option>Deduction</option><option>Recovery</option></select></td>\
            </tr>');
        table.row.add(row);
    });
    table.draw();
}

var payment_id_for_view_transactions = 0;

function ViewTransactionsWindowLoaded(payment_id)
{
    payment_id_for_view_transactions = payment_id;
    $("#view_transactions_form").dialog({
        title: "VIEW TRANSACTIONS",
        width: 1200,
        height: 500,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $.ajax({
                type: "POST",
                url: "/CompanyPayment/ViewTransView",
                beforeSend: function (xhr) {
                    $('.view_trans_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        PaymentId: payment_id
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {

                        $(".view_trans_table").DataTable().clear().draw();
                        if (data.TransactionList.length == 0) {
                            ShowInformationDialog("Information","No Transaction to Display");
                        } else {
                            PopulateViewTransactionTable(data.TransactionList);
                        }
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.view_trans_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

}

