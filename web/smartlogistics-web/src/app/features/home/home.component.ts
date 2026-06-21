import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/auth/auth.service';
import { environment } from '../../core/config/environment';

@Component({
  selector: 'app-home',
  imports: [MatButtonModule, RouterLink],
  template: `
    <main class="home">
      <h1>{{ environment.appName }}</h1>
      <p>Angular 22 · Keycloak OIDC · YARP Gateway · .NET 10 Microservices</p>

      @if (auth.isAuthenticated()) {
        <a mat-raised-button color="primary" routerLink="/dashboard">Go to Dashboard</a>
      } @else {
        <button mat-raised-button color="primary" (click)="auth.login()">Login with Keycloak</button>
      }
    </main>
  `,
  styles: `
    .home {
      text-align: center;
      padding: 4rem 1rem;
    }
    h1 { margin-bottom: 0.5rem; }
    p { color: #555; margin-bottom: 2rem; }
  `,
})
export class HomeComponent {
  readonly auth = inject(AuthService);
  readonly environment = environment;
}
