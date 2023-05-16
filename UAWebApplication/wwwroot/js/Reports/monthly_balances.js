
$(document).ready(function () {

    $('#mlb_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All", 10, 25, 50]],
        select: true,
        //'sDom': 't',
        "scrollY": "330px",
        "scrollCollapse": true,
        "bSort": false,
        
    });

    $('.mlb_view_btn').on('click', function () {
        var error = false;
        var month = $(".mlb_month_dp").val();
        if (month === '' || month === null) {
            ShowInformationDialog('Error', "month Missing");
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/MonthlyBalances/ViewRecords",
                beforeSend: function (xhr) {
                    $('.mlb_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Month: month
                    }
                ),
                success: function (response) {

                    var data = JSON.parse(response);

                    if (data.Message == "OK") {

                        $(".mlb_table").DataTable().clear().draw();
                        var table = $(".mlb_table").DataTable();
                        $.each(data.RecordList, function (index, item) {
                            var row = $('<tr>\
                                <td style = "text-align: center;padding:5px;" > ' + ((item.sno === null) ? "" : item.sno) + '</td >\
                                <td style= "text-align: center;padding:5px;" > ' + ((item.Lorry === null) ? "" : item.Lorry) + '</td>\
                                <td style= "text-align: right;padding:5px;" > ' + ((item.Commission === null) ? "" : item.Commission) + '</td>\
                                <td style= "text-align: right;padding:5px;" > ' + ((item.MonthBalananceDebit === null) ? "" : item.MonthBalananceDebit) + '</td>\
                                <td style= "text-align: right;padding:5px;" > ' + ((item.MonthBalananceCredit === null) ? "" : item.MonthBalananceCredit) + '</td>\
                                <td style= "text-align: right;padding:5px;" ></td>\
                                </tr>');
                            table.row.add(row);
                        });
                        table.draw();
                    }
                    else
                    {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.mlb_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }


    });

    $('.mlb_print_btn').on('click', function ()
    {
        var error = false;
        var month = $(".mlb_month_dp").val();
        if (month === '' || month === null) {
            ShowInformationDialog('Error', "Month Missing");
            error = true;
        }
        var recordsList = [];
        $("#mlb_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["sno"] =parseInt(text);
                    }
                }
                else if (ii === 1) {
                    xyz["Lorry"] = $(this).text();
                }
                else if (ii ===2) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["Commission"] = parseFloat(text);
                    }
                }
                else if (ii === 3) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["MonthBalananceDebit"] = parseFloat(text);
                    }
                }
                else if (ii === 4) {
                    let text = $(this).text();
                    if (text != null && text != "null" && text != "") {
                        xyz["MonthBalananceCredit"] = parseFloat(text);
                    }
                }
                
            });
            recordsList.push(xyz);
        });

        if (error === false) {

            $.ajax({
                url: "/MonthlyBalances/ReportPreview",
                type: "POST",
                beforeSend: function (xhr) { $('.mlb_ajax-loader').css("visibility", "visible"); },
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                data: JSON.stringify(
                    {
                        Month: month,
                        RecordsList: recordsList
                    }),
                success: function (data) {
                    var window1 = window.open('', '_blank');
                    window1.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + data + "'></iframe>");
                },
                complete: function () {
                    $('.mlb_ajax-loader').css("visibility", "hidden");
                }
            });

        }
        
    });
  
});
