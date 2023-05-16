$(document).ready(function () {

    $('#provinces_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        selecte: true

    });

    $.ajax({
        type: "GET",
        url: "/Provinces/ProvincesList",
        beforeSend: function (xhr) {
            $('.provinces_ajax-loader').css("visibility", "visible");
        },
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $("#provinces_table").DataTable().clear().draw();
                PopulateProvincesTable(data.ProvincesList);
            }
            else {
                ShowInformationDialog('Information', data.Message);
            }
        },
        complete: function () {
            $('.provinces_ajax-loader').css("visibility", "hidden");
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });

    $('.provinces_new_btn').on('click', function () {
        ProvincesWindowLoaded(0);
    });

    $('.np_save_btn').on('click', function () {

        $('.np_save_btn').prop('disabled', true);

        var error = false;
        var name = $(".np_name_txt").val();
        if (name === '' || name === null) {
            ShowInformationDialog('Error', "Name Missing");
            error = true;
        }
        var inter_province = $(".np_inter_province_txt").val();
        if (inter_province === '' || inter_province === null) {
            ShowInformationDialog('Error', "Inter Province Sales Tax Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/Provinces/Save",
                beforeSend: function (xhr) {
                    $('.np_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Id: province_id_for_new_province,
                        Name: name,
                        InterProvinceSalesTax: inter_province
                    }
                ),
                success: function (response) {

                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        if (province_id_for_new_province == 0) {
                            ShowInformationDialog('Information', "Saved Successfully");
                            PopulateProvincesTable(data.ProvincesList);
                            $('.np_name_txt').val("");
                            $('.np_inter_province_txt').val("");
                        }
                        else {
                            ShowInformationDialog('Information', "Updated Successfully");
                            $("#provinces_table > tbody > tr").each(function (i, v) {
                                var IdInTable = 0;
                                $(this).children('td').each(function (ii, vv) {
                                    if (ii === 0) {
                                        IdInTable = $(this).text();
                                    }
                                });
                                if (IdInTable == province_id_for_new_province) {

                                    $(this).children('td').each(function (ii, vv) {
                                        if (ii === 1) { $(this).text(name); }
                                        else if (ii === 2) { $(this).text(inter_province); }
                                    });
                                    return false;
                                }
                            });
                            $('.np_form').dialog('close');
                        }

                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.np_save_btn').prop('disabled', false);
                    $('.np_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.np_save_btn').prop('disabled', false);

        }
    });

    $('#np_form').bind('dialogclose', function (event, ui) {
        province_id_for_new_province = 0;
        $(".np_name_txt").val("");
        $(".np_inter_province_txt").val("");
    });
});

function PopulateProvincesTable(list) {
    var table = $('#provinces_table').DataTable();

    $.each(list, function (index, item) {
        var jRow = $('<tr><td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.Name + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.InterProvinceSalesTax+ '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:ProvincesWindowLoaded('+ item.Id + ');"><i class="fa fa-pencil" title="Update"></i></a>  |  \
                <a href="javascript:DeleteProvince('+ item.Id + ');"><i class="fa fa-trash" style="color: red;" title="Delete"></i></a></td>\
            <td></td></tr>');
        table.row.add(jRow);
    });
    table.draw();
}

function DeleteProvince(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/Provinces/Delete",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ Id: id }),
                success: function (response) {

                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        var table = $('#provinces_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return table.row(value).data()[0] == id;
                        });
                        table.rows(filteredData).remove().draw();
                        ShowInformationDialog('Information', "Deleted Successfully");
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

var province_id_for_new_province = 0;

function ProvincesWindowLoaded(id) {

    province_id_for_new_province = id;

    $("#np_form").dialog({
        title: "New Province",
        width: 600,
        height: 190,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            if (id != 0) {
                $.ajax({
                    type: "POST",
                    url: "/Provinces/WindowLoaded",
                    beforeSend: function (xhr) {
                        $('.np_ajax-loader').css("visibility", "visible");
                        xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                    },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify(
                        {
                            Id: id
                        }
                    ),
                    success: function (response) {

                        var data = JSON.parse(response);
                        if (data.Message === "OK") {
                            $('.np_name_txt').val(data.ObjToUpdate.Name);
                            $('.np_inter_province_txt').val(data.ObjToUpdate.InterProvinceSalesTax);

                        }
                        else {
                            ShowInformationDialog("Erro", data.Message);
                        }
                    },
                    complete: function () {
                        $('.np_ajax-loader').css("visibility", "hidden");
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}


