import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DateDetail } from './date-detail';

describe('DateDetail', () => {
  let component: DateDetail;
  let fixture: ComponentFixture<DateDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DateDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DateDetail);
    component = fixture.componentInstance;
    component.date = new Date();
    component.entries = [];
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
