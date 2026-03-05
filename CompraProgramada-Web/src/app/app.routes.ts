import { Routes } from '@angular/router';

import { ClienteCarteiraComponent } from './components/cliente-carteira/cliente-carteira';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard';
import { LoginComponent } from './components/login/login';
import { CadastroComponent } from './components/cadastro/cadastro'; 

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' }, 
  { path: 'login', component: LoginComponent },
  { path: 'cadastro', component: CadastroComponent },
  { path: 'cliente', component: ClienteCarteiraComponent },
  { path: 'admin', component: AdminDashboardComponent }
];