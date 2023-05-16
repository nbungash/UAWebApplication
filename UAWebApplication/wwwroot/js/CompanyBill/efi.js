
$(document).ready(function () {

    PageLoaded();

    $('.efi_view_btn').on('click', function () {

        var formData = new FormData();
        var file = document.getElementById("FileUpload").files[0];
        formData.append("file", file);

        $.ajax({
            type: "POST",
            beforeSend: function () {
                $('.efi_ajax-loader').css("visibility", "visible");
            },
            url: "/EFI/UploadFile",
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                var data = JSON.parse(response);
                if (data.Message === "OK") {
                    $(".efi_table").DataTable().clear().draw();
                    var table = $('.efi_table').DataTable();
                    $.each(data.TripList, function (index, item) {
                        var row = $('<tr><td style="text-align: center;padding:8px;">' + item.TripId + '</td>\
                            <td style="text-align: center;padding:8px;">' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate)) + '</td>\
                            <td style= "text-align: center;padding:8px;" > ' + ((item.LorryTitle === null) ? "" : item.LorryTitle) + '</td>\
                            <td style="text-align: center;padding:8px;">' + ((item.InvoiceDate === null) ? "" : GetFormatedDate(item.InvoiceDate)) + '</td>\
                            <td style="padding:1px;"><input type="text" style="width: 100%;text-align: center;" value="' + ((item.TokenNo === null) ? "" : item.TokenNo) + '"/></td>\
                            <td style="text-align: center;padding:8px;">' + ((item.Quantity === null) ? "" : item.Quantity) + '</td>\
                            <td style= "text-align: right;padding:8px;" > ' + ((item.Freight === null) ? "" : item.Freight) + '</td>\
                            <td style="padding:1px;" ><input type="text" style="text-align: right;width: 100%;" value="' + ((item.ShortQty === null) ? "" : item.ShortQty) + '"/></td>\
                            <td style="padding:1px;"><input type="text" style="text-align: right;width: 100%;" value="' + ((item.ShortAmount === null) ? "" : item.ShortAmount) + '"/></td>\
                            <td style="text-align: center;padding:5px;">\
                                <a href="javascript:ViewHistory('+ item.SNo + ',' + item.Lorry + ',\'' + GetFormatedDate(item.InvoiceDate) + '\');">History</a></td>\
                            <td class="d-none">'+ item.SNo + '</td>\
                            <td class="d-none">'+ item.Lorry + '</td>\
                            </tr>');
                        table.row.add(row);
                    });
                    table.draw();
                }
                else {
                    ShowInformationDialog('Error', data.Message);
                }
            },
            complete: function () {
                $('.efi_ajax-loader').css("visibility", "hidden");
            },
            error: function (error) {
                alert("errror");
            }
        });
    });

    $('.efi_save_btn').on('click', function () {

        $('.efi_save_btn').prop('disabled', true);

        var error = false;
        var company = $(".efi_company_select").val();
        if (company === '' || company === null) {
            ShowInformationDialog('Error', "Company Missing");
            $('.efi_save_btn').prop('disabled', false);
            error = true;
        }
        var bill_date = $(".efi_bill_date_dp").val();
        if (bill_date === '' || bill_date === null) {
            ShowInformationDialog('Error', "Bill Date Missing");
            $('.efi_save_btn').prop('disabled', false);
            error = true;
        }
        var bill_no = document.getElementById('FileUpload').files[0].name;
        bill_no = bill_no.substring(0, bill_no.lastIndexOf('.'));
        if (bill_no === '' || bill_no === null) {
            ShowInformationDialog('Error', "Bill No Missing");
            $('.efi_save_btn').prop('disabled', false);
            error = true;
        }
        var tripList = [];
        $("#efi_table > tbody > tr").each(function (i, v) {
            var xyz = {};
            $(this).children('td').each(function (ii, vv) {
                if (ii === 0) {
                    xyz["TripId"] = $(this).text();
                }
                if (ii === 3) {
                    let date = $(this).text();
                    xyz["InvoiceDateDay"] = date.substring(0, 2);
                    xyz["InvoiceDateMonth"] = date.substring(3, 5);
                    xyz["InvoiceDateYear"] = date.substring(6, 10);
                }
                if (ii === 4) {
                    var $input1 = $(this).find('input[type="text"]');
                    xyz["TokenNo"] = $input1.val();
                }
                if (ii === 5) {
                    xyz["FuelQty"] = $(this).text();
                }
                if (ii === 6) {
                    xyz["Freight"] = parseFloat($(this).text());
                }
                if (ii === 7) {
                    var $input1 = $(this).find('input[type="text"]');
                    if ($input1.val() == "")
                    {
                        xyz["ShortQty"] = null;
                    }
                    else
                    {
                        xyz["ShortQty"] = $input1.val();
                    }
                }
                if (ii === 8) {
                    var $input1 = $(this).find('input[type="text"]');
                    if ($input1.val() == "") {
                        xyz["ShortAmount"] = null;
                    }
                    else {
                        xyz["ShortAmount"] = $input1.val();
                    }
                }
            });
            tripList.push(xyz);
        });

        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/EFI/SaveBill",
                beforeSend: function (xhr) {
                    $('.efi_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        CompanyId: company,
                        BillDate: bill_date,
                        BillNo: bill_no,
                        TripList: tripList
                    }
                ),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        ShowInformationDialog('Information', "Saved Successfully.");

                        $('.efi_pick_file_txt').val("");
                        $('.efi_company_select').val("");
                        $('.efi_bill_date_dp').val("");
                        $(".efi_table").DataTable().clear().draw();
                        $(".view_history_table").DataTable().clear().draw();
                        sno_for_trip_grid_in_view_history = 0;
                    }
                    else
                    {
                        ShowInformationDialog("Error", data.Message);
                    }
                },
                complete: function () {
                    $('.efi_save_btn').prop('disabled', false);
                    $('.efi_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $(document).on("change", "#history_grid_check", function () {

        var history_grid_check = $(this).is(':checked');
        var tripID = $(this).val();

        if (history_grid_check) {
            $.ajax({
                url: "/EFI/UpdateHistory",
                type: "POST",
                beforeSend: function (xhr) {
                    $('.efi_ajax-loader').css("visibility", "visible");
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        TripIDHistoryGrid: tripID,
                        SNoInTripGrid: sno_for_trip_grid_in_view_history
                    }
                ),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {

                        $("#efi_table > tbody > tr").each(function (i, v) {
                            var SNoInTable = 0;
                            $(this).children('td').each(function (ii, vv) {
                                if (ii === 10) {
                                    SNoInTable = $(this).text();
                                }
                            });
                            if (SNoInTable == sno_for_trip_grid_in_view_history) {
                                $(this).children('td').each(function (ii, vv) {
                                    if (ii === 0) {
                                        $(this).text(data.TripClass.TripId);
                                    }
                                    if (ii === 1) {
                                        $(this).text(GetFormatedDate(data.TripClass.EntryDate));
                                    }
                                    else if (ii === 10) {
                                        $(this).html('<a href="javascript:ViewHistory(' + data.TripClass.TripId + ');">History</a>');
                                    }
                                });
                                return false;
                            }
                        });

                        $('.view_history_form').dialog('close');
                    }
                    else {
                        ShowInformationDialog("Information", data.Message);
                    }

                },
                complete: function () {
                    $('.efi_ajax-loader').css("visibility", "hidden");
                }
            });
        }

    });

    $('.efi_pick_file_txt').on('change', function () {
        $('.efi_company_select').val("");
        $('.efi_bill_date_dp').val("");
        $(".efi_table").DataTable().clear().draw();
        
    });

    $('#view_history_form').bind('dialogclose', function (event, ui) {

        sno_for_trip_grid_in_view_history = 0;

        $(".view_history__table").DataTable().clear().draw();
    });
});

