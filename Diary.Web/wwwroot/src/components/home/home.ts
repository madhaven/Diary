import { Component, Signal, WritableSignal, signal } from '@angular/core';
import { EntryList } from '@components/entry-list/entry-list';
import { RouterLink } from '@angular/router';
import { Entry } from '@models/entities';
import { StateService } from '@services/state';
import { EntryView } from '@components/entry-view/entry-view';
import { EmptyState } from '@components/empty-state/empty-state';
import { EntryPlayer } from '@components/entry-player/entry-player';

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
  ],
})
export class Home {
  readonly entries: WritableSignal<Entry[]>;
  readonly selectedDate: WritableSignal<Date | null>;
  readonly isPlaying: WritableSignal<boolean>;
  readonly isTransitioning: WritableSignal<boolean>;
  readonly selectedDateEntries: Signal<Entry[]>;
  
  constructor(
    private state: StateService,
  ) {
    this.entries = this.state.entries;
    this.selectedDate = this.state.selectedDate;
    this.isPlaying = this.state.isPlaying;
    this.isTransitioning = this.state.isTransitioning;
    this.selectedDateEntries = this.state.entriesOfSelectedDate;
  }

  clearSelection() {
    this.state.clearSelection();
  }

  createNewEntry() {
    this.state.startRecording();
  }
}
