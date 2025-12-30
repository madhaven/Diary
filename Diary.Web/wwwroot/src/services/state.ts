import { Injectable, signal, computed, effect } from '@angular/core';
import { DiaryService } from './diary';
import { Entry } from '@models/entities';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';

export type DiaryPage = 'HOME' | 'ENTRY_RECORD' | '';

@Injectable({
  providedIn: 'root'
})
export class StateService {
  // Core Data
  readonly entries = signal<Entry[]>([]);
  readonly selectedDate = signal<Date | null>(null);

  // View State
  readonly currentPage = signal<DiaryPage>('HOME');
  readonly expandedYear = signal<number | null>(null);
  readonly isPlaying = signal(false);
  readonly isTransitioning = signal(false);

  // Derived State
  readonly entriesOfSelectedDate = computed(() => {
    const date = this.selectedDate();
    const allEntries = this.entries();
    if (!date) return [];
    
    return allEntries.filter(entry => 
      entry.time.getFullYear() === date.getFullYear() &&
      entry.time.getMonth() === date.getMonth() &&
      entry.time.getDate() === date.getDate()
    );
  });

  constructor(
    private diaryService: DiaryService,
    private router: Router
  ) {
    this.getAllEntries();

    // Track routing for page state
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.updatePageFromUrl(event.urlAfterRedirects || event.url);
    });
    
    // Initialize page state
    this.updatePageFromUrl(this.router.url);
  }

  private updatePageFromUrl(url: string) {
    if (url.includes('newentry')) {
      this.currentPage.set('ENTRY_RECORD');
      // Reset home-specific states when leaving home context conceptually
      this.selectedDate.set(null);
      this.isPlaying.set(false);
    } else {
      this.currentPage.set('HOME');
    }
  }

  // Data Operations Relay
  // just to keep every operation going via state

  getAllEntries() {
    this.diaryService.getAllEntries().subscribe({
      next: (data) => {
        const entries = data.map(entry => ({
          id: entry.id,
          text: entry.text,
          intervals: entry.intervals,
          time: new Date(entry.time),
          printDate: entry.printDate,
          string: this.diaryService.entryToString(entry.text),
        } as Entry));
        this.entries.set(entries);
      },
      error: (err) => console.error('Failed to fetch entries', err)
    });
  }

  addEntry(entry: Entry) {
    return this.diaryService.addEntry(entry);
  }

  // UI State Actions

  setSelectedDate(date: Date | null) {
    this.selectedDate.set(date);
    if (date) {
      this.isPlaying.set(false);
      this.isTransitioning.set(false);
      if (this.currentPage() === 'ENTRY_RECORD') {
        this.router.navigate(['/']);
      }
    }
  }

  clearSelection() {
    this.selectedDate.set(null);
  }

  setPlaying(playing: boolean) {
    this.isPlaying.set(playing);
  }
  
  setTransitioning(transitioning: boolean) {
    this.isTransitioning.set(transitioning);
  }

  play() {
    this.isTransitioning.set(true);
    setTimeout(() => {
      this.isPlaying.set(true);
      this.isTransitioning.set(false);
    }, 500);
  }
}
