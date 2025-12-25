import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { DiaryService } from '@services/diary';
import { Entry } from '@models/entities';
import { KeyValuePipe } from '@angular/common';

@Component({
  selector: 'entry-list',
  standalone: true,
  templateUrl: './entry-list.html',
  styleUrl: './entry-list.css',
  imports: [
    KeyValuePipe
  ],
})
export class EntryList implements OnInit {
  entries: WritableSignal<Entry[]> = signal([]);
  dateMap: WritableSignal<Map<Number, Entry[]>> = signal(new Map<Number, Entry[]>());

  constructor(private diaryService: DiaryService) {
  }

  ngOnInit() {
    setTimeout(() => {
      this.fetchAllEntries();
    });
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

        var dateMap = new Map<Number, Entry[]>();
        this.entries().forEach(entry => {
          var date = new Date(
            entry.time.getFullYear(),
            entry.time.getMonth(),
            entry.time.getDate()
          );
          var dateIndex = date.getTime();
          var entriesInDate = dateMap.has(dateIndex) ? dateMap.get(dateIndex)! : [];
          entriesInDate.push(entry);
          dateMap.set(dateIndex, entriesInDate);
        });
        this.dateMap.set(dateMap);
      },
      error: (error) => console.error("Error fetching entries", error)
    });
  }
}
