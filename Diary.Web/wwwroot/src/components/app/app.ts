import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Entry } from 'models/entities';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
})
export class App {
  entries: Entry[] = [];

  constructor() { }
}