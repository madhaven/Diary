import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EntryList } from './entry-list';

describe('EntryList', () => {
  let component: EntryList;
  let fixture: ComponentFixture<EntryList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EntryList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EntryList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
