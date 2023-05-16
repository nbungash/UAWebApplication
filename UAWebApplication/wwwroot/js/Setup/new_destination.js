$(document).ready(function () {

    $('.destination_new_btn').on('click', function () {
        DestinationWindowLoaded(0);
    });

    $('.new_destination_save_btn').on('click', function () {

        $('.new_destination_save_btn').prop('disabled', true);

        var error = false;
        var company = $(".new_destination_company_select").val();
        if (company === '' || company === null || company==0) {
            ShowInformationDialog('Error', "Company Missing");
            error = true;
        }
        var title = $(".new_destination_title_txt").val();
        if (title === '' || title === null) {
            ShowInformationDialog('Error', "Title Missing");
            error = true;
        }
        var title_urdu = $(".new_destination_title_urdu_txt").val();
        if (title_urdu === '' || title_urdu === null) {
            ShowInformationDialog('Error', "Title Urdu Missing");
            error = true;
        }
        var code = $(".new_destination_code_txt").val();
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/Destination/Save",
                beforeSend: function (xhr) {
                    $('.new_destination_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Id: destination_id_for_new_destination,
                        Title: title,
                        TitleUrdu: title_urdu,
                        DestinationCode: code,
                        PartyId: company
                    }
                ),
                success: function (response) {

                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        if (destination_id_for_new_destination == 0) {
                            ShowInformationDialog('Information', "Saved Successfully");
                            PopulateDestinationTable(data.DestinationList);
                            $('.new_destination_company_select').val("");
                            $('.new_destination_title_txt').val("");
                            $('.new_destination_title_urdu_txt').val("");
                            $('.new_destination_code_txt').val("");
                        }
                        else {
                            ShowInformationDialog('Information', "Updated Successfully");
                            $("#destination_table > tbody > tr").each(function (i, v) {
                                var IdInTable = 0;
                                $(this).children('td').each(function (ii, vv) {
                                    if (ii === 0) {
                                        IdInTable = $(this).text();
                                    }
                                });
                                if (IdInTable == destination_id_for_new_destination) {

                                    $(this).children('td').each(function (ii, vv) {
                                        if (ii === 1) { $(this).text(title); }
                                        else if (ii === 2) { $(this).text(title_urdu); }
                                        else if (ii === 3) { $(this).text(code); }
                                        else if (ii === 4) { $(this).text($(".new_destination_company_select option:selected").text()); }
                                    });
                                    return false;
                                }
                            });
                            $('.new_destination_form').dialog('close');
                        }

                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.new_destination_save_btn').prop('disabled', false);
                    $('.new_destination_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.new_destination_save_btn').prop('disabled', false);

        }
    });

    $('#new_destination_form').bind('dialogclose', function (event, ui) {
        destination_id_for_new_destination = 0;
        $('.new_destination_company_select').val("");
        $('.new_destination_title_txt').val("");
        $('.new_destination_title_urdu_txt').val("");
        $('.new_destination_code_txt').val("");
    });

});
var destination_id_for_new_destination = 0;

function DestinationWindowLoaded(id) {

    destination_id_for_new_destination = id;

    $("#new_destination_form").dialog({
        title: "New Destination",
        width: 500,
        height: 260,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
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
                        GroupType: "PARTY",
                        IsActive: true
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".new_destination_company_select").empty();
                        $(".new_destination_company_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.AccountList, function (index, item) {
                            $(".new_destination_company_select").append($("<option />").val(item.AccountId).text(item.Title));
                        });
                        $(".new_destination_company_select").val(0);
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
            if (id != 0) {
                $.ajax({
                    type: "POST",
                    url: "/Destination/WindowLoaded",
                    beforeSend: function (xhr) {
                        $('.new_destination_ajax-loader').css("visibility", "visible");
                        xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                    },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ Id: id }),
                    success: function (response) {

                        var data = JSON.parse(response);
                        if (data.Message === "OK") {
                            $('.new_destination_company_select').val(data.ObjToUpdate.PartyId);
                            $('.new_destination_title_txt').val(data.ObjToUpdate.Title);
                            $('.new_destination_title_urdu_txt').val(data.ObjToUpdate.TitleUrdu);
                            $('.new_destination_code_txt').val(data.ObjToUpdate.DestinationCode);
                        }
                        else {
                            ShowInformationDialog("Erro", data.Message);
                        }
                    },
                    complete: function () {
                        $('.new_destination_ajax-loader').css("visibility", "hidden");
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}