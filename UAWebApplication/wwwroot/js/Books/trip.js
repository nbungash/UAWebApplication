
$(document).ready(function () {

    //var windowHeight = $(window).height();
    //var topOffset = $(".trip_table_div").offset().top;

    var tableHeight = $(window).height() - $(".trip_table_div").offset().top-200;
    $('#trip_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        selecte: true,
        "scrollY": tableHeight + "px",
        /*"scrollY": "310px",*/
        "scrollCollapse": true,
        bSort: true

    });

    $.ajax({
        type: "POST",
        url: "/ChartOfAccount/AccountsByGroupList",
        beforeSend: function (xhr) {},
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
                $(".trip_party_select").empty();
                $(".trip_party_select").append($("<option />").val(0).text("All"));
                $.each(data.AccountList, function (index, item) {
                    $(".trip_party_select").append($("<option />").val(item.AccountId).text(item.Title));
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

    $('.trip_filter_btn').on('click', function () {

        var error = false;
        var company_id = $(".trip_party_select").val();
        if (company_id === '' || company_id === null) {
            ShowInformationDialog('Error', "Party Missing");
            error = true;
        }
        var fromdate = $(".trip_from_dp").val();
        if (fromdate === '' || fromdate === null) {
            ShowInformationDialog('Error', "From Date Missing");
            error = true;
        }
        var todate = $(".trip_to_dp").val();
        if (todate === '' || todate === null) {
            ShowInformationDialog('Error', "To Date Missing");
            error = true;
        }

        if (error === false) {

            $.ajax({
                type: "POST",
                url: "/Trip/FilterByDateRange",
                beforeSend: function (xhr) {
                    $('.trip_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        CompanyId: company_id,
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
                    $('.trip_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }

    });

    $('.trip_print_btn').on('click', function () {

        var trip_list = [];
        $("#table1 > tbody > tr").each(function (i, v) {
            var xyz = {};
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    xyz["Id"] = $(this).text();
                }

            });
            trip_list.push(xyz);

        });

        $.ajax({
            url: "/Trip/ReportPreview",
            type: "POST",
            beforeSend: function (xhr) { },
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            data: JSON.stringify(
                {
                    TripList: trip_list,
                }),
            success: function (data) {
                var window1 = window.open('', '_blank');
                window1.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(data) + "'></iframe>");
            }
        });

    });
});

function PopulateTripTable(trip_list)
{
    var table = $('#trip_table').DataTable();
    $.each(trip_list, function (index, item) {
        var jRow2 = $('<tr><td style="text-align: center;padding:5px;">' + ((item.TripId === null) ? "" : item.TripId) + '</td>\
                <td style= "text-align: center;padding:5px;" > ' + ((item.EntryDate === null || item.EntryDate === "0001-01-01T00:00:00") ? "" : GetFormatedDate(item.EntryDate)) + '</td>\
                <td style= "text-align: center;padding:5px;" > ' + ((item.LorryTitle === null) ? "" : item.LorryTitle) + '</td>\
                <td style="text-align: center;padding:5px;">' + ((item.TokenNo === null) ? "" : item.TokenNo) + '</td>\
                <td style="text-align: left;padding:5px;" >' + ((item.Quantity === null) ? "" : item.Quantity) + '</td>\
                <td style="text-align: left;padding:5px;">' + ((item.Freight === null) ? "" : item.Freight) + '</td>\
                <td style="text-align: center;padding:5px;">' + ((item.Commission === null) ? "" : item.Commission) + '</td>\
                <td style="text-align: right;padding:5px;">' + ((item.Tax === null) ? "" : item.Tax) + '</td>\
                <td style="text-align: right;padding:5px;">' + ((item.ShortQty === null) ? "" : item.ShortQty) + '</td>\
                <td style="text-align: right;padding:5px;">' + ((item.ShortAmount === null) ? "" : item.ShortAmount) + '</td>\
                <td style="text-align: right;padding:5px;">' + ((item.TripAdvance === null) ? "" : item.TripAdvance) + '</td>\
                <td style="text-align: right;padding:5px;">' + ((item.Munshiana === null) ? "" : item.Munshiana) + '</td>\
                <td style="text-align: center;padding:5px;">\
                    <a href="javascript:NewTripWindowLoaded(' + item.TripId + ');">Update</a>\
                    <a href="javascript:DeleteTrip(' + item.TripId + ');">Delete</a></td>\
                </tr>');
        table.row.add(jRow2);
    });
    table.draw();
}

function DeleteTrip(id) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/Trip/DeleteTrip",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN",$('input:hidden[name="__RequestVerificationToken"]').val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ TripId: id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        ShowInformationDialog('Information', "Deleted Successfully.");
                        var table = $('#trip_table').DataTable();
                        var filteredData = table.rows().indexes().filter(function (value, index) {
                            return table.row(value).data()[0] == id;
                        });
                        table.rows(filteredData).remove().draw();
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
