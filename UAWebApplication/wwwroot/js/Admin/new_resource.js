
$(document).ready(function () {

    $('.page_new_page_btn').on('click', function () {
        OpenPage();
    });

    $('.new_page_save_btn').on('click', function () {

        $('.new_page_save_btn').prop('disabled', true);

        var error = false;
        var title = $(".new_page_title_txt").val();
        if (title === '' || title === null) {
            ShowInformationDialog('Error', "Title Missing");
            $('.new_page_save_btn').prop('disabled', false);
            error = true;
        }
        if (error === false) {
            $.ajax({
                type: "POST",
                url: "/Resource/SaveResource",
                beforeSend: function (xhr) {
                    $('.new_page_save_ajax-loader').css("visibility", "visible");
                    xhr.setRequestHeader("XSRF-TOKEN", $(".AntiForge" + " input").val());
                },
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(
                    {
                        Title: title
                    }
                ),
                success: function (response) {
                    
                    var data = JSON.parse(response);

                    ShowInformationDialog('Information', data.Message);

                    if (data.Message === "Saved Successfully")
                    {
                        $('.new_page_form').dialog('close');

                        PopulateResourceTable(data.ResourceList);

                    }
                },
                complete: function () {
                    $('.new_page_save_btn').prop('disabled', false);
                    $('.new_page_save_ajax-loader').css("visibility", "hidden");
                },
                failure: function (response) {
                    alert('Some thing wrong');
                }
            });
        }
    });

    $('#new_page_form').bind('dialogclose', function (event, ui) {

        $('.new_page_save_btn').prop('disabled', false);
        $(".new_page_title_txt").val("");
    });

    
});

function OpenPage()
{
    $("#new_page_form").dialog({
        title: "New Resource",
        width: 500,
        height: 200,
        open: function () {
            // On open, hide the original submit button
            $(this).find("[type=submit]").hide();
            
        }
    });
}
