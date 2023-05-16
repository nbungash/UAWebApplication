
$(document).ready(function () {

    $('#ledger_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All", 10, 25, 50]],
        select: true,
        //'sDom': 't',
        "scrollY": "350px",
        "scrollCollapse": true,
        "bSort": false,
        
    });

    AccountGroupList('.ledger_group_select');

    $('.ledger_group_select').change(function () {
        var selected_item_text = $(".ledger_group_select option:selected").text();
        if (selected_item_text !== "Select...") {
            GetAccountsByGroup(selected_item_text, true, '.ledger_account_select');
        }
    });

    $('.ledger_view_btn').on('click', function () {
        var error = false;
        var group = $(".ledger_group_select").val();
        if (group === '' || group === null || group === "Select...") {
            ShowInformationDialog('Error', "Group Missing");
            error = true;
        }
        var account = $(".ledger_account_select").val();
        if (account === '' || account === null || account === 0) {
            ShowInformationDialog('Error', "Account Missing");
            error = true;
        }
        var fromdate = $(".ledger_date_from_dp").val();
        if (fromdate === '' || fromdate === null) {
            ShowInformationDialog('Error', "From Date Missing");
            error = true;
        }
        var todate = $(".ledger_date_to_dp").val();
        if (todate === '' || todate === null) {
            ShowInformationDialog('Error', "To Date Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/GeneralLedger/ViewRecords",
                beforeSend: function (xhr) {
                    $('.ledger_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Group: group,
                        AccountId:account,
                        FromDate: fromdate,
                        ToDate: todate
                    }
                ),
                success: function (response) {

                    var data = JSON.parse(response);

                    if (data.Message == "OK") {

                        $(".ledger_table").DataTable().clear().draw();
                        var table = $(".ledger_table").DataTable();
                        $.each(data.LedgerList, function (index, item) {
                            var row = $('<tr>\
                                <td style = "text-align: center;padding:5px;" > ' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate)) + '</td >\
                                <td style= "text-align: left;padding:5px;" > ' + ((item.Description === null) ? "" : item.Description) + '</td>\
                                <td style= "text-align: center;padding:5px;" > ' + ((item.Lorry === null) ? "" : item.Lorry) + '</td>\
                                <td style= "text-align: center;padding:5px;" > ' + ((item.ChequeNo === null) ? "" : item.ChequeNo) + '</td>\
                                <td style= "text-align: center;padding:5px;" > ' + ((item.VoucherNo === null) ? "" : item.VoucherNo) + '</td>\
                                <td style= "text-align: right;padding:5px;" > ' + ((item.Debit === null) ? "" : item.Debit) + '</td>\
                                <td style="text-align: right;padding:5px;">' + ((item.Credit === null) ? "" : item.Credit) + '</td>\
                                <td style="text-align: right;padding:5px;" >' + ((item.Balance === null) ? "" : item.Balance) + '</td>\
                                <td style="text-align: center;padding:5px;">' + ((item.DebitCredit === null) ? "" : item.DebitCredit) + '</td>\
                                </tr>');
                            table.row.add(row);
                        });
                        table.draw();
                    }
                    else
                    {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.ledger_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }


    });

    $('.ledger_print_btn').on('click', function ()
    {
        var error = false;
        var account = $(".ledger_account_select").val();
        if (account === '' || account === null || account === 0) {
            ShowInformationDialog('Error', "Account Missing");
            error = true;
        }
        var fromdate = $(".ledger_date_from_dp").val();
        if (fromdate === '' || fromdate === null) {
            ShowInformationDialog('Error', "From Date Missing");
            error = true;
        }
        var todate = $(".ledger_date_to_dp").val();
        if (todate === '' || todate === null) {
            ShowInformationDialog('Error', "To Date Missing");
            error = true;
        }
        if (error === false) {

            $.ajax({
                url: "/GeneralLedger/ReportPreview",
                type: "POST",
                beforeSend: function (xhr) { $('.ledger_ajax-loader').css("visibility", "visible"); },
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                data: JSON.stringify(
                    {
                        AccountTitle: $(".ledger_account_select option:selected").text(),
                        AccountId: account,
                        FromDate: fromdate,
                        ToDate: todate,
                    }),
                success: function (data) {
                    var window1 = window.open('', '_blank');
                    window1.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + data + "'></iframe>");
                },
                complete: function () {
                    $('.ledger_ajax-loader').css("visibility", "hidden");
                }
            });

        }
        
    });
  
});
