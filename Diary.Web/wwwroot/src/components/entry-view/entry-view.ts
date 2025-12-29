import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Entry } from '@models/entities';

@Component({
  selector: 'entry-view',
  standalone: true,
  templateUrl: './entry-view.html',
  styleUrl: './entry-view.css',
  imports: [DatePipe]
})
export class EntryView {
  @Input({ required: true }) date!: Date;
  @Input({ required: true }) entries: Entry[] = [];
  @Output() playClicked = new EventEmitter<void>();

  play() {
    this.playClicked.emit();
  }
}
