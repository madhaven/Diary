import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Entry } from '@models/entities';
import { EntryContract } from '@models/contracts';

@Injectable({
  providedIn: 'root'
})
export class DiaryService {

  private apiUrl = 'http://localhost:5086/api/entry';

  constructor(private http: HttpClient) { }

  getAllEntries(): Observable<EntryContract[]> {
    return this.http.get<EntryContract[]>(`${this.apiUrl}/all`);
  }

  addEntry(entry: Entry): Observable<EntryContract> {
    var contract: EntryContract = {
      id: null,
      intervals: entry.intervals,
      printDate: entry.printDate,
      text: entry.text,
      time: entry.time.toISOString(),
    }
    return this.http.post<EntryContract>(this.apiUrl, contract);
  }

  public entryToString(entry: string): string {
    let result = "";
    for (let i = 0; i < entry.length; i++) {
      if (entry[i] === '\b') {
        result = result.slice(0, result.length - 1);
      } else {
        result += entry[i];
      }
    }
    return result;
  }

}