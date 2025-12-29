import { Component, Input } from '@angular/core';
import { Entry } from '@models/entities';

@Component({
  selector: 'entry-player',
  standalone: true,
  templateUrl: './entry-player.html',
  styleUrl: './entry-player.css'
})
export class EntryPlayer {
  @Input({ required: true }) entries: Entry[] = [];
}