function PageLoaded() {
    $('#efi_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 10, 25, 50], ["All", 10, 25, 50]],
        selecte: true,
        paging: false,
        searching: false,
        "scrollY": "350px",
        "scrollCollapse": true
    });

    $('#view_history_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        selecte: true,
        bSort: false,
        'sDom': 't',
        "scrollY": "290px",
        "scrollCollapse": true

    });

    //Company List
    GetAccountsByGroup("PARTY", true, '.efi_company_select');
}

function RefreshRecord(id) {
    $.ajax({
        type: "POST",
        url: "/EFI/RefreshTripInGrid",
        beforeSend: function (xhr) {
            $('.efi_ajax-loader').css("visibility", "visible");
            xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
        },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({ JournalID: id }),
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message === "OK") {
                $("#efi_table > tbody > tr").each(function (i, v) {
                    var id2 = 0;
                    $(this).children('td').each(function (ii, vv) {
                        if (ii === 0) {
                            id2 = $(this).text();
                        }
                    });
                    if (id2 == id) {
                        $(this).children('td').each(function (ii, vv) {
                            if (ii === 0) {
                                $(this).text(data.TripClass.JournalID);
                            }
                            if (ii === 1) {
                                $(this).text(GetFormatedDate(data.TripClass.EntryDate));
                            }
                            else if (ii === 2) {
                                $(this).text(data.TripClass.Account);
                            }
                            else if (ii === 3) {
                                $(this).text(GetFormatedDate(data.TripClass.InvoiceDate));
                            }
                            else if (ii === 4) {
                                $(this).html('<input type="text" style="width: 100%;text-align: center;" value="' + ((data.TripClass.TokenNO === null) ? "" : data.TripClass.TokenNO) + '"/>');
                            }
                            else if (ii === 5) {
                                $(this).text(data.TripClass.DeliveryNumber);
                            }
                            else if (ii === 6) {
                                $(this).text(data.TripClass.FuelQty);
                            }
                            else if (ii === 7) {
                                $(this).text(data.TripClass.Freight);
                            }
                            else if (ii === 8) {
                                $(this).html('<input type="text" style="text-align: right;width: 100%;" value="' + ((data.TripClass.ShortQty === null) ? "" : data.TripClass.ShortQty) + '"/>');
                            }
                            else if (ii === 9) {
                                $(this).html('<input type="text" style="text-align: right;width: 100%;" value="' + ((data.TripClass.ShortAmount === null) ? "" : data.TripClass.ShortAmount) + '"/>');
                            }
                            else if (ii===10)
                            {
                                $(this).html('<a href="javascript:ViewHistory(' + data.TripClass.JournalID + ');">History</a>\
                                <a href="javascript:RefreshRecord('+ data.TripClass.JournalID + ');">Refresh</a>');
                            }
                        });
                        return false;
                    }
                });
            }
            else
            {
                ShowInformationDialog('Error', data.Message);
            }
        },
        complete: function () {
            $('.efi_ajax-loader').css("visibility", "hidden");
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });
}

