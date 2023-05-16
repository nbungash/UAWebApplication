
$(document).ready(function () {

    $('#cp_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": "350px",
        "scrollCollapse": true,
        "bSort": false
    });

    GetAccountsByGroup("PARTY", true, '.cp_company_select');

    $.ajax({
        type: "GET",
        url: "/CompanyPayment/Years",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message === "OK") {
                $(".cp_year_select").empty();
                $(".cp_year_select").append($("<option />").val(0).text("Select..."));
                $.each(data.YearList, function (index, item) {
                    $(".cp_year_select").append($("<option />").val(item).text(item));
                });
                $(".cp_year_select").val(0);
            }
            else {
                ShowInformationDialog('Error', data.Message);
            }
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });

    $('.cp_view_btn').on('click', function () {
        ViewSummary();
    });

    $(document).on("change", "#finalized_check", function () {

        var finalized_check = $(this).is(':checked');
        var id = $(this).val();

        $.ajax({
            url: "/CompanyPayment/ChangeCompanyPaymentStatus",
            type: "POST",
            beforeSend: function (xhr) {
                $('.cp_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    CompanyPaymentId: id,
                    IsFinalized: finalized_check
                }
            ),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    if (finalized_check == true) {
                        var table = $('#cp_table').DataTable();
                        table.rows().every(function (index, element) {
                            var row = $(this.node());
                            var data = this.data();
                            if (data[1] == id) {
                                $('td', row).css('background-color', 'lightblue');
                            }
                        });
                        $("#cp_table > tbody > tr").each(function (i, v) {
                            var IdInTable = "";
                            $(this).children('td').each(function (ii, vv) {
                                if (ii == 1) {
                                    IdInTable = $(this).text();
                                }
                            });
                            if (IdInTable == id) {
                                $(this).children('td').each(function (ii, vv) {
                                    if (ii == 5) {
                                        $(this).text("Finalized");
                                    }
                                    if (ii == 8) {

                                        $(this).html('<a href="javascript:OpenViewTrips(' + id + ');">View</a>');
                                    }
                                    if (ii == 11) {

                                        $(this).html('<a href="javascript:OpenViewOnlinePayment(' + id + ');">View</a>');
                                    }
                                    if (ii == 12) {

                                        $(this).html('');
                                    }
                                });
                            }
                        });
                        ShowInformationDialog("Info", "Payment No:" + id + " is Finalized Now.");
                    }
                    else {
                        var table = $('#cp_table').DataTable();
                        table.rows().every(function (index, element) {
                            var row = $(this.node());
                            var data = this.data();
                            if (data[1] == id) {
                                $('td', row).css('background-color', 'white');
                            }
                        });
                        $("#cp_table > tbody > tr").each(function (i, v) {
                            var IdInTable = "";
                            $(this).children('td').each(function (ii, vv) {
                                if (ii == 1) {
                                    IdInTable = $(this).text();
                                }
                            });
                            if (IdInTable == id) {
                                $(this).children('td').each(function (ii, vv) {
                                    if (ii == 5) {
                                        $(this).text("Editable");
                                    }
                                    if (ii == 8) {
                                        $(this).html('<a href="javascript:OpenViewTrips(' + id + ');">View</a>  |  \
                                            <a href="javascript:OpenAttachTripsToCompannyPayment(' + id + ');">Attach</a>');
                                    }
                                    if (ii == 11) {

                                        $(this).html('<a href="javascript:OpenViewOnlinePayment(' + id + ');">View</a> | \
                                                        <a href="javascript:OpenAttachOnlinePayment(' + id + ');">Attach</a>');
                                    }
                                    if (ii == 12) {

                                        $(this).html('<a href="javascript:CompanyPaymentUpdate(' + id + ');">Update</a> | \
                                                        <a href="javascript:CompanyPaymentDelete(' + id + ');">Delete</a>');
                                    }
                                });
                            }
                        });
                        ShowInformationDialog("Info", "Payment No:"+ id +" is Editable Now.");
                    }
                }
                else {

                    ShowInformationDialog("Error", data.Message);

                }
            },
            complete: function () {
                $('.cp_ajax-loader').css("visibility", "hidden");
            }
        });

    });

});

