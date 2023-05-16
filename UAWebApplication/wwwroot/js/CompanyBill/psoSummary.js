$(document).ready(function () {
    //viewTable
    $('#summary_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All", 10, 25, 50]],
        "select": false,
        "bSort": false,
        "scrollY": "300px",
        "scrollCollapse": true,
    });
    //company list
    $.ajax({
        type: "GET",
        async: false,
        url: "/PSOSummary/YearList",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
        },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $(".summary_year_combo_box").empty();
                $(".summary_year_combo_box").append($("<option />").val(0).text("Select Bill Date"));
                $.each(data.yearText, function (index, item) {
                    $(".summary_year_combo_box").append($("<option />").val(item).text(item));
                });
                $(".summary_year_combo_box").val("");
            }
            else {
                ShowInformationDialog('Error', data.Message);
            }

        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });

    $('.summary_year_combo_box').change(function () {

        var selectedYear = $(".summary_year_combo_box").val();
        $.ajax({
            type: "POST",
            async: false,
            url: "/PSOSummary/BillDateList",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(
                {
                    BillYear: selectedYear
                }
            ),
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message == "OK") {

                    $("#summary_table").DataTable().clear();
                    $(".summary_BillDate_combo_box").empty();
                    $(".summary_BillDate_combo_box").append($("<option />").val(0).text("Select Bill Date"));
                    $.each(data.DateList, function (index, item) {
                        $(".summary_BillDate_combo_box").append($("<option />").val(item).text(item));
                    });
                    $(".summary_BillDate_combo_box").val("");
                }
                else {
                    ShowInformationDialog('Error', data.Message);
                }

            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });
    });
   

    //Get SummaryList
    $('.summary_view_by_type_btn').on('click', function () {
        $('.summary_view_by_type_btn').prop('disabled', true);

        let error = false;
        var billDate = $('.summary_BillDate_combo_box').val();
        if (billDate === null || billDate === "") {
            ShowInformationDialog('Error', "Bill NO. Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/PSOSummary/SummaryView",
                beforeSend: function (xhr) {
                    $('.summary_View_ajax_loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        BillDate: billDate,
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {
                        $("#summary_table").DataTable().clear();
                        PopulateSummaryTable(data.SummaryView);
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                complete: function () {
                    $('.summary_view_by_type_btn').prop('disabled', false);
                    $('.summary_View_ajax_loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
        else {
            $('.summary_view_by_type_btn').prop('disabled', false);

        }
    });

    //Print 
    $('.summary_Print_btn').on('click', function () {
        $('.summary_Print_btn').prop('disabled', true);

        var error2 = false;
        var billDate = $('.summary_BillDate_combo_box').val();
        if (billDate === null || billDate === "") {
            ShowInformationDialog('Error', "Bill NO. Missing");
            error2 = true;
        }

        if (error2 == false) {
            $.ajax({
                url: "/PSOSummary/PSOSummaryPreview",
                type: "POST",
                beforeSend: function (xhr) {
                    $('.summary_View_ajax_loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                data: JSON.stringify(
                    {
                        BillDate: billDate
                    }
                ),
                success: function (data) {
                    var window1 = window.open('', '_blank');
                    window1.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(data) + "'></iframe>");
                },
                complete: function () {
                    $('.summary_View_ajax_loader').css("visibility", "hidden");
                    $('.summary_Print_btn').prop('disabled', false);
                }
            });
        }
        else {
            $('.summary_Print_btn').prop('disabled', false);

        }
    });
})

function PopulateSummaryTable(SummaryList) {
    var table = $('#summary_table').DataTable();
    $.each(SummaryList, function (index, item) {
        var jRow = $('<tr>\
        <td style= "text-align: center;padding:5px;" > ' + ((item.Sno === null) ? "" : item.Sno) + '</td>\
        <td style= "text-align: center;padding:5px;" > ' + ((item.BillDate === null) ? "" : item.BillDate) + '</td>\
        <td style= "text-align: center;padding:5px;" > ' + ((item.BillNo === null) ? "" : item.BillNo) + '</td>\
        <td style= "text-align: right;padding:5px;" > ' + ((item.Amount === null) ? "" : item.Amount) + '</td>\
        <td style= "text-align: left;padding:5px;"></td>\
        </tr>');
        table.row.add(jRow);
    });
    table.draw();
};