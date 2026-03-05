import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClienteCarteira } from './cliente-carteira';

describe('ClienteCarteira', () => {
  let component: ClienteCarteira;
  let fixture: ComponentFixture<ClienteCarteira>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClienteCarteira],
    }).compileComponents();

    fixture = TestBed.createComponent(ClienteCarteira);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
