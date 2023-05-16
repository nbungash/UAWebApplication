
$(document).ready(function () {

    $('#j_table1').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        select: true,
        "scrollY": "320px",
        "scrollCollapse": true


    });

    $('#j_filter_btn').on('click', function () {
        var error = false;
        var date_filter = $(".j_date_filter_select").val();
        if (date_filter === '' || date_filter === null) {
            ShowInformationDialog('Error', "Date Filter Missing");
            error = true;
        }
        var fromdate = $("#j_from_dp").val();
        if (fromdate === '' || fromdate === null) {
            ShowInformationDialog('Error', "From Date Missing");
            error = true;
        }
        var todate = $("#j_to_dp").val();
        if (todate === '' || todate === null) {
            ShowInformationDialog('Error', "To Date Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/GeneralJournal/FilterByDateRange",
                beforeSend: function (xhr) {
                    $('.j_filter_ajax-loader').css("visibility", "visible");

                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        DateFilter: date_filter,
                        FromDate: fromdate,
                        ToDate: todate
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#j_table1").DataTable().clear().draw();
                        PopulateJournalTable(data.JournalList);
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.j_filter_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('#j_find_btn').on('click', function () {
        $("#find_journal_form").dialog({
            title: "Find Records",
            width: 700,
            height: 280,
            open: function () {
                // On open, hide the original submit button
                $(this).find("[type=submit]").hide();
                $.ajax({
                    type: "GET",
                    url: "/ChartOfAccount/AccountGroupList",
                    contentType: "application/json",
                    dataType: "json",
                    success: function (response) {
                        var data = JSON.parse(response);
                        if (data.Message == "OK") {
                            $(".fj_group_select").empty();
                            $(".fj_group_select").append($("<option />").text("Select..."));
                            $.each(data.GroupList, function (index, item) {
                                $(".fj_group_select").append($("<option />").text(item));
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
                $(".fj_group_select").val("");
            }
        });

    });

    $('.fj_find_by_account_btn').on('click', function ()
    {

        var error = false;

        var account = $(".fj_account_select").val();
        if (account === '' || account === null || account === 0) {
            ShowInformationDialog('Error', "Account Missing");
            error = true;
        }
        var fromdate = $(".fj_from_dp").val();
        if (fromdate === '' || fromdate === null) {
            ShowInformationDialog('Error', "From Date Missing");
            error = true;
        }
        var todate = $(".fj_to_dp").val();
        if (todate === '' || todate === null) {
            ShowInformationDialog('Error', "To Date Missing");
            error = true;
        }

        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/GeneralJournal/FindByAccount",
                beforeSend: function (xhr) {
                    $('.fj_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        AccountId: account,
                        FromDate: fromdate,
                        ToDate: todate,
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#j_table1").DataTable().clear().draw();
                        if (data.JournalList.length == 0) {
                            ShowInformationDialog("Information", "No Record to Display.");
                        }
                        else {
                            PopulateJournalTable(data.JournalList);
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.fj_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });

    $('.fj_transaction_id_btn').on('click', function () {

        var error = false;
        var transaction_id = $('.fj_transaction_id_txt').val();
        if (transaction_id === '' || transaction_id === null) {
            ShowInformationDialog('Error', "Transaction Id Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/GeneralJournal/FindByTransactionId",
                beforeSend: function (xhr) {
                    $('.fj_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ TransactionId: transaction_id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#j_table1").DataTable().clear().draw();
                        if (data.JournalList.length == 0) {
                            ShowInformationDialog("Information", "No Record to Display.");
                        }
                        else {
                            PopulateJournalTable(data.JournalList);
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.fj_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });

    $('.fj_voucher_no_btn').on('click', function () {
        var error = false;
        var voucher_no = $('.fj_voucher_no_txt').val();
        if (voucher_no === '' || voucher_no === null) {
            ShowInformationDialog('Error', "Voucher No Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/GeneralJournal/FindByVoucherNo",
                beforeSend: function (xhr) {
                    $('.fj_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ VoucherNo: voucher_no }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#j_table1").DataTable().clear().draw();
                        if (data.JournalList.length == 0) {
                            ShowInformationDialog("Information", "No Record to Display.");
                        }
                        else {
                            PopulateJournalTable(data.JournalList);
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.fj_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('.fj_cheque_no_btn').on('click', function () {

        var error = false;
        var cheque_no = $('.fj_cheque_no_txt').val();
        if (cheque_no === '' || cheque_no === null) {
            ShowInformationDialog('Error', "ChequeNo Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/GeneralJournal/FindByChequeNo",
                beforeSend: function (xhr)
                {
                    $('.fj_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ ChequeNo: cheque_no }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#j_table1").DataTable().clear().draw();
                        if (data.JournalList.length == 0) {
                            ShowInformationDialog("Information", "No Record to Display.");
                        }
                        else {
                            PopulateJournalTable(data.JournalList);
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.fj_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('.fj_trip_id_btn').on('click', function () {

        var error = false;
        var trip_id = $('.fj_trip_id_txt').val();
        if (trip_id === '' || trip_id === null) {
            ShowInformationDialog('Error', "Trip Id Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/GeneralJournal/FindByTripId",
                beforeSend: function (xhr) {
                    $('.fj_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ TripId: trip_id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#j_table1").DataTable().clear().draw();
                        if (data.JournalList.length == 0) {
                            ShowInformationDialog("Information","No Record to Display.");
                        }
                        else {
                            PopulateJournalTable(data.JournalList);
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.fj_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });

    $('#find_journal_form').bind('dialogclose', function (event, ui) {

        $('.fj_group_select').val("");
        $('.fj_account_select').val("");
        $(".fj_from_dp").val("");
        $(".fj_to_dp").val("");

        $(".fj_transaction_id_txt").val("");
        $(".fj_voucher_no_txt").val("");
        $(".fj_cheque_no_txt").val("");
        $(".fj_trip_id_txt").val("");
    });

    $('.fj_group_select').change(function () {
        var selected_item_text = $(".fj_group_select option:selected").text();
        if (selected_item_text !== "Select...") {
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/AccountsByGroupList",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        GroupType: selected_item_text,
                        IsActive:true
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".fj_account_select").empty();
                        $.each(data.AccountList, function (index, item) {
                            $(".fj_account_select").append($("<option />").val(item.AccountId).text(item.Title));
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

});

function PopulateJournalTable(journal_list)
{
    var table = $("#j_table1").DataTable();
    $.each(journal_list, function (index, item) {
        var jRow = $('<tr>\
            <td style="text-align: center;padding:5px;">' + item.TransId + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + GetFormatedDate(item.EntryDate) + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.AccountTitle + '</td>\
            <td style="text-align: left;padding:5px;">' + ((item.Description === null) ? "" : item.Description) + '</td>\
            <td style="text-align: right;padding:5px;" >' + ((item.Debit === null) ? "" : item.Debit) + '</td>\
            <td style="text-align: right;padding:5px;">' + ((item.Credit === null) ? "" : item.Credit) + '</td>\
            <td style="text-align: center;padding:5px;">' + ((item.ChequeNo === null) ? "" : item.ChequeNo) + '</td>\
            <td class="edit_column" style="text-align: center;padding:5px;">\
                <a href="javascript:UpdateTransaction(' + item.TransId + ',\'' + item.TripId + '\');">Update</a> | \
                <a href="javascript:DeleteTransaction('+ item.TransId + ','+'General Journal'+');">Delete</a></td>\
            </tr>');
        table.row.add(jRow);
    });
    table.draw();
}

function GetAccountGroups() {
    $.ajax({
        type: "GET",
        url: "/Trip/AccountGroupList",
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            $(".fj_group_select").empty();
            var data = JSON.parse(response);
            $.each(data, function (index, item) {
                $(".fj_group_select").append($("<option />").text(item));
            });

        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });
}

