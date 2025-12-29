import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Entry } from '@models/entities';

@Component({
  selector: 'date-detail',
  standalone: true,
  templateUrl: './date-page.html',
  styleUrl: './date-page.css',
  imports: [DatePipe]
})
export class DatePage {
  @Input({ required: true }) date!: Date;
  @Input({ required: true }) entries: Entry[] = [];
  @Output() playClicked = new EventEmitter<void>();

  play() {
    this.playClicked.emit();
  }
}
