import { Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Entry } from '@models/entities';

@Component({
  selector: 'date-detail',
  standalone: true,
  templateUrl: './date-detail.html',
  styleUrl: './date-detail.css',
  imports: [DatePipe]
})
export class DateDetail {
  @Input({ required: true }) date!: Date;
  @Input({ required: true }) entries: Entry[] = [];

  play() {
    console.log('Playing entries for', this.date);
  }
}
