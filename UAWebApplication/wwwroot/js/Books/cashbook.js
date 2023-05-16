
$(document).ready(function () {

    $('#kharch_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        /*'sDom': 't',*/
        "bSort": false,
        "scrollY": "320px",
        "scrollCollapse": true
    });
    $('#amdan_table').dataTable({
        "pagingType": "full_numbers",
        "lengthMenu": [[-1, 25, 50, 10], ["All", 25, 50, 10]],
        /*'sDom': 't',*/
        "bSort": false,
        "scrollY": "320px",
        "scrollCollapse": true
    });

    $(".cash_book_view_btn").on('click', function () {
        ViewDayBook();
    });

    $.contextMenu({
        selector: '#amdan_table tr',
        trigger: 'right',
        callback: async function (key, options) {
            var row = $("#amdan_table").DataTable().row(options.$trigger);
            switch (key) {
                case 'delete':
                    DeleteTransaction(row.data()[4], "Cash Book");
                    ViewDayBook();
                    //row.remove().draw();
                    break;
                case 'update':
                    UpdateTransaction(row.data()[4], row.data()[5]);
                    ViewDayBook();
                    break;
                default:
                    break
            }
        },
        items: {
            "update": { name: "Update", icon: "edit" },
            "delete": { name: "Delete", icon: "delete" }
        }
    }) 
    $.contextMenu({
        selector: '#kharch_table tr',
        trigger: 'right',
        callback: async function (key, options) {
            var row = $("#kharch_table").DataTable().row(options.$trigger);
            switch (key) {
                case 'delete':
                    DeleteTransaction(row.data()[3], "Cash Book");
                    ViewDayBook();
                    //row.remove().draw();
                    break;
                case 'update':
                    UpdateTransaction(row.data()[3], row.data()[4]);
                    ViewDayBook();
                    break;
                default:
                    break
            }
        },
        items: {
            "update": { name: "Update", icon: "edit" },
            "delete": { name: "Delete", icon: "delete" }
        }
    }) 

    $('.cash_book_trip_btn').on('click', function () {
        NewTripWindowLoaded(0);

    });

    $('.cash_book_transaction_btn').on('click', function () {
        NewTransactionWindowLoaded(0);
    });

});

function ViewDayBook() {
    var error = false;
    var entry_date = $(".cash_book_entry_date_dp").val();
    if (entry_date === '' || entry_date === null) {
        ShowInformationDialog('Error', "Date Missing");
        error = true;
    }
    if (error === false) {
        $.ajax({
            type: "POST",
            url: "/CashBook/SearchByEntryDate",
            beforeSend: function (xhr) {
                $('.cash_book_view_ajax-loader').css("visibility", "visible");
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ SearchDate: entry_date }),
            success: function (response) {
                var record = JSON.parse(response);

                $(".amdan_table").DataTable().clear().draw();
                $(".kharch_table").DataTable().clear().draw();
                var amdan_table = $(".amdan_table").DataTable();
                var kharch_table = $(".kharch_table").DataTable();
                if (record.Message == "OK") {
                    $.each(record.DbList, function (index, item) {

                        if (item.Type === "AMDAN") {
                            if (item.Description === "Cash B/F : " || item.Description === "Total Amdan : " ||
                                item.Description === "Total Kharch : " || item.Description === "Cash C/F : ") {
                                var row = $('<tr style="background-color: #eee;font-weight: bold;">\
                                    <td style="text-align: right;padding:3px;font-size:15px;"></td>\
                                    <td style="text-align: center;padding:3px;"></td>\
                                    <td style="text-align: right;padding:3px;">'+ item.Description +'</td>\
                                    <td style="text-align: right;padding:2px;font-size:15px;" >' + item.Amount + '</td>\
                                    <td class="d-none"></td>\
                                    <td class="d-none"></td>\
                                    </tr>');
                                amdan_table.row.add(row);
                            }
                            else
                            {
                                var row2 = $('<tr><td style="text-align: left;padding:3px;">' + item.AccountTitle + '</td>\
                                <td style="text-align: center;padding:3px;" > ' + ((item.ChequeNo === null) ? "" : item.ChequeNo) + '</td>\
                                <td style="text-align: right;padding:3px;" > ' + ((item.Description === null) ? "" : item.Description) + '</td>\
                                <td style="text-align: right;padding:3px;" >' + item.Amount + '</td>\
                                <td class="d-none">' + item.TransId + '</td>\
                                <td class="d-none">' + item.TripId + '</td>\
                                </tr>');
                                amdan_table.row.add(row2);
                            }
                        }
                        else if (item.Type === "KHARCH") {
                            if (item.Description === "Total Kharch  ") {
                                var row3 = $('<tr style="background-color: #eee;font-size:15px;font-weight: bold;">\
                                    <td style="text-align: right;padding:3px;"></td>\
                                    <td style="text-align: right;padding:3px;">' + item.Description + '</td>\
                                    <td style="text-align: right;padding:3px;" >' + item.Amount + '</td>\
                                    <td class="d-none"></td>\
                                    <td class="d-none"></td>\
                                    </tr>');
                                kharch_table.row.add(row3);
                            }
                            else {
                                var row4 = $('<tr><td style="text-align: left;padding:3px;">' + item.AccountTitle + '</td>\
                                <td style="text-align: right;padding:3px;" > ' + ((item.Description === null) ? "" : item.Description) + '</td>\
                                <td style="text-align: right;padding:3px;" >' + item.Amount + '</td>\
                                <td class="d-none">' + item.TransId + '</td>\
                                <td class="d-none">' + item.TripId + '</td>\
                                </tr>');
                                kharch_table.row.add(row4);
                            }
                        }

                    });
                    amdan_table.draw();
                    kharch_table.draw();
                }
                else
                {
                    ShowInformationDialog('Information', record.Message);
                }
            },
            complete: function () {
                $('.cash_book_view_ajax-loader').css("visibility", "hidden");
            },
            failure: function (response) {
                alert('Some thing wrong');
            }
        });
    }
}
