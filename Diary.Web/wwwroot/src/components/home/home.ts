import { Component, computed, Signal, WritableSignal } from '@angular/core';
import { EntryList } from '@components/entry-list/entry-list';
import { RouterLink } from '@angular/router';
import { Entry } from '@models/entities';
import { StateService } from '@services/state';
import { EntryRecorder } from '@components/entry-recorder/entry-recorder';
import { EntryPlayer } from '@components/entry-player/entry-player';
import { EntryView } from '@components/entry-view/entry-view';
import { EmptyState } from '@components/empty-state/empty-state';

@Component({
  selector: 'diary-home',
  standalone: true,
  templateUrl: './home.html',
  styleUrl: './home.css',
  imports: [
    EntryList,
    RouterLink,
    EmptyState,
    EntryView,
    EntryPlayer,
    EntryRecorder,
  ],
})
export class Home {
  readonly entries: WritableSignal<Entry[]>;
  readonly selectedDate: WritableSignal<Date | null>;
  readonly isPlaying: WritableSignal<boolean>;
  readonly isTransitioning: WritableSignal<boolean>;
  readonly selectedDateEntries: Signal<Entry[]>;
  
  readonly isRecording = computed(() => this.state.currentPage() === 'ENTRY_RECORD');

  constructor(
    private state: StateService,
  ) {
    this.entries = this.state.entries;
    this.selectedDate = this.state.selectedDate;
    this.isPlaying = this.state.isPlaying;
    this.isTransitioning = this.state.isTransitioning;
    this.selectedDateEntries = this.state.entriesOfSelectedDate;
  }

  onPlay() {
    this.state.play();
  }

  clearSelection() {
    this.state.clearSelection();
  }
}
