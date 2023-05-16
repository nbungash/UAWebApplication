
$(document).ready(function () {

    $('.cb_new_btn').on('click', function () {

        OpenNewCompanyBill(0);

    });

    $('.ncb_save_btn').on('click', function () {

        $('.ncb_save_btn').prop('disabled', true);

        var error1 = false;
        var company = $(".ncb_company_select").val();
        if (company === '' || company === null || company===0) {
            ShowInformationDialog('Error', "Company Missing");
            error1 = true;
        }
        var bill_no = $(".ncb_bill_no_txt").val();
        if (bill_no === '' || bill_no === null) {
            ShowInformationDialog('Error', "Bill No Missing");
            error1 = true;
        }

        var bill_date = $(".ncb_bill_date_dp").val();
        if (bill_date === '' || bill_date === null) {
            ShowInformationDialog('Error', "Bill Date Missing");
            error1 = true;
        }
        if (error1 === false) {
            $.ajax({
                type: "POST",
                url: "/CompanyBill/SaveBill",
                beforeSend: function (xhr) {
                    $('.ncb_save_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Id: company_bill_id_for_new_company_bill,
                        PartyId: company,
                        BillDate: bill_date,
                        BillNo: bill_no,
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        ShowInformationDialog('Information', "Saved Successfully");
                        $('.ncb_form').dialog('close');
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.ncb_save_btn').prop('disabled', false);
                    $('.ncb_save_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.ncb_save_btn').prop('disabled', false);
        }
    });

    $('#ncb_form').bind('dialogclose', function (event, ui) {

        company_bill_id_for_new_company_bill = 0;

        $(".ncb_company_select").val("");
        $(".ncb_bill_no_txt").val("");
        $(".ncb_bill_date_dp").val("");
    });
    
});

var company_bill_id_for_new_company_bill = 0;

function OpenNewCompanyBill(company_bill_id) {

    company_bill_id_for_new_company_bill = company_bill_id;

    $("#ncb_form").dialog({
        title: "COMPANY BILL",
        width: 550,
        height: 250,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            GetAccountsByGroup("PARTY", true, '.ncb_company_select');
            if (company_bill_id !== 0)
            {
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "/CompanyBill/NewCompanyBillWindowLoaded",
                    beforeSend: function (xhr) {
                        $('.ncb_save_ajax-loader').css("visibility", "visible");
                    },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ BillId: company_bill_id }),
                    success: function (response) {
                        var record = JSON.parse(response);
                        if (record.Message == "OK") {
                            $('.ncb_company_select').val(record.ObjToUpdate.PartyId);
                            $('.ncb_bill_no_txt').val(record.ObjToUpdate.BillNo);
                            $('.ncb_bill_date_dp').val(GetFormatedDate2(record.ObjToUpdate.BillDate));
                        }
                    },
                    complete: function () {
                        $('.ncb_save_ajax-loader').css("visibility", "hidden");
                    },
                    failure: function (response) {
                        alert('Some thing wrong');
                    }
                });
            }
        }
    });
}