var sno_for_trip_grid_in_view_history = 0;
function ViewHistory(SNo, Lorry, InvoiceDate) {
    sno_for_trip_grid_in_view_history = SNo;
    $("#view_history_form").dialog({
        title: "VIEW HISTORY",
        width: 600,
        height: 450,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            $.ajax({
                type: "POST",
                url: "/EFI/ViewHistory",
                beforeSend: function (xhr) {
                    $('.view_history_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        LorryAccountId: Lorry,
                        InvoiceDateString:InvoiceDate
                    }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message == "OK") {

                        $(".view_history_table").DataTable().clear().draw();
                        var table = $('.view_history_table').DataTable();

                        $.each(data.TripList, function (index, item) {
                            var row = $('<tr>\
                            <td style="text-align: center;padding:8px;"><input id="history_grid_check" value="'+ item.TripId + '" type="checkbox"/></td>\
                            <td style="text-align: center;padding:8px;">' + item.TripId + '</td>\
                            <td style="text-align: center;padding:8px;">' + ((item.EntryDate === null) ? "" : GetFormatedDate(item.EntryDate)) + '</td>\
                            <td style= "text-align: center;padding:8px;" > ' + ((item.LorryTitle === null) ? "" : item.LorryTitle) + '</td>\
                            </tr>');
                            table.row.add(row);
                        });
                        table.draw();
                    }
                    else
                    {

                        ShowInformationDialog('Error', data.Message);

                    }
                },
                complete: function () {
                    $('.view_history_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

}
    

