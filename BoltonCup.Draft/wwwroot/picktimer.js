function picktimer(countdownSeconds) {
    var timer, days, hours, minutes, seconds;

    dateEnd = new Date();
    dateEnd = new Date(dateEnd.getUTCFullYear(),
        dateEnd.getUTCMonth(),
        dateEnd.getUTCDate(),
        dateEnd.getUTCHours(),
        dateEnd.getUTCMinutes(),
        dateEnd.getUTCSeconds() + countdownSeconds);
    dateEnd = dateEnd.getTime();

    if (isNaN(dateEnd)) {
        return;
    }

    timer = setInterval(calculate);

    function calculate() {
        var dateStart = new Date();
        var dateStart = new Date(dateStart.getUTCFullYear(),
            dateStart.getUTCMonth(),
            dateStart.getUTCDate(),
            dateStart.getUTCHours(),
            dateStart.getUTCMinutes(),
            dateStart.getUTCSeconds());
        var timeRemaining = parseInt((dateEnd - dateStart.getTime()) / 1000)

        if (timeRemaining >= 0) {
            days = parseInt(timeRemaining / 86400);
            timeRemaining = (timeRemaining % 86400);
            hours = parseInt(timeRemaining / 3600);
            timeRemaining = (timeRemaining % 3600);
            minutes = parseInt(timeRemaining / 60);
            timeRemaining = (timeRemaining % 60);
            seconds = parseInt(timeRemaining);

            document.getElementById("timersec").innerHTML = ("0" + seconds).slice(-2);
        } else {
            return;
        }
    }
}
