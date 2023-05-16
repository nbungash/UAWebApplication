
$(document).ready(function () {

    $('#destination_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All",10, 25, 50]],
        selecte: true,
        "scrollY": "300px",
        "scrollCollapse": true

    });

    $.ajax({
        type: "POST",
        url: "/ChartOfAccount/AccountsByGroupList",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
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
                $(".destination_company_select").empty();
                $(".destination_company_select").append($("<option />").val(0).text("Select..."));
                $.each(data.AccountList, function (index, item) {
                    $(".destination_company_select").append($("<option />").val(item.AccountId).text(item.Title));
                });
                $(".destination_company_select").val(0);
            }
            else {
                ShowInformationDialog("Error", data.Message);
            }
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });
    
    $('.destination_company_select').change(function () {
        var selected_item_value = $(".destination_company_select").val();
        $.ajax({
            type: "POST",
            url: "/Destination/DestinationsByCompanyList",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ CompanyId: selected_item_value }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    $("#destination_table").DataTable().clear().draw();
                    PopulateDestinationTable(data.DestinationList);
                }
                else {
                    ShowInformationDialog("Error", data.Message);
                }
            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });

    });

});

function PopulateDestinationTable(list) {
    var table = $('#destination_table').DataTable();
    $.each(list, function (index, item) {
        var jRow = $('<tr><td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.Title + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + item.TitleUrdu + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + ((item.DestinationCode === null) ? "" : item.DestinationCode) + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + item.CompanyTitle + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:DestinationWindowLoaded('+ item.Id + ');">Update</a>  |  \
                <a href="javascript:DeleteDestination('+ item.Id + ');">Delete</a></td>\
            </tr>');
        table.row.add(jRow);
    });
    table.draw();
}

function DeleteDestination(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/Destination/Delete",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ Id: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        var table = $('#destination_table').DataTable();
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

