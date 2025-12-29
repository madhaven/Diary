import { Routes } from '@angular/router';
import { EntryRecorder } from './components/entry-recorder/entry-recorder';
import { Home } from '@components/home/home';

export const appRoutes: Routes = [
  { path: 'newentry', component: EntryRecorder },
  { path: '', component: Home },
  { path: '**', redirectTo: '' }
];
