
$(document).ready(function () {

    $('#product_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All",10, 25, 50]],
        selecte: true

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
                $(".product_company_select").empty();
                $(".product_company_select").append($("<option />").val(0).text("Select..."));
                $.each(data.AccountList, function (index, item) {
                    $(".product_company_select").append($("<option />").val(item.AccountId).text(item.Title));
                });
                $(".product_company_select").val(0);
            }
            else {
                ShowInformationDialog("Error", data.Message);
            }
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });
    
    $('.product_company_select').change(function () {
        var selected_item_value = $(".product_company_select").val();
        $.ajax({
            type: "POST",
            url: "/Product/ProductsByCompanyList",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ CompanyId: selected_item_value }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    $("#product_table").DataTable().clear().draw();
                    PopulateProductTable(data.ProductList);
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

function PopulateProductTable(list) {
    var table = $('#product_table').DataTable();
    $.each(list, function (index, item) {
        var jRow = $('<tr><td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style= "text-align: left;padding:5px;" > ' + item.Title + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + item.TitleUrdu + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + ((item.ProductCode === null) ? "" : item.ProductCode) + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + item.CompanyTitle + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:ProductWindowLoaded('+ item.Id + ');">Update</a>  |  \
                <a href="javascript:DeleteProduct('+ item.Id + ');">Delete</a></td>\
            </tr>');
        table.row.add(jRow);
    });
    table.draw();
}

function DeleteProduct(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/Product/Delete",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ Id: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        var table = $('#product_table').DataTable();
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

