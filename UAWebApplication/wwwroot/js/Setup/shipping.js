
$(document).ready(function () {

    $('#shipping_table').dataTable({
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
                IsActive:true
            }),
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $(".shipping_company_select").empty();
                $(".shipping_company_select").append($("<option />").val(0).text("Select..."));
                $.each(data.AccountList, function (index, item) {
                    $(".shipping_company_select").append($("<option />").val(item.AccountId).text(item.Title));
                });
                $(".shipping_company_select").val(0);
            }
            else {
                ShowInformationDialog("Error", data.Message);
            }
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });
    
    $('.shipping_company_select').change(function () {
        var selected_item_value = $(".shipping_company_select").val();
        $.ajax({
            type: "POST",
            url: "/Shipping/ShippingsByCompanyList",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ CompanyId: selected_item_value }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    $("#shipping_table").DataTable().clear().draw();
                    PopulateShippingTable(data.ShippingList);
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

function PopulateShippingTable(list) {
    var table = $('#shipping_table').DataTable();
    $.each(list, function (index, item) {
        var jRow = $('<tr><td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.Title + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + item.TitleUrdu + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + ((item.ShippingCode === null) ? "" : item.ShippingCode) + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + item.CompanyTitle + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:ShippingWindowLoaded('+ item.Id + ');">Update</a>  |  \
                <a href="javascript:DeleteShipping('+ item.Id + ');">Delete</a></td>\
            </tr>');
        table.row.add(jRow);
    });
    table.draw();
}

function DeleteShipping(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/Shipping/Delete",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ Id: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        var table = $('#shipping_table').DataTable();
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

