

$(document).ready(function () {

    $('#page_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        selecte: true

    });

    $.ajax({
        type: "GET",
        url: "/Resource/ResourceList",
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK")
            {
                $("#page_table").DataTable().clear().draw();
                PopulateResourceTable(data.ResourceList);
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

});

function PopulateResourceTable(resource_list)
{
    var table = $('#page_table').DataTable();

    $.each(resource_list, function (index, item) {
        var jRow = $('<tr><td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.Title + '</td>\
            <td style="text-align: center;padding:5px;"><a href="javascript:DeleteResource('+ item.Id + ');">Delete</a></td>\
            <td style="text-align: left;padding:5px;"></td></tr>');
        table.row.add(jRow);
    });
    table.draw();
}

function DeleteResource(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/Resource/DeleteResource",
                beforeSend: function (xhr)
                {
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ ResourceId: id }),
                success: function (response) {

                    var message = JSON.parse(response);

                    //$('#dialog-confirm').dialog("close");
                    if (message === "Resource Deleted Successfully") {

                        var table = $('#page_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return table.row(value).data()[0] == id;
                        });
                        table.rows(filteredData).remove().draw();
                    }
                    ShowInformationDialog('Information', message);
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



