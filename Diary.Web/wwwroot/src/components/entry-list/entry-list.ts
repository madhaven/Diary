import { Component, computed, signal, WritableSignal } from '@angular/core';
import { Entry } from '@models/entities';
import { DatePipe, KeyValue, KeyValuePipe } from '@angular/common';
import { StateService } from '@services/state';

@Component({
  selector: 'entry-list',
  standalone: true,
  templateUrl: './entry-list.html',
  styleUrl: './entry-list.css',
  imports: [
    KeyValuePipe,
    DatePipe,
  ],
})
export class EntryList {
  readonly expandedYear: WritableSignal<number | null>;
  yearMap = computed(() => {
    const entries = this.state.entries();
    const yearMap = new Map<number, Map<number, Entry[]>>();
    entries.forEach(entry => {
      const year = entry.time.getFullYear();
      const date = new Date(year, entry.time.getMonth(), entry.time.getDate());
      const dateKey = date.getTime();      
      const dateMap = yearMap.get(year) ?? new Map<number, Entry[]>();
      const entriesForDate = dateMap.get(dateKey) ?? [];
      entriesForDate.push(entry);
      dateMap.set(dateKey, entriesForDate);
      yearMap.set(year, dateMap);
    });
    return yearMap;
  });

  constructor(private state: StateService) {
    this.expandedYear = state.expandedYear;
  }

  toggleYear(year: number, event: Event) {
    event.stopPropagation();
    this.expandedYear.set(this.expandedYear() === year ? null : year);
  }

  selectDate(date: number, event: Event) {
    event.stopPropagation();
    this.state.setSelectedDate(new Date(date));
  }

  isSelected(dateKey: number): boolean {
    return this.state.selectedDate()?.getTime() === dateKey;
  }
  
  ascOrder = (a: KeyValue<number, any>, b: KeyValue<number, any>): number => {
    return (a.key as number) >= (b.key as number)
      ? ((b.key as number) < (a.key as number) ? 1 : 0)
      : -1;
  }
}
