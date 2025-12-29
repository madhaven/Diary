import { Component, computed, OnInit, signal, WritableSignal } from '@angular/core';
import { EntryList } from '@components/home/entry-list/entry-list';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { Entry } from '@models/entities';
import { DiaryService } from '@services/diary';
import { EmptyState } from './empty-state/empty-state';
import { DatePage } from './date-page/date-page';
import { DiaryEntry } from '@components/diary-entry/diary-entry';
import { EntryPlayer } from '@components/home/entry-player/entry-player';
import { filter } from 'rxjs';

@Component({
  selector: 'diary-home',
  standalone: true,
  templateUrl: './home.html',
  styleUrl: './home.css',
  imports: [
    EntryList,
    RouterLink,
    EmptyState,
    DatePage,
    DiaryEntry,
    EntryPlayer
  ],
})
export class Home implements OnInit {
  selectedDate = signal<Date | null>(null);
  entries: WritableSignal<Entry[]> = signal([]);
  isNewEntry = signal(false);
  isPlaying = signal(false);
  isTransitioning = signal(false);
  
  selectedDateEntries = computed(() => {
    const date = this.selectedDate();
    if (!date) return [];
    
    return this.entries().filter(entry => 
      entry.time.getFullYear() === date.getFullYear() &&
      entry.time.getMonth() === date.getMonth() &&
      entry.time.getDate() === date.getDate()
    );
  });

  constructor(private diaryService: DiaryService, private router: Router) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.checkRoute();
    });
  }

  ngOnInit() {
    this.fetchAllEntries();
    this.checkRoute();
  }

  private checkRoute() {
    this.isNewEntry.set(this.router.url === '/newentry');
    if (this.isNewEntry()) {
      this.selectedDate.set(null);
      this.isPlaying.set(false);
      this.isTransitioning.set(false);
    }
  }

  onDateSelected(date: Date) {
    this.selectedDate.set(date);
    this.isPlaying.set(false);
    this.isTransitioning.set(false);
    if (this.isNewEntry()) {
      this.router.navigate(['/']);
    }
  }

  onPlay() {
    this.isTransitioning.set(true);
    setTimeout(() => {
      this.isPlaying.set(true);
      this.isTransitioning.set(false);
    }, 400);
  }

  onEntryAdded(entry: Entry) {
    this.fetchAllEntries();
    this.selectedDate.set(entry.time);
    this.router.navigate(['/']);
  }

  clearSelection() {
    this.selectedDate.set(null);
  }

  private fetchAllEntries() {
    this.diaryService.getAllEntries().subscribe({
      next: (data) => {
        var entries = data.map(entry => {
          return {
            id: entry.id,
            text: entry.text,
            intervals: entry.intervals,
            time: new Date(entry.time),
            printDate: entry.printDate,
            string: this.diaryService.entryToString(entry.text),
          } as Entry;
        });
        this.entries.set(entries);
      },
      error: (error) => console.error("Error fetching entries", error)
    });
  }
}