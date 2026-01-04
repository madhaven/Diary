import { Component, ViewChild, ElementRef, input, output, AfterViewInit } from '@angular/core';
import { Entry } from '@models/entities';

@Component({
  selector: 'diary-search',
  standalone: true,
  templateUrl: './search.html',
  styleUrl: './search.css',
  imports: []
})
export class Search implements AfterViewInit {
  entries = input.required<Entry[]>();
  resultsChange = output<Entry[]>();

  @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;
  @ViewChild('dateFrom') dateFrom?: ElementRef<HTMLInputElement>;
  @ViewChild('dateTo') dateTo?: ElementRef<HTMLInputElement>;
  @ViewChild('strictMatch') strictMatch?: ElementRef<HTMLInputElement>;

  ngAfterViewInit() {
    this.searchInput?.nativeElement.focus();
  }

  updateSearchResults() {
    const query = this.searchInput?.nativeElement.value.toLowerCase().trim() || "";
    const tags = query.split(" ").filter(s => s.trim() !== "");
    const uniqueTags = [...new Set(tags)].filter(w => w.length > 0);
    
    if (uniqueTags.length === 0) {
      this.resultsChange.emit([]);
      return;
    }
    
    const fromVal = this.dateFrom?.nativeElement.value;
    const toVal = this.dateTo?.nativeElement.value;
    const isStrict = this.strictMatch?.nativeElement.checked || false;
    const fromDate = fromVal ? new Date(fromVal) : null;
    const toDate = toVal ? new Date(toVal) : null;
    if (toDate) toDate.setHours(23, 59, 59, 999);

    const results = this.entries().filter(entry => {
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

    this.resultsChange.emit(results);
  }

  clearSearch(input: HTMLInputElement) {
    input.value = '';
    this.updateSearchResults();
    input.focus();
  }
}