function ViewSummary()
{
    var error = false;
    var company_id = $('.cp_company_select').val();
    if (company_id === null || company_id === 0) {
        ShowInformationDialog('Error', "Company Missing");
        error = true;
    }
    var year = parseInt($('.cp_year_select').val());
    if (year === null || year === '' || year==0) {
        ShowInformationDialog('Error', "Year Missing");
        error = true;
    }
    if (error === false) {
        $.ajax({
            type: "POST",
            url: "/CompanyPayment/ViewPayments",
            beforeSend: function (xhr) {
                $('.cp_view_btn').prop('disabled', true);
                $('.cp_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    Year: year,
                    CompanyId: company_id
                }),
            success: function (response) {
                var obj = JSON.parse(response);
                if (obj.Message === "OK") {
                    $(".cp_table").DataTable().clear().draw();
                    PopulateCompanyPaymentTable(obj.CompanyPaymentList);
                }
                else {
                    ShowInformationDialog('Information', obj.Message);
                }
            },
            complete: function () {
                $('.cp_view_btn').prop('disabled', false);
                $('.cp_ajax-loader').css("visibility", "hidden");
            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });
    }
}

function PopulateCompanyPaymentTable(CompanyPaymentList) {
    var table3 = $("#cp_table").DataTable();
    $.each(CompanyPaymentList, function (index, item) {

        var row3 = $('<tr><td style="text-align: center;padding:5px;"><input id="cp_is_finalized_check" value="' + item.Id + '" type="checkbox" ' + item.Finalized + '/></td>\
            <td style="text-align: center;padding:5px;">' + item.Id + '</td>\
            <td style="text-align: center;padding:5px;">' + GetFormatedDate(item.SummaryDate) + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + item.CompanyTitle + '</td>\
            <td style="text-align: center;padding:5px;">' + item.MonthTitle + '</td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:OpenSummary(' + item.Id + ');">Print</a></td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:AttachTransactionsWindowLoaded(' + item.Id + ');">Attach</a> | \
                <a href="javascript:ViewTransactionsWindowLoaded(' + item.Id + ');">View</a></td>\
            <td style="text-align: center;padding:5px;"><a href="javascript:ViewTripsWindowLoaded('+ item.Id + ');">Freight</a></td>\
            <td style="text-align: center;padding:5px;"><a href="javascript:ViewShorgtagesWindowLoaded('+ item.Id + ');">Shortages</a></td>\
            <td style="text-align: center;padding:5px;">\
                <a href="javascript:OpenNewCompanyPayment(' + item.Id + ');">Update</a> | \
                <a href="javascript:CompanyPaymentDelete(' + item.Id + ');">Delete</a></td>\
            </tr>');
        table3.row.add(row3);
    });
    table3.draw();
}

var company_payment_id = 0;

function CompanyPaymentDelete(company_payment_id2)
{
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) === "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/CompanyPayment/CompanyPaymentDelete",
                beforeSend: function (xhr) { },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ CompanyPaymentId: company_payment_id2 }),
                success: function (response) {
                    var message = JSON.parse(response);
                    if (message == "Deleted Successfully") {
                        var table = $('#cp_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return TryParseFloat(table.row(value).data()[1]) === company_payment_id2;
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
    });

}

function FinalizeOrEditablePaymentSummary(payment_summary_id) {

    FinalizedSummary(payment_summary_id);
}

function FinalizedSummary(payment_summary_id) {

    var question = "Are You Sure to Finalized Summary ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/CompanyPayment/FinalizeCompanyPayment",
                beforeSend: function (xhr) {
                    //$('.uf_main_ajax-loader').css("visibility", "visible");
                    //$("#pb2_main_div").find("*").prop('disabled', true);
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        CompanyPaymentId: payment_summary_id
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    ShowInformationDialog('Information', data.Message);
                    if (data.Message == "Saved Successfully") {
                        //$("#pb2_main_div").find("*").prop('disabled', false);
                        ViewSummary();
                    }
                },
                complete: function () {
                    //$('.uf_main_ajax-loader').css("visibility", "hidden");
                    //$("#pb2_main_div").find("*").prop('disabled', false);
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

}
