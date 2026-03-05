import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard'; 

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, AdminDashboardComponent], 
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class AppComponent {
  title = 'CompraProgramada-Web';
}