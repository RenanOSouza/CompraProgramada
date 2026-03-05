import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  
  private baseUrl = 'http://localhost:5109/api/usuarios'; 

  constructor(private http: HttpClient) { }

  login(credenciais: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, credenciais);
  }

  cadastrar(dadosUsuario: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/adesao`, dadosUsuario);
  }
}