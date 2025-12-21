var isPromptVisible = true;
let keyPressTimestamps = [];
let userInput = '';

window.addEventListener('load', () => {
    const commandField = document.getElementById('command-field');
    commandField.addEventListener('keydown', handleInputKeys);
    commandField.addEventListener('selectionchange', e => {
        setCaretToEnd(commandField);
    });
    commandField.focus();
    recordKeyPressTime();
});

function makePromptVanish() {
    const prompt = document.getElementById('prompt');
    prompt.classList.add('hidden');
    prompt.addEventListener('transitionend', () => { prompt.style.display = 'none'; }, { once: true });
    isPromptVisible = false;
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
            userInput += '\b';
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
        if (isPromptVisible) makePromptVanish();
        userInput += event.key;
        recordKeyPressTime();
        return;
    }
    console.warn('modified key', event.key);
}

function recordKeyPressTime() {
    keyPressTimestamps.push(Date.now());
}

function handleEntryEnter(inputField) {    
    // Clear timestamps if no text was entered
    if (userInput.trim() === '') {
        userInput = "";
        inputField.value = "";
        keyPressTimestamps = [];
        return;
    }

    // Calculate intervals from timestamps
    const timestamps = keyPressTimestamps;
    timestamps.push(new Date());
    const intervals = [];
    for (let i = 1; i < timestamps.length; i++) {
        intervals.push((timestamps[i] - timestamps[i-1]) / 1000);
    }

    const object = {
        "text": userInput + '\n',
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
        userInput = '';
        keyPressTimestamps = [new Date()];
    })
    .catch(error => {
        console.error('Error sending command:', error);
    });
}