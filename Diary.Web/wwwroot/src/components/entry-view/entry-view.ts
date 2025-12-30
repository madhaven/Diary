import { Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Entry } from '@models/entities';
import { StateService } from '@services/state';

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

  constructor (
    private state: StateService,
  ) {}

  play() {
    this.state.play();
  }
}
