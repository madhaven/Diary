import { Component, Signal, WritableSignal, signal } from '@angular/core';
import { EntryList } from '@components/entry-list/entry-list';
import { RouterLink } from '@angular/router';
import { Entry } from '@models/entities';
import { StateService } from '@services/state';
import { EntryView } from '@components/entry-view/entry-view';
import { EmptyState } from '@components/empty-state/empty-state';
import { EntryPlayer } from '@components/entry-player/entry-player';
import { Search } from '@components/search/search';

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
    Search,
  ],
})
export class Home {
  readonly entries: WritableSignal<Entry[]>;
  readonly selectedDate: WritableSignal<Date | null>;
  readonly isPlaying: WritableSignal<boolean>;
  readonly isTransitioning: WritableSignal<boolean>;
  readonly selectedDateEntries: Signal<Entry[]>;
  readonly sidebarMode = signal<'ENTRIES' | 'SEARCH'>('ENTRIES');
  readonly searchResults = signal<Entry[]>([]);
  
  constructor(
    private state: StateService,
  ) {
    this.entries = this.state.entries;
    this.selectedDate = this.state.selectedDate;
    this.isPlaying = this.state.isPlaying;
    this.isTransitioning = this.state.isTransitioning;
    this.selectedDateEntries = this.state.entriesOfSelectedDate;
  }

  setSidebarMode(mode: 'ENTRIES' | 'SEARCH') {
    this.sidebarMode.set(mode);
  }

  clearSelection() {
    this.state.clearSelection();
  }

  createNewEntry() {
    this.state.startRecording();
  }
}
