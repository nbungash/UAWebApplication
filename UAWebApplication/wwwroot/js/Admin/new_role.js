$(document).ready(function () {

    $.ajax({
        type: "GET",
        async: false,
        url: "/Role/ResourceList",
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {

                $(".new_role_page_select").empty();

                $.each(data.ResourceList, function (index, item) {
                    $(".new_role_page_select").append($("<option />").val(item.Id).text(item.Title));
                });
            }
            else {
                ShowInformationDialog('Error', data.Message);
            }

        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });

    $('.roles_new_btn').on('click', function () {
        OpenNewRole("0");
    });

    $('.new_role_save_btn').on('click', function () {

        $('.new_role_save_btn').prop('disabled', true);

        var error = false;

        var name = $(".new_role_title_txt").val();
        if (name === '' || name === null) {
            ShowInformationDialog('Error', "Name Missing");
            $('.new_role_save_btn').prop('disabled', false);
            error = true;
        }
        var page1 = $(".new_role_page_select").val();
        if (page1 === '' || page1 === null|| page1===0) {
            ShowInformationDialog('Error', "Page Missing");
            $('.new_role_save_btn').prop('disabled', false);
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/Role/SaveRole",
                beforeSend: function (xhr)
                {
                    $('.new_role_save_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Id: role_id_for_new_role,
                        Name: name,
                        ResourceId:page1
                    }
                ),
                success: function (response) {
                    var data = JSON.parse(response);

                    ShowInformationDialog('Information', data.Message);
                    if (data.Message === "Saved Successfully") {

                        PopulateRoleTable(data.RolesList);
                        $('.new_role_form').dialog('close');
                    }
                    else if (data.Message === "Updated Successfully") {

                        var table = $('#roles_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return table.row(value).data()[0] == role_id_for_new_role;
                        });
                        table.rows(filteredData).remove().draw();

                        PopulateRoleTable(data.RolesList);
                        $('.new_role_form').dialog('close');
                    }
                    
                },
                complete: function () {
                    $('.new_role_save_btn').prop('disabled', false);
                    $('.new_role_save_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('#new_role_form').bind('dialogclose', function (event, ui) {

        role_id_for_new_role = "0";
        $('.new_role_save_btn').prop('disabled', false);
        $(".new_role_title_txt").val("");
        $(".new_role_page_select").val("");
        $('.roles_new_btn').prop('disabled', false);
    });

    
});

var role_id_for_new_role = "0";

function OpenNewRole(id)
{
    role_id_for_new_role = id;

    $("#new_role_form").dialog({
        title: "New Role",
        width: 500,
        height: 250,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            
            if (id !== "0") {
                 $.ajax({
                    type: "POST",
                     url: "/Role/Role2",
                     beforeSend: function (xhr)
                     {
                         xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                     },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                     data: JSON.stringify({ RoleId: id }),
                     success: function (response) {
                         var data = JSON.parse(response);
                         if (data.Message == "OK") {
                             $(".new_role_title_txt").val(data.Role1.Name);
                             $("#new_role_page_select").val(data.Role1.ResourceId);
                         }
                         else {

                             ShowInformationDialog('Error', data.Message);
                         }
                     },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}

function UpdateRole(id) {
    OpenNewRole(id);
}
