import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EntryRecorder } from './entry-recorder';

describe('DiaryEntry', () => {
  let component: EntryRecorder;
  let fixture: ComponentFixture<EntryRecorder>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EntryRecorder]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EntryRecorder);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
