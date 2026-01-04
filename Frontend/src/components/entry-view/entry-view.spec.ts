import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EntryView } from './entry-view';

describe('DateDetail', () => {
  let component: EntryView;
  let fixture: ComponentFixture<EntryView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EntryView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EntryView);
    component = fixture.componentInstance;
    component.date = new Date();
    component.entries = [];
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
