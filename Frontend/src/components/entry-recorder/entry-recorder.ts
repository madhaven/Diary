import { AfterViewInit, Component, ElementRef, EventEmitter, NgZone, Output, signal, ViewChild } from '@angular/core';
import { DiaryService } from '@services/diary';
import { StateService } from '@services/state';
import { Entry } from '@models/entities';
import { EntryContract } from '@models/contracts';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'entry-recorder',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './entry-recorder.html',
  styleUrl: './entry-recorder.css',
})
export class EntryRecorder implements AfterViewInit {
  @ViewChild('commandfield') commandField!: ElementRef;
  @Output('entryAdded') entryAdded = new EventEmitter<Entry>();

  private readonly PROMPT_DELAYS = [0.509,0.24,0.09,0.08,0.185,0.106,0.044,0.05,0.159,0.036,0.094,0.132,0.001,0.095,0.171,0.116,0.135,0.069,0.113,0.111,0.133,0.086,0.122,0.033,0.081,0.12,0.084,0.111,0.107,0.052,0.499,0.13,0.57]; // 1.057
  private keyPressTimestamps: number[] = [];
  private promptMessage = "Say something good about today :)";
  private inputKeys: string = "";
  private preventKeys = ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End', 'Delete', 'Tab'];
  userInput: string = '';
  promptText = signal("");
  isPromptVisible = signal(true);
  
  // Speech Recognition
  recognition: any;
  isRecording = signal(false);

  constructor(
    private diaryService: DiaryService,
    private state: StateService,
    private router: Router,
    private zone: NgZone
  ) { } 

  ngOnInit() {
    setTimeout(() => {
      this.autoTypePrompt(this.PROMPT_DELAYS);
      this.resetKeys();
    });
    this.initSpeechRecognition();
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.commandField.nativeElement.focus();
    }, 0);
  }
  
  private initSpeechRecognition() {
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    if (!SpeechRecognition) return;

    this.recognition = new SpeechRecognition();
    this.recognition.continuous = true;
    this.recognition.interimResults = true;
    this.recognition.lang = 'en-US';

    this.recognition.onresult = (event: any) => {
      this.zone.run(() => {
        for (let i = event.resultIndex; i < event.results.length; i++) {
          if (event.results[i].isFinal) {
            const transcript = event.results[i][0].transcript;
            this.handleSpeechText(transcript);
          }
        }
      });
    };
    
    this.recognition.onend = () => {
      if (this.isRecording()) {
        try {
          this.recognition.start();
        } catch(e) {
          // ignore if already started
        }
      }
    };
  }

  toggleRecording() {
    if (!this.recognition) return;
    
    if (this.isRecording()) {
      this.stopRecording();
    } else {
      this.startRecording();
    }
  }

  startRecording() {
    this.isRecording.set(true);
    try {
      this.recognition.start();
    } catch(e) {
      console.error(e);
    }
  }

  stopRecording() {
    this.isRecording.set(false);
    this.recognition?.stop();
  }

  handleSpeechText(text: string) {
    if (this.isPromptVisible()) this.makePromptVanish();
    
    let textToAppend = text;
    if (this.userInput && !this.userInput.endsWith(' ') && textToAppend.trim().length > 0) {
        textToAppend = ' ' + textToAppend;
    }
    
    this.userInput += textToAppend;
    for (const char of textToAppend) {
      this.recordKey(char);
    }

    this.forceRefresh();
  }

  private forceRefresh() {
    this.commandField.nativeElement.blur();
    setTimeout(() => this.setCaretToEnd(), 10);
  }

  private async autoTypePrompt(intervals: number[]) {
    this.promptText.set("");
    for (let i = 0; i < this.promptMessage.length; i++) {
      await this.sleep(intervals[i]);
      this.promptText.update(msg => msg + this.promptMessage[i]);
    }
  }

  makePromptVanish() {
    this.isPromptVisible.set(false);
  }

  private sleep(s: number) {
    return new Promise(resolve => setTimeout(resolve, s * 250));
  }

  private resetKeys() {
    this.inputKeys = "";
    this.userInput = "";
    this.keyPressTimestamps = [Date.now()];
    this.commandField.nativeElement.blur();
    this.commandField.nativeElement.focus();
    window.scrollTo({behavior: 'smooth', top: document.body.scrollHeight})
  }

  private recordKey(key: string) {
    this.inputKeys += key;
    this.keyPressTimestamps.push(Date.now());
  }

  setCaretToEnd() {
    const element = this.commandField.nativeElement;
    if (typeof element.selectionStart == "number") {
      element.selectionStart = element.selectionEnd = element.value.length;
      element.focus();
    } else if (typeof element.createTextRange != "undefined") { // to deal with old-school browsers
      element.focus();
      var range = element.createTextRange();
      range.collapse(false);
      range.select();
    }
  }

  handleInputKeys(event: KeyboardEvent) {
    if (event.key === 'Backspace') {
      if (event.ctrlKey) {
        event.preventDefault();
        return;
      }
      
      if (this.userInput.length > 0) {
        this.recordKey('\b');
      }
      return;
    }

    if (event.key === 'Enter') {
      event.preventDefault();
      this.handleEntryEnter();
      return;
    }

    if (this.preventKeys.includes(event.key)) {
      event.preventDefault();
      return;
    }

    if (event.key.length === 1) {
      if (this.isPromptVisible()) this.makePromptVanish();
      this.recordKey(event.key);
      return;
    }

    console.warn("modified key", event.key);
  }

  private handleEntryEnter() {
    if (this.userInput.trim() === '') {
      this.resetKeys();
      return;
    }

    const timestamps = this.keyPressTimestamps;
    timestamps.push(new Date().getTime());
    const intervals: number[] = [];
    for (let i = 1; i < timestamps.length; i++) {
      intervals.push((timestamps[i] - timestamps[i-1]) / 1000);
    }

    const newEntry: Entry = {
      "text": this.inputKeys + '\n',
      "intervals": intervals,
      "time": new Date(),
      "printDate": false
    };

    this.state.addEntry(newEntry).subscribe({
      next: (response: EntryContract) => {
        this.resetKeys();
        console.log('added entry', response);
        const entry = {
          intervals: response.intervals,
          printDate: response.printDate,
          text: response.text,
          time: new Date(response.time),
          id: "",
          string: this.diaryService.entryToString(response.text)
        };
        this.entryAdded.emit(entry);
        this.resetKeys();
        this.stopWordCheck(entry);
      },
      error: (error) => console.error('Error adding entry', error)
    });
  }

  private stopWordCheck(lastEntry: Entry): void {
    var entryText = this.diaryService.entryToString(lastEntry.text);
    if (entryText.toLowerCase().includes('bye')) {
      this.state.getAllEntries();
      this.router.navigate(['/']);
    }
  }
}
