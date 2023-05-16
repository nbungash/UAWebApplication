
$(document).ready(function () {

    $('#vs_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": "230px",
        "scrollCollapse": true,
        "bSort": false
    });

    $('.vs_save_btn').on('click', function () {

        var tripdata = [];
        $("#vs_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            var check1 = false;
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    check1 = false;
                    var $chkbox = $(this).find('input[type="checkbox"]');
                    if ($chkbox.length) {
                        var status = $chkbox.prop('checked');
                        if (status === true) {
                            check1 = true;
                        }
                    }
                }
                if (ii === 1) {
                    xyz["TripId"] = $(this).text();
                }
            });
            if (check1 === true) {
                tripdata.push(xyz);
            }
        });
        $.ajax({
            type: "POST",
            url: "/CompanyPayment/SaveShortages",
            beforeSend: function (xhr) {
                $('.vs_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    PaymentId: payment_id_for_view_shortages,
                    TripList: tripdata
                }),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {
                    ShowInformationDialog('Information', "Saved Successfully.");
                }
                else {
                    ShowInformationDialog('Error', data.Message);
                }
            },
            complete: function () {
                $('.vs_ajax-loader').css("visibility", "hidden");
            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });

    });

    $('#view_shortages_form').bind('dialogclose', function (event, ui) {

        $(".vs_table").DataTable().clear().draw();
        payment_id_for_view_shortages = 0;

    });
    
});

function PopulateViewShortagesTable(TripList) {
    var table = $(".vs_table").DataTable();
    $.each(TripList, function (index, item) {
        var row = $('<tr><td style="text-align: center;padding:5px;"><input type="checkbox" ' + item.IsBilled +'/></td>\
            <td style="text-align: center;padding:5px;">' + item.TripId + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + GetFormatedDate(item.EntryDate) + '</td>\
            <td style="text-align: left;padding:5px;" >' + item.ShippingTitle + '</td>\
            <td style="text-align: left;padding:5px;">' + item.DestinationTitle + '</td>\
            <td style="text-align: enter;padding:5px;">' + ((item.TokenNo === null) ? "" : item.TokenNo) + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + GetFormatedDate(item.InvoiceDate) + '</td>\
            <td style= "text-align: center;padding:5px;" > ' + item.LorryTitle + '</td>\
            <td style="text-align: left;padding:5px;">' + item.ProductTitle + '</td>\
            <td style="text-align: right;padding:5px;">' + ((item.ShortQty === null) ? "" : item.ShortQty) + '</td>\
            <td style="text-align: right;padding:5px;">' + ((item.ShortAmount === null) ? "" : item.ShortAmount) + '</td>\
            </tr>');
        table.row.add(row);
    });
    table.draw();
}

var payment_id_for_view_shortages = 0;

function ViewShorgtagesWindowLoaded(payment_id)
{
    payment_id_for_view_shortages = payment_id;
    $("#view_shortages_form").dialog({
        title: "VIEW SHORTAGES",
        width: 1200,
        height: 500,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $.ajax({
                type: "POST",
                url: "/CompanyPayment/ViewShortages",
                beforeSend: function (xhr) {
                    $('.vs_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        PaymentId: payment_id
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {

                        $(".vs_table").DataTable().clear().draw();
                        if (data.TripList.length == 0) {
                            ShowInformationDialog("Information","No Trip to Display");
                        } else {
                            PopulateViewShortagesTable(data.TripList);
                        }
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.vs_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

}

