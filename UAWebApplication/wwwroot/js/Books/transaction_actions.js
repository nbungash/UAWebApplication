
$(document).ready(function () {

    

});

function UpdateTransaction(transaction_id, trip_id) {
    try {
        if (trip_id == "null")
        {
            NewTransactionWindowLoaded(transaction_id);
        }
        else
        {
            NewTripWindowLoaded(trip_id);
        }
    } catch (e) {
        ShowInformationDialog("Error",e); // Logs the error
    }
}

function DeleteTransaction(transaction_id,source) {
    var question = "Are You Sure You want to Delete ?";
    confirmation(question).then(function (answer) {
        var ansbool = (String(answer) == "true");
        if (ansbool) {
            $.ajax({
                type: "POST",
                url: "/GeneralJournal/DeleteTransaction",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN", $('input:hidden[name="__RequestVerificationToken"]').val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ TransactionId: transaction_id }),
                success: function (response) {
                    var data = JSON.parse(response);
                    if (data.Message === "OK") {
                        ShowInformationDialog("Information", "Deleted Successfully.");
                        if (source == "General Journal") {
                            var table = $('#j_table1').DataTable();
                            var filteredData = table.rows().indexes().filter(function (value, index) {
                                return table.row(value).data()[0] == transaction_id;
                            });
                            table.rows(filteredData).remove().draw();
                        }
                        else if (source == "Cash Book") {
                            ViewDayBook();
                        }
                    }
                    else {
                        ShowInformationDialog('Error', data.Message);
                    }
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });
}
