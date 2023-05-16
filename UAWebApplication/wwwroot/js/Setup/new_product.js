$(document).ready(function () {

    $('.product_new_btn').on('click', function () {
        ProductWindowLoaded(0);
    });

    $('.new_product_save_btn').on('click', function () {

        $('.new_product_save_btn').prop('disabled', true);

        var error = false;
        var company = $(".new_product_company_select").val();
        if (company === '' || company === null || company==0) {
            ShowInformationDialog('Error', "Company Missing");
            error = true;
        }
        var title = $(".new_product_title_txt").val();
        if (title === '' || title === null) {
            ShowInformationDialog('Error', "Title Missing");
            error = true;
        }
        var title_urdu = $(".new_product_title_urdu_txt").val();
        if (title_urdu === '' || title_urdu === null) {
            ShowInformationDialog('Error', "Intra Province Sales Tax Missing");
            error = true;
        }
        var code = $(".new_product_code_txt").val();
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/Product/Save",
                beforeSend: function (xhr) {
                    $('.new_product_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Id: product_id_for_new_product,
                        Title: title,
                        TitleUrdu: title_urdu,
                        ProductCode: code,
                        PartyId: company
                    }
                ),
                success: function (response) {

                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        if (product_id_for_new_product == 0) {
                            ShowInformationDialog('Information', "Saved Successfully");
                            PopulateProductTable(data.ProductList);
                            $('.new_product_company_select').val("");
                            $('.new_product_title_txt').val("");
                            $('.new_product_title_urdu_txt').val("");
                            $('.new_product_code_txt').val("");
                        }
                        else {
                            ShowInformationDialog('Information', "Updated Successfully");
                            $("#product_table > tbody > tr").each(function (i, v) {
                                var IdInTable = 0;
                                $(this).children('td').each(function (ii, vv) {
                                    if (ii === 0) {
                                        IdInTable = $(this).text();
                                    }
                                });
                                if (IdInTable == product_id_for_new_product) {

                                    $(this).children('td').each(function (ii, vv) {
                                        if (ii === 1) { $(this).text(title); }
                                        else if (ii === 2) { $(this).text(title_urdu); }
                                        else if (ii === 3) { $(this).text(code); }
                                        else if (ii === 4) { $(this).text($(".new_product_company_select option:selected").text()); }
                                    });
                                    return false;
                                }
                            });
                            $('.new_product_form').dialog('close');
                        }

                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.new_product_save_btn').prop('disabled', false);
                    $('.new_product_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.new_product_save_btn').prop('disabled', false);

        }
    });

    $('#new_product_form').bind('dialogclose', function (event, ui) {
        product_id_for_new_product = 0;
        $('.new_product_company_select').val("");
        $('.new_product_title_txt').val("");
        $('.new_product_title_urdu_txt').val("");
        $('.new_product_code_txt').val("");
    });

});
var product_id_for_new_product = 0;

function ProductWindowLoaded(id) {

    product_id_for_new_product = id;

    $("#new_product_form").dialog({
        title: "New Product",
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
                        $(".new_product_company_select").empty();
                        $(".new_product_company_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.AccountList, function (index, item) {
                            $(".new_product_company_select").append($("<option />").val(item.AccountId).text(item.Title));
                        });
                        $(".new_product_company_select").val(0);
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
                    url: "/Product/WindowLoaded",
                    beforeSend: function (xhr) {
                        $('.new_product_ajax-loader').css("visibility", "visible");
                        xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                    },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ Id: id }),
                    success: function (response) {

                        var data = JSON.parse(response);
                        if (data.Message === "OK") {
                            $('.new_product_company_select').val(data.ObjToUpdate.PartyId);
                            $('.new_product_title_txt').val(data.ObjToUpdate.Title);
                            $('.new_product_title_urdu_txt').val(data.ObjToUpdate.TitleUrdu);
                            $('.new_product_code_txt').val(data.ObjToUpdate.ProductCode);
                        }
                        else {
                            ShowInformationDialog("Erro", data.Message);
                        }
                    },
                    complete: function () {
                        $('.new_product_ajax-loader').css("visibility", "hidden");
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}