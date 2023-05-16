$(document).ready(function () {

    $('.user_new_btn').on('click', function () {

        $("#new_user_form").dialog({
            title: "New User",
            width: 500,
            height: 210,
            open: function () {
                // On open, hide the original submit button
                $(this).find("[type=submit]").hide();
            }
        });

    });

    $('.new_user_save_btn').on('click', function () {

        $('.new_user_save_btn').prop('disabled', true);

        var error = false;
        var name = $('.new_user_name_txt').val();
        if (name === '' || name === null) {
            ShowInformationDialog('Error', "User Name Missing");
            $('.new_user_save_btn').prop('disabled', false);
            error = true;
        }
        var email = $('.new_user_email_txt').val();
        if (email === '' || email === null) {
            ShowInformationDialog('Error', "Email Missing");
            $('.new_user_save_btn').prop('disabled', false);
            error = true;
        }
        var display_name = $('.new_display_name_txt').val();
        if (display_name === '' || display_name === null) {
            ShowInformationDialog('Error', "Display Name Missing");
            $('.new_user_save_btn').prop('disabled', false);
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/User/SaveUser",
                beforeSend: function (xhr) {
                    $('.new_user_save_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        UserName: name,
                        Email: email,
                        DisplayName: display_name

                    }
                ),
                success: function (response) {

                    var data = JSON.parse(response);

                    ShowInformationDialog('Information', data.Message);
                    if (data.Message === "Saved Successfully") {

                        PopulateUserTable(data.UserList);
                        $('.new_user_form').dialog('close');
                    }
                },
                complete: function () {
                    $('.new_user_save_btn').prop('disabled', false);
                    $('.new_user_save_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('#new_user_form').bind('dialogclose', function (event, ui) {

        $('.new_user_save_btn').prop('disabled', false);
        $(".new_user_name_txt").val("");
        $(".new_user_password_txt").val("");
        $(".new_user_confirm_password_txt").val("");
        $(".new_user_email_txt").val("");
        $(".new_user_phone_no_txt").val("");
    });
    
});
