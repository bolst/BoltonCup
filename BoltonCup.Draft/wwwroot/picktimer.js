
var timerObj = null;

function startTimer(countdownSeconds) {
    if (timerObj != null) { timerObj.pause(); }
    
    timerObj = initTimer(countdownSeconds);
}

function pauseTimer() {
    if (timerObj != null) {
        timerObj.pause();   
    }
}

function resumeTimer() {
    if (timerObj != null) {
        timerObj.resume();
    }
}

function initTimer(countdownSeconds) {
    pause = true;
    
    var timer;

    const radius = 57;
    const circumference = 2 * Math.PI * radius;

    let startTime;
    dateEnd = new Date();
    dateEnd.setSeconds(dateEnd.getSeconds() + countdownSeconds);
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
                    stroke-dashoffset="0" />
                <text
                    id="timer-text"
                    x="50%"
                    y="50%"
                    dominant-baseline="middle"
                    text-anchor="middle"
                    font-size="2rem"
                    fill="#FF0000">
                </text>
            </svg>
        `;
    }

    const circle = timerElem.querySelector("circle");
    const text = timerElem.querySelector("#timer-text");

    circle.style.transition = "stroke-dashoffset 1s linear";

    var dt_ms = countdownSeconds * 1000;
    var now = 0;
    function calculate() {
        now = dt_ms - (new Date().getTime() - startTime);
        let secRemaining = Math.floor(now / 1000);

        if (secRemaining >= 0) {
            console.log(`secRemaining: ${secRemaining}`);

            text.textContent = secRemaining.toString();

            const progress = secRemaining / countdownSeconds;
            const offset = circumference * (1 - progress);
            circle.style.strokeDashoffset = offset;
        } else {
            clearInterval(timer);
        }
    }

    retval = {};
    retval.resume = function() {
        startTime = new Date().getTime();
        timer = setInterval(calculate, 150);
    }

    retval.pause = function() {
        dt_ms = now;
        clearInterval(timer);
    }
    
    retval.resume();
    return retval;
}
