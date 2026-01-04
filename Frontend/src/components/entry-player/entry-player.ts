import { Component, ElementRef, ViewChild, AfterViewInit, WritableSignal } from '@angular/core';
import { Router } from '@angular/router';
import { Entry } from '@models/entities';
import { StateService } from '@services/state';

@Component({
  selector: 'entry-player',
  standalone: true,
  templateUrl: './entry-player.html',
  styleUrl: './entry-player.css'
})
export class EntryPlayer implements AfterViewInit {
  @ViewChild('entryText') entryText!: ElementRef<HTMLParagraphElement>;
  readonly entries: WritableSignal<Entry[]>;
  readonly playbackSpeed: WritableSignal<number>;

  constructor (
    private state: StateService,
    private router: Router,
  ) {
    this.entries = state.entries;
    this.playbackSpeed = state.playbackSpeed;
  }

  ngAfterViewInit(): void {
    this.play();
  }

  private async play() {
    const entriesOfSelectedDate = this.state.entriesOfSelectedDate();
    for (const entry of entriesOfSelectedDate) {
      await this.replay(entry);
    }
    this.state.isPlaying.set(false);
    this.router.navigate(['/']); // TODO: navigate to where user left off
  }

  private async replay(entry: Entry): Promise<void> {
    const speedAdjustment = 1000 / this.playbackSpeed();
    const el = this.entryText.nativeElement;
    el.innerHTML = "";

    for (let i = 0; i < entry.text.length; i++) {
      el.classList.remove('hidden');
      const char = entry.text[i];
      const delay = entry.intervals[i];

      await new Promise(resolve => setTimeout(resolve, delay * speedAdjustment));

      if (char === '\b') {
        el.textContent = el.textContent?.slice(0, -1) || '';
      } else if (char === '\n') {
        el.classList.add('hidden');
        return;
      } else {
        el.textContent += char;
      }
    }
  }
}
