import { Component, Signal, WritableSignal, signal, ViewChild, ElementRef } from '@angular/core';
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
  @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;
  @ViewChild('dateFrom') dateFrom?: ElementRef<HTMLInputElement>;
  @ViewChild('dateTo') dateTo?: ElementRef<HTMLInputElement>;
  @ViewChild('strictMatch') strictMatch?: ElementRef<HTMLInputElement>;

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
    if (mode === 'SEARCH') {
      setTimeout(() => this.searchInput?.nativeElement.focus(), 0);
    }
  }

  updateSearchResults() {
    const query = this.searchInput?.nativeElement.value.toLowerCase().trim() || "";
    const tags = query.split(" ").filter(s => s.trim() !== "");
    const uniqueTags = [...new Set(tags)].filter(w => w.length > 0);
    
    if (uniqueTags.length === 0) {
      this.searchResults.set([]);
      return;
    }
    
    const fromVal = this.dateFrom?.nativeElement.value;
    const toVal = this.dateTo?.nativeElement.value;
    const isStrict = this.strictMatch?.nativeElement.checked || false;
    const fromDate = fromVal ? new Date(fromVal) : null;
    const toDate = toVal ? new Date(toVal) : null;
    if (toDate) toDate.setHours(23, 59, 59, 999);

    const results = this.state.entries().filter(entry => {
      const entryStr = entry.string?.toLowerCase() || '';
      if (isStrict) {
        if (!uniqueTags.every(word => entryStr.includes(word))) return false;
      } else {
        if (!uniqueTags.some(word => entryStr.includes(word))) return false;
      }

      if (fromDate && entry.time < fromDate) return false;
      if (toDate && entry.time > toDate) return false;
      return true;
    });

    this.searchResults.set(results);
  }

  clearSearch(input: HTMLInputElement) {
    input.value = '';
    this.updateSearchResults();
    input.focus();
  }

  clearSelection() {
    this.state.clearSelection();
  }

  createNewEntry() {
    this.state.startRecording();
  }
}
