
$(document).ready(function () {

    $('.trip_find_btn').on('click', function () {
        $("#find_trip_form").dialog({
            title: "Search Trips",
            width: 600,
            height: 310,
            open: function () {
                // On open, hide the original submit button
                $(this).find("[type=submit]").hide();
                $.ajax({
                    type: "POST",
                    url: "/ChartOfAccount/AccountsByGroupList",
                    beforeSend: function (xhr) { },
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
                            $(".ft_company_select").empty();
                            $(".ft_company_select").append($("<option />").val(0).text("All"));
                            $.each(data.AccountList, function (index, item) {
                                $(".ft_company_select").append($("<option />").val(item.AccountId).text(item.Title));
                            });
                        }
                        else {
                            ShowInformationDialog("Erro", data.Message);
                        }
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
                $.ajax({
                    type: "POST",
                    url: "/ChartOfAccount/AccountsByGroupList",
                    beforeSend: function (xhr) { },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify(
                        {
                            GroupType: "LORRY",
                            IsActive: true
                        }),
                    success: function (response) {
                        var data = JSON.parse(response);
                        if (data.Message == "OK") {
                            $(".ft_lorry_select").empty();
                            $(".ft_lorry_select").append($("<option />").val(0).text("All"));
                            $.each(data.AccountList, function (index, item) {
                                $(".ft_lorry_select").append($("<option />").val(item.AccountId).text(item.Title));
                            });
                        }
                        else {
                            ShowInformationDialog("Erro", data.Message);
                        }
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
                $(".ft_company_select").trigger("change");
                $(".ft_date_criteria_select").val(1);

            }
        });

    });

    $('.ft_company_select').change(function () {
        var company_id = parseInt($(this).val());
        $(".ft_shipping_select").empty();
        $(".ft_shipping_select").append($("<option />").val(0).text("All"));
        GetShippingList(company_id);
        $(".ft_destination_select").empty();
        $(".ft_destination_select").append($("<option />").val(0).text("All"));
        GetDestinationList(company_id);
        $(".ft_product_select").empty();
        $(".ft_product_select").append($("<option />").val(0).text("All"));
        GetProductList(company_id);
    });

    $('.ft_view_btn').on('click', function () {

        var error = false;
        var fromdate = $(".ft_from_dp").val();
        if (fromdate === '' || fromdate === null) {
            ShowInformationDialog('Error', "From Date Missing");
            error = true;
        }
        var todate = $(".ft_to_dp").val();
        if (todate === '' || todate === null) {
            ShowInformationDialog('Error', "To Date Missing");
            error = true;
        }

        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/Trip/FindByAll",
                beforeSend: function (xhr) {
                    $('.ft_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        CompanyId: $(".ft_company_select").val(),
                        ShippingId: $(".ft_shipping_select").val(),
                        DestinationId: $(".ft_destination_select").val(),
                        ProductId: $(".ft_product_select").val(),
                        DateCriteria: $(".ft_date_criteria_select").val(),
                        LorryId: $(".ft_lorry_select").val(),
                        FromDate: fromdate,
                        ToDate: todate
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#trip_table").DataTable().clear().draw();
                        if (data.TripList.length == 0) {
                            ShowInformationDialog("Information", "Oops! No Trip to Display");
                        }
                        else {
                            PopulateTripTable(data.TripList);
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.ft_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });
    $('.ft_trip_id_btn').on('click', function () {

        var error = false;
        var trip_id = $('.ft_trip_id_txt').val();
        if (trip_id === '' || trip_id === null) {
            ShowInformationDialog('Error', "Trip Id Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/Trip/FindByTripId",
                beforeSend: function (xhr) {
                    $('.ft_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ TripId: trip_id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#trip_table").DataTable().clear().draw();
                        if (data.TripList.length == 0) {
                            ShowInformationDialog("Information", "Oops! No Trip to Display");
                        }
                        else {
                            PopulateTripTable(data.TripList);
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.ft_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });
    $('.ft_invoice_no_btn').on('click', function () {

        var error = false;
        var invoice_no = $('.ft_invoice_no_txt').val();
        if (invoice_no === '' || invoice_no === null) {
            ShowInformationDialog('Error', "Invoice No Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/Trip/FindByInvoiceNo",
                beforeSend: function (xhr) { $('.ft_ajax-loader').css("visibility", "visible"); },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ InvoiceNo: invoice_no }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#trip_table").DataTable().clear().draw();
                        if (data.TripList.length == 0) {
                            ShowInformationDialog("Information", "Oops! No Trip to Display");
                        }
                        else {
                            PopulateTripTable(data.TripList);
                        }
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.ft_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });

    $('#find_trip_form').bind('dialogclose', function (event, ui) {

        $('.ft_invoice_no_txt').val("");

        $(".ft_from_dp").val("");
        $(".ft_to_dp").val("");
        $(".ft_company_select").val(0);
        $(".ft_shipping_select").val(0);
        $(".ft_destination_select").val(0);
        $(".ft_product_select").val(0);
        $(".ft_lorry_select").val(0);
        $(".ft_date_criteria_select").val(0);
    });
});

function GetShippingList(company_id) {
    $.ajax({
        type: "POST",
        url: "/Shipping/ShippingsByCompanyList",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
        },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({ CompanyId: company_id }),
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $.each(data.ShippingList, function (index, item) {
                    $(".ft_shipping_select").append($("<option />").val(item.Id).text(item.Title));
                });
            }
            else {
                ShowInformationDialog("");
            }
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });
}
function GetDestinationList(company_id) {
    $.ajax({
        type: "POST",
        url: "/Destination/DestinationsByCompanyList",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
        },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({ CompanyId:company_id }),
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $.each(data.DestinationList, function (index, item) {
                    $(".ft_destination_select").append($("<option />").val(item.Id).text(item.Title));
                });
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
function GetProductList(company_id) {
    $.ajax({
        type: "POST",
        url: "/Product/ProductsByCompanyList",
        beforeSend: function (xhr) { },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({ CompanyId: company_id }),
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $.each(data.ProductList, function (index, item) {
                    $(".ft_product_select").append($("<option />").val(item.Id).text(item.Title));
                });
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

