export function initCountdown(dateEnd) {
    let days, hours, minutes, seconds;

    dateEnd = new Date(dateEnd).getTime();

    if (isNaN(dateEnd)) {
        return;
    }

    const timerId = setInterval(calculate, 1000);

    function calculate() {
        let dateStart = new Date();
        dateStart = new Date(dateStart.getUTCFullYear(),
            dateStart.getUTCMonth(),
            dateStart.getUTCDate(),
            dateStart.getUTCHours(),
            dateStart.getUTCMinutes(),
            dateStart.getUTCSeconds());
        let timeRemaining = parseInt((dateEnd - dateStart.getTime()) / 1000)

        if (timeRemaining >= 0) {
            days = parseInt(timeRemaining / 86400);
            timeRemaining = (timeRemaining % 86400);
            hours = parseInt(timeRemaining / 3600);
            timeRemaining = (timeRemaining % 3600);
            minutes = parseInt(timeRemaining / 60);
            timeRemaining = (timeRemaining % 60);
            seconds = parseInt(timeRemaining);

            const daySlice = days >= 100 ? -3 : -2;
            
            // make sure element exists before updating
            const daysElem = document.getElementById("days");
            if (daysElem) {
                daysElem.innerHTML = ("0" + days).slice(daySlice);
                document.getElementById("hours").innerHTML = ("0" + hours).slice(-2);
                document.getElementById("minutes").innerHTML = ("0" + minutes).slice(-2);
                document.getElementById("seconds").innerHTML = ("0" + seconds).slice(-2);
            } else {
                clearInterval(timerId);
            }
        }
    }
    
    return timerId;
}

export function stopCountdown(timerId) {
    if (timerId) {
        clearInterval(timerId);
    }
}