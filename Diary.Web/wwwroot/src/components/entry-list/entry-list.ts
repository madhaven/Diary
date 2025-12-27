import { Component, ElementRef, EventEmitter, HostListener, Input, Output, signal, WritableSignal } from '@angular/core';
import { Entry } from '@models/entities';
import { DatePipe, KeyValue, KeyValuePipe } from '@angular/common';

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
  yearMap: WritableSignal<Map<number, Map<number, Entry[]>>> = signal(new Map<number, Map<number, Entry[]>>());
  expandedYear = signal<number | null>(null);
  
  @Input() selectedDate: Date | null = null;

  @Input() set entries(value: Entry[]) {
    const yearMap = new Map<number, Map<number, Entry[]>>();
    value.forEach(entry => {
      const year = entry.time.getFullYear();
      const date = new Date(year, entry.time.getMonth(), entry.time.getDate());
      const dateKey = date.getTime();      
      const dateMap = yearMap.get(year) ?? new Map<number, Entry[]>();
      const entries = dateMap.get(dateKey) ?? [];
      entries.push(entry);
      dateMap.set(dateKey, entries);
      yearMap.set(year, dateMap);
    });

    this.yearMap.set(yearMap);
  }

  @Output() entrySelected = new EventEmitter<Entry>();
  @Output() yearSelected = new EventEmitter<number>();
  @Output() dateSelected = new EventEmitter<Date>();

  constructor(private elementRef: ElementRef) {
  }

  toggleYear(year: number, event: Event) {
    event.stopPropagation();
    this.yearSelected.emit(year);
    this.expandedYear.set(this.expandedYear() === year ? null : year);
  }

  selectDate(date: number, event: Event) {
    event.stopPropagation();
    this.dateSelected.emit(new Date(date));
  }

  selectEntry(entry: Entry) {
    this.entrySelected.emit(entry);
  }

  isSelected(dateKey: number): boolean {
    return this.selectedDate?.getTime() === dateKey;
  }
  
  ascOrder = (a: KeyValue<number, any>, b: KeyValue<number, any>): number => {
    return (a.key as number) >= (b.key as number)
      ? ((b.key as number) < (a.key as number) ? 1 : 0)
      : -1;
  }
}