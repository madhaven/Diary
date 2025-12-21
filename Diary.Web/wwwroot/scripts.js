var isPromptVisible = true;

window.addEventListener('load', () => {
    const commandField = document.getElementById('command-field');
    commandField.addEventListener('keydown', handleInputKeys);
    commandField.addEventListener('selectionchange', e => {
        setCaretToEnd(commandField);
    });
    commandField.focus();
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
    const commandField = document.getElementById('command-field'); // Get commandField inside handler
    if (isPromptVisible) makePromptVanish();

    // Prevent Ctrl + Backspace from deleting whole words
    if (event.key === 'Backspace' && event.ctrlKey) {
        event.preventDefault();
        return;
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
    }
}

function handleEntryEnter(inputField) {
    const inputText = inputField.value;
    if (inputText.trim() === '') { return; }

    console.log('Saving entry:', inputText);
    const object = {
        "text": inputText,
        "intervals": [],
        "time": new Date().toISOString(), // ISO format: 2025-12-21T14:35:59.455Z
        "printDate": false
    }
    const jsonString = JSON.stringify(object);

    fetch("/api/entry", {
        method: 'POST',
        body: jsonString,
        headers: { 'Content-Type': 'application/json' },
    })
    .then(response => {
        console.log('test', response);
        if (response.ok) { return response.json(); }
        throw new Error(`HTTP error! status: ${response.status}`);
    })
    .then(data => {
        console.log('Command response:', data);
        inputField.value = '';
    })
    .catch(error => {
        console.error('Error sending command:', error);
    });
}