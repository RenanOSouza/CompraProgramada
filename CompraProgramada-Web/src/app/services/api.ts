import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  
  private baseUrl = 'http://localhost:5109/api'; 

  constructor(private http: HttpClient) { }

  getCarteira(clienteId: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/clientes/${clienteId}/carteira`);
  }

  getCestaAtual(): Observable<any> {
    return this.http.get(`${this.baseUrl}/admin/cesta/atual`);
  }

  
  getCustodiaMaster(): Observable<any> {
    return this.http.get(`${this.baseUrl}/admin/conta-master/custodia`); 
  }

atualizarCesta(dadosCesta: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/admin/cesta-top-five`, dadosCesta);
  }

  executarMotor(dataReferencia: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/motor/executar-compra`, { dataReferencia });
  }

  atualizarAporte(clienteId: number, novoValorMensal: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/clientes/${clienteId}/valor-mensal`, { novoValorMensal });
  }

  getHistoricoCestas(): Observable<any> {
    return this.http.get(`${this.baseUrl}/admin/historico`); 
  }

  getHistoricoVendas(clienteId: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/clientes/${clienteId}/historico-vendas`);
  }

  sairDoProduto(clienteId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/clientes/${clienteId}/saida`, {});
  }
}