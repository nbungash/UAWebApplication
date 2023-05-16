
$(document).ready(function () {

    $('#user_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        selecte: true

    });

    $.ajax({
        type: "GET",
        url: "/User/UserList",
        beforeSend: function (xhr) { },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {

                $("#user_table").DataTable().clear().draw();
                if (data.UserList != null) {
                    PopulateUserTable(data.UserList);
                }
            }
            else {

                ShowInformationDialog('Information', data.Message);

            }
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });

});

function PopulateUserTable(user_list) {

    var table = $('#user_table').DataTable();
    $.each(user_list, function (index, item) {
        var jRow = $('<tr><td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.UserName + '</td>\
            <td style="text-align: left;padding:5px;">' + item.Email + '</td>\
            <td style="text-align: center;padding:5px;">' + item.ActivationStatus + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:ActivateAccount(\'' + item.Id + '\');">Activate</a>  |  \
                <a href="javascript:DeActivateAccount(\'' + item.Id + '\');">De Activate</a></td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:OpenUserRoles(\'' + item.Id + '\');">Role</a>  |  \
                <a href="javascript:DeleteUser(\''+ item.Id + '\');">Delete</a></td></tr>');
        table.row.add(jRow);
    });
    table.draw();

}

function ActivateAccount(id) {
    var question = "Are You Sure to Activate User ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/User/ActivateUser",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ UserId: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "User Activated Successfully") {
                        $("#user_table > tbody > tr").each(function (i, v) {
                            var UserIdInTable = "";
                            $(this).children('td').each(function (ii, vv) {
                                if (ii === 0) {
                                    UserIdInTable = $(this).text();
                                }
                            });
                            if (UserIdInTable == id) {
                                $(this).children('td').each(function (ii, vv) {

                                    if (ii === 3) {
                                        $(this).text("Activated");
                                    }

                                });
                                return false;
                            }
                        });
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

function DeActivateAccount(id) {
    var question = "Are You Sure to De Activate User Account ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/User/DeActivateUser",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ UserId: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "User De Activated Successfully") {

                        $("#user_table > tbody > tr").each(function (i, v) {
                            var UserIdInTable = "";
                            $(this).children('td').each(function (ii, vv) {
                                if (ii === 0) {
                                    UserIdInTable = $(this).text();
                                }
                            });
                            if (UserIdInTable == id) {
                                $(this).children('td').each(function (ii, vv) {

                                    if (ii === 3) {
                                        $(this).text("Not Activated");
                                    }
                                    
                                });
                                return false;
                            }
                        });
                        
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

function DeleteUser(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/User/DeleteUser",
                beforeSend: function (xhr)
                {
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ UserId: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "User Deleted Successfully") {

                        var table = $('#user_table').DataTable();
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


