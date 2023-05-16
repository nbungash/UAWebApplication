
$(document).ready(function () {

    $('#roles_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        selecte: true

    });

    $.ajax({
        type: "GET",
        url: "/Role/ResourceList",
        contentType: "application/json",
        dataType: "json",
        success: function (response) {

            var data = JSON.parse(response);

            if (data.Message == "OK") {

                $(".roles_page_select").empty();
                $.each(data.ResourceList, function (index, item) {
                    $(".roles_page_select").append($("<option />").val(item.Id).text(item.Title));
                });
                $(".roles_page_select").val(0);
            }
            else
            {
                ShowInformationDialog('Error', data.Message);
            }
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });

    $('.roles_view_btn').on('click', function () {

        $('.roles_view_btn').prop('disabled', true);

        var error1 = false;
        var page1 = $('.roles_page_select').val();
        if (page1 === null || page1 === "" || page1 === 0) {
            ShowInformationDialog('Error', "Resource Missing");
            $('.roles_view_btn').prop('disabled', false);
            error = true;
        }
        if (error1 === false) {
            $.ajax({
                type: "POST",
                url: "/Role/RolesList",
                beforeSend: function (xhr)
                {
                    $('.roles_view_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ ResourceId: page1 }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#roles_table").DataTable().clear().draw();
                        PopulateRoleTable(data.RolesList);
                    }
                    else
                    {
                        ShowInformationDialog('Error', data.Message);
                    }

                },
                complete: function () {
                    $('.roles_view_btn').prop('disabled', false);
                    $('.roles_view_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

});

function PopulateRoleTable(roles_list)
{
    var table = $('#roles_table').DataTable();
    $.each(roles_list, function (index, item) {
        var jRow = $('<tr><td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.Name + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:OpenNewRole(\'' + item.Id + '\');">Update</a>  |  \
                <a href="javascript:DeleteRole(\''+ item.Id + '\');">Delete</a></td>\
            <td style="text-align: left;padding:5px;"></td></tr>');
        table.row.add(jRow);
    });
    table.draw();
}

function DeleteRole(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/Role/DeleteRole",
                beforeSend: function (xhr)
                {
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ RoleId: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "Role Deleted Successfully") {

                        var table = $('#roles_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return table.row(value).data()[0] == id;
                        });
                        table.rows(filteredData).remove().draw();
                    }
                    ShowInformationDialog('Information', data.Message);
                    $('#dialog-confirm').dialog("close");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
        }
    });
}


