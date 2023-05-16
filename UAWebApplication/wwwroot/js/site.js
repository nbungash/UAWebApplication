
$(document).ready(function () {

    var bootstrapButton = $.fn.button.noConflict();
    $.fn.bootstrapBtn = bootstrapButton;

    ShowThemes();

    $('.info_btn').on('click', function () {

        $('.info_form').dialog('close');
    });

    $('#info_form').bind('dialogclose', function (event, ui) {

        $('info_message_lbl').text("");

    });

});


function ShowInformationDialog(title, message) {

    $('#info_form').dialog({
        title: title,
        resizable: false,
        height: "auto",
        width: 450,
        open: function () {

            if (title == "Information") {
                $('#info_image').attr('src', '/images/info1.png');
            }
            else if (title == "Error") {
                $('#info_image').attr('src', '/images/error1.png');
            }

            $("#info_message_lbl").empty();
            $("#info_message_lbl").append(message);
        }
    });
}

function confirmation(question) {
    var defer = $.Deferred();
    $('<div></div>').html(question).dialog({
        autoOpen: true,
        title: 'Confirmation',
        height: 200,
        width: 400,
        buttons: {
            "Yes": function () {
                defer.resolve("true");//this text 'true' can be anything. But for this usage, it should be true or false.
                $(this).dialog("close");
            },
            "No": function () {
                defer.resolve("false");//this text 'false' can be anything. But for this usage, it should be true or false.
                $(this).dialog("close");
            }
        },
        close: function () {
            $(this).remove();
        }
    });
    return defer.promise();
}

function GetFormatedDate(dateObject) {
    var d = new Date(dateObject);
    var day = d.getDate();
    var month = d.getMonth() + 1;
    var year = d.getFullYear();
    if (day < 10) {
        day = "0" + day;
    }
    if (month < 10) {
        month = "0" + month;
    }
    var date = day + "-" + month + "-" + year;

    return date;
}

function GetFormatedDate2(dateObject) {
    if (dateObject != null) {
        var d = new Date(dateObject);
        var day = d.getDate();
        var month = d.getMonth() + 1;
        var year = d.getFullYear();
        if (day < 10) {
            day = "0" + day;
        }
        if (month < 10) {
            month = "0" + month;
        }
        var date = year + "-" + month + "-" + day;

        return date;
    }
    else {
        return null;
    }
}

function ShowThemes() {

    $.ajax({
        type: "GET",
        url: "/User/ShowUserTheme",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {

            var data = JSON.parse(response);
            if (data.Message === "OK") {

                $("#stylesheet").attr('href', '/lib/jquery-ui-themes-1.13.2/themes/' + data.UserTheme + '/jquery-ui.min.css');
            }
        },
        failure: function (response) {
            alert('Some thing wrong');
        }
    });



}

function AccountGroupList(selectId) {
    $.ajax({
        type: "GET",
        url: "/ChartOfAccount/AccountGroupList",
        beforeSend: function (xhr) { },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $(selectId).empty();
                $(selectId).append($("<option />").text("Select..."));
                $.each(data.GroupList, function (index, item) {
                    $(selectId).append($("<option />").text(item));
                });
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

function GetAccountsByGroup(Group, IsActive, selectId) {
    $.ajax({
        type: "POST",
        url: "/ChartOfAccount/AccountsByGroupList",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(
            {
                GroupType: Group,
                IsActive: IsActive
            }),
        success: function (response) {
            var data = JSON.parse(response);
            if (data.Message == "OK") {
                $(selectId).empty();
                $(selectId).append($("<option />").val(0).text("Select..."));
                $.each(data.AccountList, function (index, item) {
                    $(selectId).append($("<option />").val(item.AccountId).text(item.Title));
                });
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
