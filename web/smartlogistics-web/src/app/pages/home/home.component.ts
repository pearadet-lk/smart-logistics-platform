import { Component } from '@angular/core';
import { environment } from '../../core/config/environment';

@Component({
  selector: 'app-home',
  template: `
    <main class="home">
      <h1>{{ environment.appName }}</h1>
      <p class="subtitle">Angular 22 SPA — placeholder</p>

      <section class="cards">
        <article>
          <h2>API Gateway</h2>
          <code>{{ environment.apiGatewayUrl }}</code>
        </article>
        <article>
          <h2>Keycloak</h2>
          <code>{{ environment.keycloak.url }}/realms/{{ environment.keycloak.realm }}</code>
        </article>
        <article>
          <h2>Auth flow</h2>
          <p>Authorization Code + PKCE (TODO)</p>
        </article>
      </section>
    </main>
  `,
  styles: `
    .home {
      max-width: 960px;
      margin: 0 auto;
      padding: 2rem 1.5rem;
      font-family: system-ui, sans-serif;
    }

    h1 {
      margin: 0 0 0.25rem;
      font-size: 1.75rem;
    }

    .subtitle {
      margin: 0 0 2rem;
      color: #555;
    }

    .cards {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    }

    article {
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1rem;
      background: #fafafa;
    }

    h2 {
      margin: 0 0 0.5rem;
      font-size: 1rem;
    }

    code {
      font-size: 0.85rem;
      word-break: break-all;
    }

    p {
      margin: 0;
      color: #444;
    }
  `,
})
export class HomeComponent {
  protected readonly environment = environment;
}
