$(document).ready(function () {
    var sessionLockPerMinutes = lockSession.RemainingSessionEditAllow.Minutes;
    var sessionLockPerSeconds = lockSession.RemainingSessionEditAllow.Seconds;
    var countDownDate = convertTimeToSeconds('00:' + sessionLockPerMinutes + ':' + sessionLockPerSeconds);
    displayEditingSession();

    function displayEditingSession() {
        if (isCurrentUserLock) {
            $('.ctc-session-notify').hide();
            $('.ctc-session-notify').addClass('hidden');
            $('.ctc-session-timeout').show();
            $('.ctc-session-timeout').removeClass('hidden');
            if (isEditor) {
                enableAllCtcInputTags();
            }
            else {
                disableAllCtcInputTags();
            }

            // Update the count down every 1 second
            var x = setInterval(function () {
                countDownDate--;
                // Time calculations for minutes and seconds
                var minutes = Math.floor(countDownDate / 60);
                var seconds = Math.floor(countDownDate % 60);
                var text = '';
                if (minutes > 1) {
                    text += minutes + ' minutes ';
                }
                else {
                    text += minutes + ' minute ';
                }
                if (seconds > 1) {
                    text += seconds + ' seconds.';
                }
                else {
                    text += seconds + ' second.';
                }

                $(".ctc-session-timeout-countdown").text(text);

                // If the count down is finished, write some text 
                if (countDownDate <= 0) {
                    clearInterval(x);
                    $('.ctc-session-timeout-time-left').hide();
                    $('.ctc-session-timeout-countdown').hide();
                    $('.ctc-session-timeout .message').show();
                    $('.ctc-session-timeout .btn').show();

                    $('.ctc-session-timeout-time-left').addClass('hidden');
                    $('.ctc-session-timeout-countdown').addClass('hidden');
                    $('.ctc-session-timeout .message').removeClass('hidden');
                    $('.ctc-session-timeout .btn').removeClass('hidden');
                    disableAllCtcInputTags();
                }
                else {
                    $('.ctc-session-timeout-time-left').show();
                    $('.ctc-session-timeout-countdown').show();
                    $('.ctc-session-timeout .message').hide();
                    $('.ctc-session-timeout .btn').hide();

                    $('.ctc-session-timeout-time-left').removeClass('hidden');
                    $('.ctc-session-timeout-countdown').removeClass('hidden');
                    $('.ctc-session-timeout .message').addClass('hidden');
                    $('.ctc-session-timeout .btn').addClass('hidden');
                }
            }, 1000);
        }
        else {
            $('.ctc-session-notify').show();
            $('.ctc-session-notify').removeClass('hidden');
            $('.ctc-session-notify p').text('This case is being locked by ' + lockSession.LockedBy + '.');
            $('.ctc-session-notify').show();
            $('.ctc-session-notify').removeClass('hidden');
            disableAllCtcInputTags();
        }

    }
    function disableAllCtcInputTags() {
        $('#collapse .form-control.pop-up-input').not('.disabled-by-system').each(function () {
            $(this).attr('disabled', true);
            $(this).addClass("cto-audited-account-expire-disabled");
        });
        $('.ctc-bottom-button input').each(function() {
            $(this).attr('disabled', true);
            $(this).addClass("cto-audited-account-expire-disabled");
        });
        $('.k-multiselect').attr('disabled', true);
        $('.k-multiselect input').attr('disabled', true);
    }
    function enableAllCtcInputTags() {
        $('#collapse .cto-audited-account-expire-disabled').not('.disabled-by-system').each(function () {
            $(this).removeAttr('disabled');
            $(this).removeClass("cto-audited-account-expire-disabled");
        });
        $('.ctc-bottom-button input').each(function () {
            $(this).attr('disabled', false);
            $(this).removeClass("cto-audited-account-expire-disabled");
        });
        $('.k-multiselect').attr('disabled', false);
        $('.k-multiselect input').attr('disabled', false);
    }
    function convertTimeToSeconds(str) {
        var a = str.split(':'); // split it at the colons

        // minutes are worth 60 seconds. Hours are worth 60 minutes.
        return (+a[0]) * 60 * 60 + (+a[1]) * 60 + (+a[2]);
    }
    $(document).on('click', '.ctc-session-timeout .btn', function () {
        if (isCurrentUserLock) {
            var uen = getUrlParameter("uen");
            var year = getUrlParameter("year");
            var entityId = getUrlParameter("entityId");
            $.ajax({
                type: 'GET',
                url: extendSessionUrl + "?uen=" + uen + "&year=" + year + "&entityId=" + entityId,
                success: function (reponse) {
                    if (reponse.Success && reponse.Data) {
                        lockSession = reponse.Data;
                        sessionLockPerMinutes = lockSession.RemainingSessionEditAllow.Minutes;
                        sessionLockPerSeconds = lockSession.RemainingSessionEditAllow.Seconds;
                        countDownDate = convertTimeToSeconds('00:' + sessionLockPerMinutes + ':' + sessionLockPerSeconds);
                        displayEditingSession();

                    }
                    else {
                        window.alert(reponse.Message);
                    }
                },
                error: function (e) {

                }
            });
        }
    });
    function getUrlParameter(sParam) {
        if (sParam === null || sParam === "")
            return 0;
        var sPageURL = window.location.search.substring(1);
        var sURLVariables = sPageURL.split('&');
        for (var i = 0; i < sURLVariables.length; i++) {
            var sParameterName = sURLVariables[i].split('=');
            if (sParameterName[0].toLowerCase() === sParam.toLowerCase()) {
                return sParameterName[1];
            }
        }
        return 0;
    }
});
