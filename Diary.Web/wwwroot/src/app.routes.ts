import { Routes } from '@angular/router';
import { DiaryEntry } from './components/diary-entry/diary-entry';
import { Home } from '@components/home/home';

export const appRoutes: Routes = [
  { path: 'newentry', component: DiaryEntry },
  { path: '', component: Home },
  { path: '**', redirectTo: '' }
];
