import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DiaryEntry } from './diary-entry';

describe('DiaryEntry', () => {
  let component: DiaryEntry;
  let fixture: ComponentFixture<DiaryEntry>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DiaryEntry]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DiaryEntry);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
