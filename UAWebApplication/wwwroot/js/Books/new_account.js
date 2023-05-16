
$(document).ready(function () {

    $('.coa_new_account_btn').on('click', function () {
        AccountSave(0);
    });

    $('.na_save_btn').on('click', function () {

        $('.na_save_btn').prop('disabled', true);

        var error = false;
        var error = false;
        var account_title = $('.na_title_txt').val();
        if (account_title === '' || account_title === null) {
            ShowInformationDialog('Error', "Account Title Missing");
            error = true;
        }
        var account_group = $('.na_group_select').val();
        if (account_group === '' || account_group === null) {
            ShowInformationDialog('Error', "Account Group Missing");
            error = true;
        }
        var account_type = $('.na_type_select').val();
        if (account_type === '' || account_type === null) {
            ShowInformationDialog('Error', "Account Type Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/Save",
                beforeSend: function (xhr) {
                    $('.na_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        AccountId: account_id_for_new_account,
                        Title: account_title,
                        GroupType: account_group,
                        AccountType: account_type,
                        Record: $('.na_active_check').is(":checked")
                    }
                ),
                success: function (response) {

                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        if (account_id_for_new_account == 0) {
                            ShowInformationDialog('Information', "Saved Successfully");
                            PopulateAccountTable(data.AccountList);
                            $(".na_group_select").val("");
                            $(".na_title_txt").val("");
                            $(".na_type_select").val("");
                            $(".na_active_check").prop('checked', false);
                        }
                        else {
                            ShowInformationDialog('Information', "Updated Successfully");
                            $("#coa_table > tbody > tr").each(function (i, v) {
                                var IdInTable = 0;
                                $(this).children('td').each(function (ii, vv) {
                                    if (ii === 1) {
                                        IdInTable = $(this).text();
                                    }
                                });
                                if (IdInTable == account_id_for_new_account) {

                                    $(this).children('td').each(function (ii, vv) {
                                        if (ii === 0) { $(this).text(account_group); }
                                        else if (ii === 2) { $(this).text(account_title); }
                                        else if (ii === 3) { $(this).text(account_type); }
                                    });
                                    return false;
                                }
                            });
                            $('.new_account_form').dialog('close');
                        }

                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.na_save_btn').prop('disabled', false);
                    $('.na_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.na_save_btn').prop('disabled', false);

        }
    });

    $('#new_account_form').bind('dialogclose', function (event, ui) {
        account_id_for_new_account = 0;
        $(".na_group_select").val("");
        $(".na_title_txt").val("");
        $(".na_type_select").val("");
        $(".na_active_check").prop('checked', false);
    });
});

var account_id_for_new_account = 0;
function AccountSave(account_id)
{
    account_id_for_new_account = account_id;
    $("#new_account_form").dialog({
        title: "New Account",
        width: 600,
        height: 250,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $(".na_group_select").val("");
            $(".na_type_select").val("");
            
            if (account_id !== 0)
            {
                 $.ajax({
                    type: "POST",
                     url: "/ChartOfAccount/WindowLoaded",
                    beforeSend: function (xhr) {},
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                     data: JSON.stringify({ AccountId: account_id }),
                    success: function (response) {
                        var data = JSON.parse(response);
                        if (data.Message == "OK") {
                            $(".na_group_select").val(data.ObjToUpdate.GroupType);
                            $(".na_title_txt").val(data.ObjToUpdate.Title);
                            $(".na_type_select").val(data.ObjToUpdate.AccountType);
                            $(".na_active_check").prop('checked', data.ObjToUpdate.Record);
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
            else
            {
                $(".na_active_check").prop('checked', true);
            }
        }
    });
}
