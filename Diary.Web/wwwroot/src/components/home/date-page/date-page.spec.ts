import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DatePage } from './date-page';

describe('DateDetail', () => {
  let component: DatePage;
  let fixture: ComponentFixture<DatePage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DatePage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DatePage);
    component = fixture.componentInstance;
    component.date = new Date();
    component.entries = [];
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
