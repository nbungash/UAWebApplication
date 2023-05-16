
$(document).ready(function () {

    $('#user_roles_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All",10, 25, 50]],
        selecte: true,
        'sDom': 't',
        "bSort": false

    });

    $.ajax({
        type: "GET",
        url: "/User/ResourceList",
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $(".user_roles_page_select").empty();
                $.each(data.ResourceList, function (index, item) {
                    $(".user_roles_page_select").append($("<option />").val(item.Id).text(item.Title));
                });
                $(".user_roles_page_select").val(0);
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

    $('.user_roles_view_btn').on('click', function () {

        $('.user_roles_view_btn').prop('disabled', true);

        var error1 = false;
        var page1 = $('.user_roles_page_select').val();
        if (page1 === null || page1 === "" || page1 === 0) {
            ShowInformationDialog('Error', "Page Missing");
            $('.user_roles_view_btn').prop('disabled', false);
            error = true;
        }
        if (error1 === false) {
            $.ajax({
                type: "POST",
                url: "/User/UserRolesList",
                beforeSend: function (xhr)
                {
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        ResourceId: page1,
                        UserId: user_id4
                    }),
                success: function (response) {
                    var data = JSON.parse(response);

                    if (data.Message == "OK") {
                        $("#user_roles_table").DataTable().clear().draw();
                        var table = $('#user_roles_table').DataTable();
                        $.each(data.UserRolesList, function (index, item) {
                            if (item.IsChecked === true) {

                                var jRow = $('<tr><td style="text-align: center;padding:5px;"><input type="checkbox" checked></td>\
                                        <td style="text-align: center;padding:5px;">' + item.Id + '</td>\
                                        <td style= "text-align: left;padding:5px;" > ' + item.Name + '</td></tr>');
                                table.row.add(jRow).draw();

                            } else {

                                var jRow = $('<tr><td style="text-align: center;padding:5px;"><input type="checkbox"></td>\
                                        <td style="text-align: center;padding:5px;">' + item.Id + '</td>\
                                        <td style= "text-align: left;padding:5px;" > ' + item.Name + '</td></tr>');
                                table.row.add(jRow).draw();

                            }
                        });
                        $('.user_roles_view_btn').prop('disabled', false);
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
        }
    });

    $('.user_roles_save_btn').on('click', function () {

        $('.user_roles_save_btn').prop('disabled', true);

        var error = false;

        var page1 = $(".user_roles_page_select").val();
        if (page1 === '' || page1 === null || page1 === 0) {
            ShowInformationDialog('Error', "Page Missing");
            $('.user_roles_save_btn').prop('disabled', false);
            error = true;
        }
        var list2 = [];
        $("#user_roles_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            var check1 = false;
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    check1 = false;
                    var $chkbox = $(this).find('input[type="checkbox"]');
                    if ($chkbox.length) {
                        var status = $chkbox.prop('checked');
                        if (status === true) {
                            xyz["IsChecked"] = status;
                            check1 = true;
                        }
                        else {
                            xyz["IsChecked"] = false;
                        }
                    }
                }
                if (ii === 1) {
                    xyz["Id"] = $(this).text();
                }
                if (ii === 2) {
                    xyz["Name"] = $(this).text();
                }
            });
            list2.push(xyz);
        });

        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/User/SaveUserRoles",
                beforeSend: function (xhr) {
                    $('.user_roles_save_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        UserId: user_id4,
                        ResourceId: page1,
                        UserRolesList:list2
                    }
                ),
                success: function (response) {
                    var data = JSON.parse(response);
                    $('.user_roles_save_btn').prop('disabled', false);
                    ShowInformationDialog('Information', data.Message);
                },
                complete: function () {
                    $('.user_roles_save_btn').prop('disabled', false);
                    $('.user_roles_save_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $(document).on('change', '#user_roles_all_check', function () {
        var check1 = $('#user_roles_all_check').is(":checked");
        $("#user_roles_table > tbody > tr").each(function (i, v) {
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    $(this).find('input[type="checkbox"]').prop('checked', check1);

                }

            });

        });
    });

    $('#user_roles_form').bind('dialogclose', function (event, ui) {

        $(".user_roles_page_select").val("");
        
        $(".user_roles_table").DataTable().clear().draw();
        user_id4 = "0";

    });

});


var user_id4 = "0";

function OpenUserRoles(user_id3) {
    user_id4 = user_id3;
    $("#user_roles_form").dialog({
        title: "User Roles",
        width: 600,
        height: 400,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $.ajax({
                type: "GET",
                async: false,
                url: "/Role/ResourceList",
                contentType: "application/json",
                dataType: "json",
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".user_roles_page_select").empty();
                        $.each(data.ResourceList, function (index, item) {
                            $(".user_roles_page_select").append($("<option />").val(item.Id).text(item.Title));
                        });
                        $(".user_roles_page_select").val(0);
                    }
                    else
                    {
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