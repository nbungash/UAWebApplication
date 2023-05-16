
$(document).ready(function () {

    $('.nsti_shipping_province_select').change(function () {

        let selectedShippingProvinceId = $(this).val();
        if (selectedShippingProvinceId != 0) {
            $.ajax({
                type: "POST",
                url: "/NewSalesTaxInvoice/GetSalesTaxPerecentage",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ ProvinceId: selectedShippingProvinceId }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".nsti_shipping_sales_tax_percentage_txt").val(data.SalesTaxPercentage);
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

    $('.nsti_destination_province_select').change(function () {

        let selectedProvinceId = $(this).val();
        if (selectedProvinceId != 0) {
            $.ajax({
                type: "POST",
                url: "/NewSalesTaxInvoice/GetSalesTaxPerecentage",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ ProvinceId: selectedProvinceId }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $(".nsti_destination_sales_tax_percentage_txt").val(data.SalesTaxPercentage);
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

    $('.nsti_save_btn').on('click', function () {

        $('.nsti_save_btn').prop('disabled', true);

        var error1 = false;
        var invoice_date = $(".nsti_invoice_date_dp").val();
        if (invoice_date === '' || invoice_date === null) {
            ShowInformationDialog('Error', "Invoice date Missing");
            error1 = true;
        }
        var shipping_province = $(".nsti_shipping_province_select").val();
        if (shipping_province === '' || shipping_province === null || shipping_province === 0) {
            ShowInformationDialog('Error', "Shipping Province Missing");
            error1 = true;
        }
        var shipping_invoice_no = $(".nsti_shipping_invoice_no_txt").val();
        if (shipping_invoice_no === '' || shipping_invoice_no === null) {
            ShowInformationDialog('Error', "Shipping Invoice No Missing");
            error1 = true;
        }
        var shipping_sales_tax_percentage = $(".nsti_shipping_sales_tax_percentage_txt").val();
        if (shipping_sales_tax_percentage === '' || shipping_sales_tax_percentage === null) {
            ShowInformationDialog('Error', "Shipping Sales Tax Percentage Missing");
            error1 = true;
        }
        var shipping_sales_tax_freight = $(".nsti_shipping_sales_tax_freight_txt").val();
        if (shipping_sales_tax_freight === '' || shipping_sales_tax_freight === null) {
            ShowInformationDialog('Error', "Shipping Sales Tax Freight Missing");
            error1 = true;
        }
        var shipping_sales_tax_amount = $(".nsti_shipping_sales_tax_amount_txt").val();
        if (shipping_sales_tax_amount === '' || shipping_sales_tax_amount === null) {
            ShowInformationDialog('Error', "Shipping Sales Tax Amount Missing");
            error1 = true;
        }

        var destination_province = $(".nsti_destination_province_select").val();
        var destination_invoice_no = $(".nsti_destination_invoice_no_txt").val();
        var destination_sales_tax_percentage = $(".nsti_destination_sales_tax_percentage_txt").val();
        var destination_sales_tax_freight = $(".nsti_destination_sales_tax_freight_txt").val();
        var destination_sales_tax_amount = $(".nsti_destination_sales_tax_amount_txt").val();

        if (error1 === false) {
            $.ajax({
                type: "POST",
                url: "/NewSalesTaxInvoice/SaveSalesTaxInvoice",
                beforeSend: function (xhr) {
                    $('.nsti_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        InvoiceDate: invoice_date,
                        PartyBillId: company_bill_id_for_new_sales_tax_invoice,
                        ShippingProvince: shipping_province,
                        ShippingInvoiceNo: shipping_invoice_no,
                        ShippingSalesTaxPercentage: shipping_sales_tax_percentage,
                        DestinationProvince: destination_province,
                        DestinationInvoiceNo: destination_invoice_no,
                        DestinationSalesTaxPercentage: destination_sales_tax_percentage,
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        ShowInformationDialog('Information', "Saved Successfully.");
                        $('.new_sales_tax_invoice_form').dialog('close');
                    }
                    else {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.nsti_save_btn').prop('disabled', false);
                    $('.nsti_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.nsti_save_btn').prop('disabled', false);
        }
    });

    $('#new_sales_tax_invoice_form').bind('dialogclose', function (event, ui) {

        company_bill_id_for_new_sales_tax_invoice = 0;

        $(".nsti_invoice_date_dp").val("");
        
        $(".nsti_shipping_province_select").val("");
        $(".nsti_shipping_province_select").empty();
        $("nsti_shipping_invoice_no_txt").val("");
        $("nsti_shipping_sales_tax_percentage_txt").val("");
        
        $(".nsti_destination_province_select").val("");
        $(".nsti_destination_province_select").empty();
        $("nsti_destination_invoice_no_txt").val("");
        $("nsti_destination_sales_tax_percentage_txt").val("");
        
        $(".nsti_invoice_date_dp").prop('disabled', false);
        //$(".nsti_shipping_province_select").prop('disabled', false);
        $("nsti_shipping_invoice_no_txt").prop('readonly', false);
        $("nsti_shipping_sales_tax_percentage_txt").prop('readonly', false);
        //$(".nsti_destination_province_select").prop('disabled', false);
        $("nsti_destination_invoice_no_txt").prop('readonly', false);
        $("nsti_destination_sales_tax_percentage_txt").prop('readonly', false);
    });

    $('.nsti_print_shipping_invoice_btn').on('click', function () {

        $('.nsti_print_shipping_invoice_btn').prop('disabled', true);
        $.ajax({
            url: "/NewSalesTaxInvoice/ReportPreview",
            type: "POST",
            beforeSend: function (xhr) {
                $('.nsti_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            data: JSON.stringify(
                {
                    PartyBillId: company_bill_id_for_new_sales_tax_invoice,
                    Type: "Shipping"
                }),
            success: function (data) {
                var window1 = window.open('', '_blank');
                window1.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(data) + "'></iframe>");
            },
            complete: function () {
                $('.nsti_ajax-loader').css("visibility", "hidden");
                $('.nsti_print_shipping_invoice_btn').prop('disabled', false);
            }
        });

    });

    $('.nsti_print_destination_invoice_btn').on('click', function () {

        $('.nsti_print_destination_invoice_btn').prop('disabled', true);

        $.ajax({
            url: "/NewSalesTaxInvoice/ReportPreview",
            type: "POST",
            beforeSend: function (xhr) {
                $('.nsti_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            data: JSON.stringify(
                {
                    PartyBillId: company_bill_id_for_new_sales_tax_invoice,
                    Type: "Destination"
                }),
            success: function (data) {
                var window1 = window.open('', '_blank');
                window1.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(data) + "'></iframe>");
            },
            complete: function () {
                $('.nsti_ajax-loader').css("visibility", "hidden");
                $('.nsti_print_destination_invoice_btn').prop('disabled', false);
            }
        });
    });

});

var company_bill_id_for_new_sales_tax_invoice = 0;

function GenerateSalesTax(company_bill_id) {

    company_bill_id_for_new_sales_tax_invoice = company_bill_id;

    $("#new_sales_tax_invoice_form").dialog({
        title: "NEW SALES TAX INVOICE",
        width: 900,
        height: 300,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $(".nsti_print_shipping_invoice_btn").hide();
            $(".nsti_print_destination_invoice_btn").hide();
            $(".nsti_save_btn").show();
            $.ajax({
                type: "GET",
                async:false,
                url: "/Provinces/ProvincesList",
                beforeSend: function (xhr) {
                    $('.nsti_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        $(".nsti_shipping_province_select").empty();
                        $(".nsti_destination_province_select").empty();
                        $(".nsti_shipping_province_select").append($("<option />").val(0).text("Select..."));
                        $(".nsti_destination_province_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.ProvincesList, function (index, item) {
                            $(".nsti_shipping_province_select").append($("<option />").val(item.Id).text(item.Name));
                            $(".nsti_destination_province_select").append($("<option />").val(item.Id).text(item.Name));
                        });
                    }
                    else {
                        ShowInformationDialog('Information', data.Message);
                    }
                },
                complete: function () {
                    $('.nsti_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
            $.ajax({
                type: "POST",
                url: "/NewSalesTaxInvoice/GenerateSalesTax",
                beforeSend: function (xhr) {
                    $('.nsti_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        CompanyBillId: company_bill_id
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        $(".nsti_invoice_date_dp").prop('disabled', false);
                        $(".nsti_invoice_date_dp").val(GetFormatedDate2(data.InvoiceDate));
                        $(".nsti_shipping_province_select").val(data.ShippingProvinceId);
                        $(".nsti_shipping_invoice_no_txt").val(data.ShippingInvoiceNo);
                        $(".nsti_shipping_sales_tax_percentage_txt").val(data.ShippingSalesTaxPercentage);
                        $(".nsti_destination_province_select").val(data.DestinationProvinceId);

                        if (data.ShippingProvinceId != data.DestinationProvinceId) {

                            $(".nsti_destination_invoice_no_txt").val(data.DestinationInvoiceNo);
                            $(".nsti_destination_sales_tax_percentage_txt").val(data.DestinationSalesTaxPercentage);
                        }
                        else {
                            $(".nsti_destination_invoice_no_txt").val("");
                            $(".nsti_destination_sales_tax_percentage_txt").val("");
                        }
                        $(".nsti_invoice_date_dp").prop('disabled', false);
                        $(".nsti_shipping_province_select").prop('disabled', false);
                        $("nsti_shipping_invoice_no_txt").prop('readonly', false);
                        $("nsti_shipping_sales_tax_percentage_txt").prop('readonly', false);
                        $(".nsti_destination_province_select").prop('disabled', false);
                        $("nsti_destination_invoice_no_txt").prop('readonly', false);
                        $("nsti_destination_sales_tax_percentage_txt").prop('readonly', false);
                    }
                    else {
                        ShowInformationDialog('Information', data.Message);
                    }
                },
                complete: function () {
                    $('.nsti_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });
}

function ViewAndPrintSalesTax(company_bill_id) {
    company_bill_id_for_new_sales_tax_invoice = company_bill_id;

    $("#new_sales_tax_invoice_form").dialog({
        title: "View & Print Sales Tax Invoice",
        width: 900,
        height: 300,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $(".nsti_print_shipping_invoice_btn").show();
            $(".nsti_print_destination_invoice_btn").show();
            $(".nsti_save_btn").hide();
            $.ajax({
                type: "GET",
                async: false,
                url: "/Provinces/ProvincesList",
                beforeSend: function (xhr) {
                    $('.nsti_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        $(".nsti_shipping_province_select").empty();
                        $(".nsti_destination_province_select").empty();
                        $(".nsti_shipping_province_select").append($("<option />").val(0).text("Select..."));
                        $(".nsti_destination_province_select").append($("<option />").val(0).text("Select..."));
                        $.each(data.ProvincesList, function (index, item) {
                            $(".nsti_shipping_province_select").append($("<option />").val(item.Id).text(item.Name));
                            $(".nsti_destination_province_select").append($("<option />").val(item.Id).text(item.Name));
                        });
                    }
                    else {
                        ShowInformationDialog('Information', data.Message);
                    }
                },
                complete: function () {
                    $('.nsti_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
            $.ajax({
                type: "POST",
                url: "/NewSalesTaxInvoice/GenerateSalesTax",
                beforeSend: function (xhr) {
                    $('.nsti_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        CompanyBillId: company_bill_id
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {

                        $(".nsti_invoice_date_dp").val(GetFormatedDate2(data.InvoiceDate));
                        $(".nsti_shipping_province_select").val(data.ShippingProvinceId);
                        $(".nsti_shipping_invoice_no_txt").val(data.ShippingInvoiceNo);
                        $(".nsti_shipping_sales_tax_percentage_txt").val(data.ShippingSalesTaxPercentage);

                        if (data.DestinationInvoiceNo != null) {
                            $(".nsti_destination_province_select").val(data.DestinationProvinceId);
                            $(".nsti_destination_invoice_no_txt").val(data.DestinationInvoiceNo);
                            $(".nsti_destination_sales_tax_percentage_txt").val(data.DestinationSalesTaxPercentage);
                        }
                        else {
                            $(".nsti_destination_province_select").val("");
                            $(".nsti_destination_invoice_no_txt").val("");
                            $(".nsti_destination_sales_tax_percentage_txt").val("");
                        }
                    }
                    else {
                        ShowInformationDialog('Information', data.Message);
                    }
                },
                complete: function () {
                    $('.nsti_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });

            $(".nsti_invoice_date_dp").prop('disabled', true);
            $(".nsti_shipping_province_select").prop('disabled', true);
            $("nsti_shipping_invoice_no_txt").prop('readonly', true);
            $("nsti_shipping_sales_tax_percentage_txt").prop('readonly', true);
            $(".nsti_destination_province_select").prop('disabled', true);
            $("nsti_destination_invoice_no_txt").prop('readonly', true);
            $("nsti_destination_sales_tax_percentage_txt").prop('readonly', true);
        }
    });
}

