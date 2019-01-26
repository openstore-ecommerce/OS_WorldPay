

$(document).ready(function () {

    $('#OS_WorldPay_cmdSave').unbind("click");
    $('#OS_WorldPay_cmdSave').click(function () {
        $('.processing').show();
        $('.actionbuttonwrapper').hide();
        // lower case cmd must match ajax provider ref.
        nbxget('os_worldpay_savesettings', '.OS_WorldPaydata', '.OS_WorldPayreturnmsg');
    });

    $('.selectlang').unbind("click");
    $(".selectlang").click(function () {
        $('.editlanguage').hide();
        $('.actionbuttonwrapper').hide();
        $('.processing').show();
        $("#nextlang").val($(this).attr("editlang"));
        // lower case cmd must match ajax provider ref.
        nbxget('os_worldpay_selectlang', '.OS_WorldPaydata', '.OS_WorldPaydata');
    });


    $(document).on("nbxgetcompleted", OS_WorldPay_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function OS_WorldPay_nbxgetCompleted(e) {

        $('.processing').hide();
        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        if (e.cmd === 'os_worldpay_selectlang') {
            alert('why????');   
        }

    };

});

