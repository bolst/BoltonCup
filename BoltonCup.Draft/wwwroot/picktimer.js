function picktimer(countdownSeconds) {
    var timer, days, hours, minutes, seconds;

    const radius = 57;
    const circumference = 2 * Math.PI * radius;

    dateEnd = new Date();
    dateEnd = new Date(dateEnd.getUTCFullYear(),
        dateEnd.getUTCMonth(),
        dateEnd.getUTCDate(),
        dateEnd.getUTCHours(),
        dateEnd.getUTCMinutes(),
        dateEnd.getUTCSeconds() + countdownSeconds);
    dateEnd = dateEnd.getTime();

    if (isNaN(dateEnd)) return;

    const timerElem = document.getElementById("timersec");

    if (timerElem) {
        timerElem.innerHTML = `
            <svg width="120" height="120">
                <circle
                    r="${radius}"
                    cx="60"
                    cy="60"
                    fill="transparent"
                    stroke="#FF0000"
                    stroke-width="6"
                    stroke-linecap="round"
                    stroke-dasharray="${circumference}"
                    stroke-dashoffset="0"
                />
                <text
                    id="timer-text"
                    x="50%"
                    y="50%"
                    dominant-baseline="middle"
                    text-anchor="middle"
                    font-size="2rem"
                    fill="#FF0000"
                >
            
                </text>
            </svg>
        `;
    }

    const circle = timerElem.querySelector("circle");
    const text = timerElem.querySelector("#timer-text");

    circle.style.transition = "stroke-dashoffset 1s linear";

    timer = setInterval(calculate);

    function calculate() {
        var dateStart = new Date();
        dateStart = new Date(dateStart.getUTCFullYear(),
            dateStart.getUTCMonth(),
            dateStart.getUTCDate(),
            dateStart.getUTCHours(),
            dateStart.getUTCMinutes(),
            dateStart.getUTCSeconds());

        var timeRemaining = parseInt((dateEnd - dateStart.getTime()) / 1000);

        if (timeRemaining >= 0) {
            days = parseInt(timeRemaining / 86400);
            timeRemaining %= 86400;
            hours = parseInt(timeRemaining / 3600);
            timeRemaining %= 3600;
            minutes = parseInt(timeRemaining / 60);
            timeRemaining %= 60;
            seconds = parseInt(timeRemaining);

            text.textContent = ("0" + seconds).slice(-2);



            const progress = timeRemaining / countdownSeconds;
            const offset = circumference * (1 - progress);
            circle.style.strokeDashoffset = offset;
        } else {
            clearInterval(timer);
        }
    }
}
