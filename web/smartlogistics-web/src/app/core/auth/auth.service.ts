import { Injectable, inject, signal } from '@angular/core';
import Keycloak from 'keycloak-js';
import { environment } from '../config/environment';
import { SignalRService } from '../realtime/signalr.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly keycloak = new Keycloak({
    url: environment.keycloak.url,
    realm: environment.keycloak.realm,
    clientId: environment.keycloak.clientId,
  });

  readonly isAuthenticated = signal(false);
  readonly username = signal<string | null>(null);
  readonly roles = signal<string[]>([]);
  readonly permissions = signal<string[]>([]);
  readonly tenant = signal<string | null>(null);

  private readonly signalR = inject(SignalRService);

  async init(): Promise<void> {
    const authenticated = await this.keycloak.init({
      onLoad: 'check-sso',
      pkceMethod: 'S256',
      silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
      checkLoginIframe: false,
    });

    this.isAuthenticated.set(authenticated);
    if (authenticated) {
      this.syncProfile();
      this.signalR.connect().catch(() => undefined);
    }
  }

  login(): Promise<void> {
    return this.keycloak.login();
  }

  logout(): Promise<void> {
    return this.keycloak.logout({ redirectUri: window.location.origin });
  }

  async getToken(): Promise<string | undefined> {
    await this.keycloak.updateToken(30);
    return this.keycloak.token;
  }

  hasRole(role: string): boolean {
    return this.roles().includes(role) || this.roles().includes('Admin');
  }

  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission) || this.hasRole('Admin');
  }

  private syncProfile(): void {
    const parsed = this.keycloak.tokenParsed as Record<string, unknown> | undefined;
    this.username.set((parsed?.['preferred_username'] as string) ?? null);
    this.tenant.set((parsed?.['tenant'] as string) ?? (parsed?.['company'] as string) ?? null);

    const realmRoles = (parsed?.['realm_access'] as { roles?: string[] })?.roles ?? [];
    const permissionClaims = parsed?.['permission'];
    const perms = Array.isArray(permissionClaims)
      ? permissionClaims.map(String)
      : permissionClaims
        ? [String(permissionClaims)]
        : [];

    this.roles.set(realmRoles);
    this.permissions.set(perms);
  }
}

export function authInitializer(auth: AuthService) {
  return () => auth.init();
}
