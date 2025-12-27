import { Component, computed, OnInit, signal, WritableSignal } from '@angular/core';
import { EntryList } from '@components/entry-list/entry-list';
import { RouterLink } from '@angular/router';
import { Entry } from '@models/entities';
import { DatePipe } from '@angular/common';
import { DiaryService } from '@services/diary';
import { EmptyState } from './empty-state/empty-state';
import { DateDetail } from './date-detail/date-detail';

@Component({
  selector: 'diary-home',
  standalone: true,
  templateUrl: './home.html',
  styleUrl: './home.css',
  imports: [
    EntryList,
    RouterLink,
    EmptyState,
    DateDetail
  ],
})
export class Home implements OnInit {
  selectedDate = signal<Date | null>(null);
  entries: WritableSignal<Entry[]> = signal([]);
  
  selectedDateEntries = computed(() => {
    const date = this.selectedDate();
    if (!date) return [];
    
    return this.entries().filter(entry => 
      entry.time.getFullYear() === date.getFullYear() &&
      entry.time.getMonth() === date.getMonth() &&
      entry.time.getDate() === date.getDate()
    );
  });

  constructor(private diaryService: DiaryService) {}

  ngOnInit() {
    this.fetchAllEntries();
  }

  onDateSelected(date: Date) {
    this.selectedDate.set(date);
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
