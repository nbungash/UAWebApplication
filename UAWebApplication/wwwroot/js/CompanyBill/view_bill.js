
$(document).ready(function () {

    $('#cb_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": "330px",
        "scrollCollapse": true,
        "bSort": false
    });

    GetAccountsByGroup("PARTY", true, '.cb_company_select');

    $.ajax({
        type: "GET",
        url: "/CompanyBill/Years",
        beforeSend: function (xhr) { $('.cb_ajax-loader').css("visibility", "visible"); },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message === "OK") {
                $(".cb_year_select").empty();
                $.each(data.YearList, function (index, item) {
                    $(".cb_year_select").append($("<option />").val(item).text(item));
                });
            }
            else {
                ShowInformationDialog('Error', data.Message);
            }
        },
        complete: function () {
            $('.cb_ajax-loader').css("visibility", "hidden");
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });

    $('.cb_view_btn').on('click', function () {

        var error = false;
        var year = $(".cb_year_select").val();
        if (year === '' || year === null) {
            ShowInformationDialog('Error', "Year Missing");
            error = true;
        }
        var company = $(".cb_company_select").val();
        if (company === '' || company === null || company === 0) {
            ShowInformationDialog('Error', "Company Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/CompanyBill/BillList",
                beforeSend: function (xhr) { $('.cb_ajax-loader').css("visibility", "visible"); },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        CompanyId: company,
                        Year: year
                    }),
                success: function (response) {

                    var data = JSON.parse(response);
                    if (data.Message === "OK") {

                        $(".cb_table").DataTable().clear().draw();
                        var table = $(".cb_table").DataTable();

                        $.each(data.PartyBillList, function (index, item) {
                            var row = $('<tr>\
                                <td style="text-align: center;padding:3px;"><input id="cb_is_finalized_check" value="'+ item.Id + '" type="checkbox" ' + item.IsChecked +'/></td>\
                                <td style="text-align: center;padding:3px;">' + item.Id + '</td>\
                                <td style="text-align: center;padding:3px;">' + item.BillNo + '</td>\
                                <td style="text-align: center;padding:3px;" >' + GetFormatedDate(item.BillDate) + '</td>\
                                <td style="text-align: center;padding:3px;">\
                                    <a href="javascript:PrintPartyBill('+ item.Id + ');">Print Bill</a></td>\
                                <td style="text-align: center;padding:3px;">\
                                    <a href="javascript:ViewTripsWindowLoaded('+ item.Id + ');">View</a></td>\
                                <td style="text-align: center;padding:3px;">\
                                    <a href="javascript:ViewAndPrintSalesTax('+ item.Id + ');">Print</a> | \
                                    <a href="javascript:GenerateSalesTax('+ item.Id + ');">Generate</a></td>\
                                <td style="text-align: center;padding:3px;">\
                                    <a href="javascript:OpenNewCompanyBill('+ item.Id + ');">Update</a>  |  \
                                    <a href="javascript:DeleteCompanyBill('+ item.Id + ');">Delete</a></td>\
                                </tr>');
                            table.row.add(row);
                        });
                        table.draw();
                    }
                    else {
                        ShowInformationDialog('Information', data.Message);
                    }
                },
                complete: function () {
                    $('.cb_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $(document).on("change", "#cb_is_finalized_check", function () {

        var cb_is_finalized_check = $(this).is(':checked');
        var id = $(this).val();

        $.ajax({
            url: "/CompanyBill/ChangeCompanyBillStatus",
            type: "POST",
            beforeSend: function (xhr) {
                $('.cb_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    CompanyBillId: id,
                    IsFinalized: cb_is_finalized_check
                }
            ),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    if (cb_is_finalized_check == true) {
                        ShowInformationDialog("Information", "Bill Id: " + id + " is Finalized Now.");
                    }
                    else {
                        ShowInformationDialog("Information", "Bill Id: " + id + " is Editable Now.");
                    }
                }
                else
                {

                    ShowInformationDialog("Error", data.Message);

                }
            },
            complete: function () {
                $('.cb_ajax-loader').css("visibility", "hidden");
            }
        });

    });


});

var selected_bill_id_for_company_bill = 0;

function DeleteCompanyBill(bill_id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) === "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/CompanyBill/DeletePartyBill",
                beforeSend: function (xhr) { },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ BillId: bill_id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        ShowInformationDialog('Information', "Deleted Successfully.");
                        var table = $('#cb_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return table.row(value).data()[0] == bill_id;
                        });
                        table.rows(filteredData).remove().draw();
                        $('#dialog-confirm').dialog("close");
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

function PrintPartyBill(party_bill_id) {

    $.ajax({
        url: "/CompanyBill/PSOBillPreview",
        type: "POST",
        beforeSend: function (xhr) {
            $('.cb_ajax-loader').css("visibility", "visible");
        },
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify(
            {
                BillId: party_bill_id
            }),
        success: function (data) {
            var window1 = window.open('', '_blank');
            window1.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(data) + "'></iframe>");
        },
        complete: function () {
            $('.cb_ajax-loader').css("visibility", "hidden");
        }
    });
}