var IS_PROMPT_VISIBLE = true;
let KEY_PRESS_TIMESTAMPS = [];
let USER_INPUT = '';
let ENTRIES = []
const PROMPT_MESSAGE = "Say something good about today :)";
const PROMPT_DELAYS = [0.509,0.24,0.09,0.08,0.185,0.106,0.044,0.05,0.159,0.036,0.094,0.132,0.001,0.095,0.171,0.116,0.135,0.069,0.113,0.111,0.133,0.086,0.122,0.033,0.081,0.12,0.084,0.111,0.107,0.052,0.499,0.13,0.57]; //,1.057];

window.addEventListener('load', () => {
    autoTypePrompt(PROMPT_MESSAGE, PROMPT_DELAYS);
    const commandField = document.getElementById('command-field');
    commandField.addEventListener('keydown', handleInputKeys);
    commandField.addEventListener('selectionchange', e => {
        setCaretToEnd(commandField);
    });
    commandField.focus();
    recordKeyPressTime();

    fetchAllEntries();
});

function sleep(s) {
    return new Promise(resolve => setTimeout(resolve, s * 250));
}

function entryToString(entry) { 
    var string = "";
    for (let i = 0; i < entry.length; i++) {
        if (entry[i] == '\b') {
            string = string.slice(0, string.length - 1);
        } else {
            string += entry[i];
        }
    }
    return string;
}

async function autoTypePrompt(promptMsg, intervals) {
    const prompt = document.getElementById('prompt');
    prompt.innerHTML = "";
    for (let i = 0; i < promptMsg.length; i++) {
        await sleep(intervals[i]);
        prompt.innerHTML += promptMsg[i];
    }
}

function makePromptVanish() {
    const prompt = document.getElementById('prompt');
    prompt.classList.add('hidden');
    prompt.addEventListener('transitionend', () => { prompt.style.display = 'none'; }, { once: true });
    IS_PROMPT_VISIBLE = false;
}

// Function to set caret to the end
function setCaretToEnd(el) {
    if (typeof el.selectionStart == "number") {
        el.selectionStart = el.selectionEnd = el.value.length;
        el.focus();
    } else if (typeof el.createTextRange != "undefined") { // to deal with old-school browsers
        el.focus();
        var range = el.createTextRange();
        range.collapse(false);
        range.select();
    }
}

function handleInputKeys(event) {
    const commandField = document.getElementById('command-field');

    // Handle Ctrl + Backspace
    if (event.key === 'Backspace' && event.ctrlKey) {
        event.preventDefault();
        return;
    }

    // Handle Backspace
    if (event.key === 'Backspace') {
        if (commandField.value.length > 0) {
            USER_INPUT += '\b';
            recordKeyPressTime();
            return;
        }
    }

    // Handle Enter key press
    if (event.key === 'Enter') {
        event.preventDefault();
        handleEntryEnter(commandField);
        return;
    }

    // Prevent cursor movement for specific keys
    const preventKeys = ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End', 'Delete', 'Tab'];
    if (preventKeys.includes(event.key)) {
        event.preventDefault();
        return;
    }

    // Record timestamp for any non-modified character key press
    if (event.key.length === 1) {
        if (IS_PROMPT_VISIBLE) makePromptVanish();
        USER_INPUT += event.key;
        recordKeyPressTime();
        return;
    }
    console.warn('modified key', event.key);
}

function recordKeyPressTime() {
    KEY_PRESS_TIMESTAMPS.push(Date.now());
}

function handleEntryEnter(inputField) {    
    // Clear timestamps if no text was entered
    if (USER_INPUT.trim() === '') {
        USER_INPUT = "";
        inputField.value = "";
        KEY_PRESS_TIMESTAMPS = [];
        return;
    }

    // Calculate intervals from timestamps
    const timestamps = KEY_PRESS_TIMESTAMPS;
    timestamps.push(new Date());
    const intervals = [];
    for (let i = 1; i < timestamps.length; i++) {
        intervals.push((timestamps[i] - timestamps[i-1]) / 1000);
    }

    const object = {
        "text": USER_INPUT + '\n',
        "intervals": intervals,
        "time": new Date().toISOString(),
        "printDate": false
    };

    fetch("/api/entry", {
        method: 'POST',
        body: JSON.stringify(object),
        headers: { 'Content-Type': 'application/json' },
    })
    .then(response => {
        if (response.ok) { return response.json(); }
        throw new Error(`HTTP error! status: ${response.status}`);
    })
    .then(data => {
        inputField.value = '';
        USER_INPUT = '';
        KEY_PRESS_TIMESTAMPS = [new Date()];
    })
    .catch(error => {
        console.error('Error sending command', error);
    });
}

async function fetchAllEntries() {
    // TODO: optimize with pagination
    console.log("getting all entries");
    fetch("/api/entry/all", { method: 'GET' })
    .then(response => {
        if (response.ok) { return response.json(); }
        throw new Error(`HTTP error! status: ${response.status}`);
    })
    .then(data => {
        console.info("get all response", data);
        data.forEach(element => {
            element.string = entryToString(element.text);
        });
        ENTRIES = data;
    })
    .catch(error => {
        console.error("Error getting all entries", error);
    })
}