
$(document).ready(function () {

    $('.transaction_debit_account_group_select').change(function () {
        var selected_item_text = $(".transaction_debit_account_group_select option:selected").text();
        if (selected_item_text !== "Select...") {
            GetAccountsByGroup(selected_item_text, true, ".transaction_debit_account_select");
        }
    });
    $('.transaction_credit_account_group_select').change(function () {
        var selected_item_text = $(".transaction_credit_account_group_select option:selected").text();
        if (selected_item_text !== "Select...") {
            GetAccountsByGroup(selected_item_text, true, ".transaction_credit_account_select");
        }
    });

    $('.transaction_save_btn').on('click', function () {

        $('.transaction_save_btn').prop('disabled', true);

        var error = false;
        var entrydate = $(".transaction_entry_date_dp").val();
        if (entrydate === '' || entrydate === null) {
            ShowInformationDialog('Error', "Entry Date Missing");
            error = true;
        }
        var debit_account = $(".transaction_debit_account_select").val();
        if (debit_account === '' || debit_account === null || debit_account ==0) {
            ShowInformationDialog('Error', "Debit Account Missing");
            error = true;
        }
        var credit_account = $(".transaction_credit_account_select").val();
        if (credit_account === '' || credit_account === null || credit_account == 0) {
            ShowInformationDialog('Error', "Credit Account Missing");            
            error = true;
        }
        var payee = $(".transaction_payee_txt").val();
        var description = $(".transaction_description_txt").val();
        if (description === '' || description === null) {
            ShowInformationDialog('Error', "description Missing");
            error = true;
        }
        var amount = $(".transaction_amount_txt").val();
        if (amount === '' || amount === null) {
            ShowInformationDialog('Error', "Amount Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/NewTransaction/Save",
                beforeSend: function (xhr) {
                    $('.transaction_save_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        TransId: transaction_id_for_new_transaction,
                        DebitAccountId: debit_account,
                        CreditAccountId: credit_account,
                        EntryDate: entrydate,
                        ReceiverName: payee,
                        Description: description,
                        VoucherNo: parseInt($(".transaction_voucher_no_txt").val()),
                        ChequeNo: $(".transaction_cheque_no_txt").val(),
                        Amount: parseFloat(amount)
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        if (transaction_id_for_new_transaction == 0) {
                            ShowInformationDialog("Information", "Saved Successfully.");
                            RefreshNewTransactionPage();
                        }
                        else {
                            ShowInformationDialog("Information", "Updated Successfully.");
                            $('#new_transaction_form').dialog('close');
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.transaction_save_ajax-loader').css("visibility", "hidden");
                    $('.transaction_save_btn').prop('disabled', false);
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.transaction_save_btn').prop('disabled', false);
        }
    });

    $('#new_transaction_form').bind('dialogclose', function (event, ui) {
        RefreshNewTransactionPage();
        
    });
});

var transaction_id_for_new_transaction = 0;

function NewTransactionWindowLoaded(transaction_id)
{
    transaction_id_for_new_transaction = transaction_id;
    $("#new_transaction_form").dialog({
        title: "New Transaction",
        width: 650,
        height: 350,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            AccountGroupList('.transaction_debit_account_group_select');
            AccountGroupList('.transaction_credit_account_group_select');
            if (transaction_id != 0) {
                $.ajax({
                    type: "POST",
                    url: "/NewTransaction/NewTransactionWindowLoaded",
                    beforeSend: function (xhr) {
                        $('.transaction_save_ajax-loader').css("visibility", "visible");
                    },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify(
                        {
                            TransactionId: transaction_id
                        }),
                    success: function (response) {
                        var data = JSON.parse(response);
                        if (data.Message == "OK") {
                            jQuery.ajaxSetup({ async: false });
                            $('.transaction_debit_account_group_select').val(data.TransactionObj.DebitAccountGroup).trigger("change");
                            $('.transaction_credit_account_group_select').val(data.TransactionObj.CreditAccountGroup).trigger("change");
                            jQuery.ajaxSetup({ async: true });
                            $('.transaction_debit_account_select').val(data.TransactionObj.DebitAccountId);
                            $('.transaction_credit_account_select').val(data.TransactionObj.CreditAccountId);
                            $('.transaction_entry_date_dp').val(GetFormatedDate2(data.TransactionObj.EntryDate));
                            $('.transaction_payee_txt').val(data.TransactionObj.ReceiverName);
                            $('.transaction_description_txt').val(data.TransactionObj.Description);
                            $('.transaction_voucher_no_txt').val(data.TransactionObj.VoucherNo);
                            $('.transaction_cheque_no_txt').val(data.TransactionObj.ChequeNo);
                            $('.transaction_amount_txt').val(data.TransactionObj.Amount);
                        }
                        else {
                            ShowInformationDialog("Error", data.Message);
                        }
                    },
                    complete: function () {
                        $('.transaction_save_ajax-loader').css("visibility", "hidden");
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}

function RefreshNewTransactionPage() {

    $(".transaction_debit_account_group_select").val(0);
    $(".transaction_debit_account_select").val(0);
    $(".transaction_credit_account_group_select").val(0);
    $(".transaction_credit_account_select").val(0);
    $(".transaction_debit_account_select").empty();
    $(".transaction_credit_account_select").empty();

    $(".transaction_entry_date_dp").val("");
    $(".transaction_payee_txt").val("");
    $(".transaction_description_txt").val("");
    $(".transaction_voucher_no_txt").val("");
    $(".transaction_cheque_no_txt").val("");
    $(".transaction_amount_txt").val("");

    transaction_id_for_new_transaction = 0;
}