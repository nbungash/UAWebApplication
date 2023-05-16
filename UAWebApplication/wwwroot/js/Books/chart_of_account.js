
$(document).ready(function () {

    $('#coa_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All",10, 25, 50]],
        selecte: true,
        "scrollY": "300px",
        "scrollCollapse": true,
        "bSort": true,

    });
    $('#view_contact_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All", 10, 25, 50]],
        select: true,
        'sDom': 't',
        "scrollCollapse": true,
        bSort: false
    });

    GetAccountGroups();

    $('.coa_view_btn').on('click', function () {

        var error = false;
        var selected_item_text = $(".coa_group_select").val();
        if (selected_item_text === '' || selected_item_text === null) {
            ShowInformationDialog('Error', "Group Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/AccountsByGroupList",
                beforeSend: function (xhr) { $('.coa_ajax-loader').css("visibility", "visible"); },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        GroupType: selected_item_text,
                        IsActive:$('.coa_active_check').is(":checked")
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".coa_table").DataTable().clear().draw();
                        PopulateAccountTable(data.AccountList);
                    }
                    else
                    {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.coa_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $(".coa_title_txt").bind('input', function () {

        $(".coa_table").DataTable().clear().draw();
        var title = $(this).val();
        if (title !== '' || title !== null) {
            if (title.length > 3) {
                $.ajax({
                    type: "POST",
                    url: "/ChartOfAccount/SearchAccountsByTitle",
                    beforeSend: function (xhr) { },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ Title: title }),
                    success: function (response) {
                        var data = JSON.parse(response);
                        if (data.Message == "OK") {
                            $(".coa_table").DataTable().clear().draw();
                            PopulateAccountTable(data.AccountList);
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
        }
    });

    $('.lif_save_btn').on('click', function () {

        $('.lif_save_btn').prop('disabled', true);
                
        $.ajax({
            type: "POST",
            url: "/ChartOfAccount/SaveLorryInfo",
            beforeSend: function (xhr) { $('.lif_ajax-loader').css("visibility", "visible");},
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    AccountId: account_id_for_account_info,
                    Capacity: $(".lif_capacity_txt").val(),
                    OwnerName: $(".lif_owner_name_txt").val(),
                    Make: $(".lif_make_txt").val(),
                    Model: $(".lif_model_txt").val(),
                    ChassisNo: $(".lif_chasis_txt").val(),
                    EngineNo: $(".lif_engine_txt").val(),
                    DipChartDueDate: $(".lif_dip_chart_date_dp").val(),
                    TrackerDueDate: $(".lif_tracker_date_dp").val(),
                    TokenDueDate: $(".lif_token_date_dp").val()
                }
            ),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    ShowInformationDialog('Information', "Saved Successfully");
                    account_id_for_account_info = 0;
                    $("#lorry_information_form").dialog("close");
                }
                else {
                    ShowInformationDialog('Error', data.Message);

                }
            },
            complete: function () {
                $('.lif_save_btn').prop('disabled', false);
                $('.lif_ajax-loader').css("visibility", "hidden");
            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });
    });
    
    //contacts
    $('.contact_save_btn').on('click', function () {

        $('.contact_save_btn').prop('disabled', true);

        var error = false;
        var name = $(".contact_name_txt").val();
        if (name === '' || name === null) {
            ShowInformationDialog('Error', "Name Missing");
            $('.contact_save_btn').prop('disabled', false);
            error = true;
        }
        var contact_no = $(".contact_contact_no_txt").val();
        if (contact_no === '' || contact_no === null) {
            ShowInformationDialog('Error', "Contact No Missing");
            $('.contact_save_btn').prop('disabled', false);
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                beforeSend: function (xhr) {
                    $('.contact_save_ajax-loader').css("visibility", "visible");
                },
                url: "/ChartOfAccount/AddContact",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        AccountContactId: contact_id_for_update_contact,
                        AccountId: account_id_for_add_contact,
                        Name: name,
                        ContactNo: contact_no
                    }
                ),
                success: function (response) {
                    var data = JSON.parse(response);

                    if (data.Message === "OK") {

                        ShowInformationDialog('Information', "Saved Successfully");
                        $("#new_contact_form").dialog("close");

                    }
                    else
                    {
                        ShowInformationDialog('Error', data.Message);
                    }
                    
                },
                complete: function () {
                    $('.contact_save_btn').prop('disabled', false);
                    $('.contact_save_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });
    $('#view_contact_form').bind('dialogclose', function (event, ui) {

        account_id_for_view_contact = 0;
        $(".view_contact_table").DataTable().clear().draw();
    });
    $('#new_contact_form').bind('dialogclose', function (event, ui) {

        contact_id_for_update_contact = 0;
        account_id_for_add_contact = 0;
        $(".contact_name_txt").val("");
        $(".contact_contact_no_txt").val("");

    });

});

function PopulateAccountTable(accountList)
{
    var table = $(".coa_table").DataTable();
    $.each(accountList, function (index, item) {
        var jRow = $('<tr><td style="text-align: center;padding:5px;">' + item.GroupType + '</td>\
            <td style="text-align: center;padding:5px;">' + item.AccountId + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.Title + '</td>\
            <td style="text-align: center;padding:5px;">' + item.AccountType + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:OpenAccountInfo(' + item.AccountId + ',\'' + item.GroupType + '\');">Detail</a> | \
                <a href="javascript:AccountSave('+ item.AccountId + ');">Update</a>  |  \
                <a href="javascript:DeleteAccount('+ item.AccountId + ');">Delete</a></td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:OpenNewContact(' + item.AccountId + ');">New</a>  |  \
                <a href="javascript:ViewContact('+ item.AccountId + ');">View</a></td>\
            </tr>');
        table.row.add(jRow);
    });
    table.draw();
}

function GetAccountGroups() {
    $.ajax({
        type: "GET",
        url: "/ChartOfAccount/AccountGroupList",
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $(".coa_group_select").empty();
                $(".coa_group_select").append($("<option />").text("All"));
                $.each(data.GroupList, function (index, item) {
                    $(".coa_group_select").append($("<option />").text(item));
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

function DeleteAccount(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/Delete",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN", $('input:hidden[name="__RequestVerificationToken"]').val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ Id: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        ShowInformationDialog('Information', "Deleted Successfully.");
                        var table = $('#coa_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return table.row(value).data()[1] == id;
                        });
                        table.rows(filteredData).remove().draw();
                        $('#dialog-confirm').dialog("close");
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
}

//Account Information
var account_id_for_account_info = 0;
function OpenAccountInfo(account_id, group) {
    account_id_for_account_info = account_id;

    if (group === "LORRY") {
        $("#lorry_information_form").dialog({
            title: "Lorry Info",
            width: 750,
            height: 310,
            open: function () {
                // On open, hide the original submit button
                $(this).find("[type=submit]").hide();
                $.ajax({
                    type: "POST",
                    url: "/ChartOfAccount/LorryInfoWindowLoaded",
                    beforeSend: function (xhr) { $('.lif_ajax-loader').css("visibility", "visible"); },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ AccountId: account_id }),
                    success: function (response) {
                        var data = JSON.parse(response);
                        if (data.Message == "OK") {
                            $(".lif_capacity_txt").val(data.ObjToUpdate.Capacity);
                            $(".lif_owner_name_txt").val(data.ObjToUpdate.OwnerName);

                            $(".lif_make_txt").val(data.ObjToUpdate.Make);
                            $(".lif_model_txt").val(data.ObjToUpdate.Model);

                            $(".lif_engine_txt").val(data.ObjToUpdate.EngineNo);
                            $(".lif_chasis_txt").val(data.ObjToUpdate.ChassisNo);

                            $(".lif_dip_chart_date_dp").val(GetFormatedDate2(data.ObjToUpdate.DipChartDueDate));
                            $(".lif_tracker_date_dp").val(GetFormatedDate2(data.ObjToUpdate.TrackerDueDate));
                            $(".lif_token_date_dp").val(GetFormatedDate2(data.ObjToUpdate.TokenDueDate));
                        }
                        else {
                            ShowInformationDialog("Error", data.Message);
                        }
                    },
                    complete: function () {
                        $('.lif_ajax-loader').css("visibility", "hidden");
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        });
    }
}

//Account Contact
var account_id_for_add_contact = 0;
function OpenNewContact(account_id) {

    account_id_for_add_contact = account_id;

    $("#new_contact_form").dialog({
        title: "New Contact",
        width: 500,
        height: 200,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
        }
    });
}

var account_id_for_view_contact = 0;
function ViewContact(account_id)
{
    account_id_for_view_contact = account_id;

    $("#view_contact_form").dialog({
        title: "View Contact",
        width: 700,
        height: 300,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/ViewContactWindowLoaded",
                beforeSend: function (xhr) { },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ AccountId: account_id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    
                    if (data.Message == "OK") {

                        $(".view_contact_table").DataTable().clear().draw();
                        var table = $(".view_contact_table").DataTable();
                        $.each(data.ContactList, function (index, item) {

                            var jRow = $('<tr><td style="text-align: left;padding:5px;">' + item.Name + '</td>\
                            <td style="text-align: center;padding:5px;">' + item.ContactNo + '</td>\
                            <td style="text-align: center;padding:5px;">\
                                <a href="javascript:UpdateContact('+ item.AccountContactId + ');">Update</a>  |  \
                                <a href="javascript:DeleteContact('+ item.AccountContactId + ');">Delete</a></td>\
                            <td class="d-none">' + item.AccountContactId + '</td>\</tr>');
                            table.row.add(jRow).draw();
                        });
                    }
                    else
                    {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });
}

var contact_id_for_update_contact = 0;
function UpdateContact(contact_id)
{
    contact_id_for_update_contact = contact_id;
    $("#new_contact_form").dialog({
        title: "New Contact",
        width: 500,
        height: 200,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/NewContactWindowLoaded",
                beforeSend: function (xhr) { },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ ContactId: contact_id }),
                success: function (response) {
                    var data = JSON.parse(response);

                    if (data.Message == "OK") {

                        $(".contact_name_txt").val(data.ObjToUpdate.Name);
                        $(".contact_contact_no_txt").val(data.ObjToUpdate.ContactNo);
                        account_id_for_add_contact = data.ObjToUpdate.AccountId;
                    }
                    else
                    {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });
    
}
function DeleteContact(contact_id) {

    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/ChartOfAccount/DeleteContact",
                beforeSend: function (xhr) { },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ ContactId: contact_id }),
                success: function (response) {
                    var data = JSON.parse(response);

                    if (data.Message === "OK") {
                        ShowInformationDialog('Information', "Deleted Successfully.");
                        var table = $('#view_contact_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return table.row(value).data()[3] == contact_id;
                        });
                        table.rows(filteredData).remove().draw();
                        $('#dialog-confirm').dialog("close");
                    }
                    else {
                        ShowInformationDialog('Information', data.Message);
                    }
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        
    });
}


