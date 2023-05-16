
$(document).ready(function ()
{

    $('.cp_new_btn').on('click', function () {

        OpenNewCompanyPayment(0);

    });

    $('.ncp_save_btn').on('click', function () {
        $('.ncp_save_btn').prop('disabled', true);

        var error1 = false;
        var company = $(".ncp_company_select").val();
        if (company === '' || company === null || company===0) {
            ShowInformationDialog('Error', "Company Missing");
            $('.ncb_save_btn').prop('disabled', false);
            error1 = true;
        }
        var date1 = $(".ncp_payment_date_dp").val();
        if (date1 === '' || date1 === null) {
            ShowInformationDialog('Error', "Date Missing");
            $('.ncp_save_btn').prop('disabled', false);
            error1 = true;
        }
        if (error1 === false) {
            $.ajax({
                type: "POST",
                url: "/NewCompanyPayment/SavePayment",
                beforeSend: function (xhr) {
                    $('.ncp_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        CompanyId: company,
                        PaymentDate: date1,
                        Id: company_payment_id_for_new_payment
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data === "OK") {
                        ShowInformationDialog('Information', "Saved Successfully");
                        $('.ncp_form').dialog('close');
                    }
                    else {
                        ShowInformationDialog('Information', data.Message);

                    }
                },
                complete: function () {
                    $('.ncp_save_btn').prop('disabled', false);
                    $('.ncp_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('#ncp_form').bind('dialogclose', function (event, ui) {

        $(".ncp_company_select").val(0);
        $(".ncp_payment_date_dp").val("");
        company_payment_id_for_new_payment = 0;

    });
});

var company_payment_id_for_new_payment = 0;

function OpenNewCompanyPayment(company_payment_id) {

    company_payment_id_for_new_payment = company_payment_id;

    $("#ncp_form").dialog({
        title: "NEW COMPANY PAYMENT",
        width: 500,
        height: 250,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            GetAccountsByGroup("PARTY", true, '.ncp_company_select');
            if (company_payment_id !== 0 && company_payment_id !== null)
            {
                $.ajax({
                    type: "POST",
                    url: "/NewCompanyPayment/NewCompanyPaymentWindowLoaded",
                    beforeSend: function (xhr) {},
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ CompanyPaymentId: company_payment_id }),
                    success: function (response) {
                        var record = JSON.parse(response);
                        if (record.Message === "OK") {
                            $.each(record.CompanyPaymentList, function (index, item) {
                                $('.ncp_company_select').val(item.CompanyId);
                                $('.ncp_payment_date_dp').val(GetFormatedDate2(item.SummaryDate));
                            });
                            
                        }
                        else
                        {
                            ShowInformationDialog('Error', record.Message);
                        }
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}
