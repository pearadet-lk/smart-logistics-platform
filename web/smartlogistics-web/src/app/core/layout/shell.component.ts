import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../auth/auth.service';
import { NavService } from '../navigation/nav.service';
import { ThemeService } from '../theme/theme.service';
import { NotificationStore } from '../stores/notification.store';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatMenuModule,
    TranslocoPipe,
  ],
  template: `
    <mat-toolbar color="primary">
      <span>{{ 'app.title' | transloco }}</span>
      <span class="spacer"></span>
      <button mat-button [matMenuTriggerFor]="langMenu">🌐</button>
      <mat-menu #langMenu="matMenu">
        <button mat-menu-item (click)="setLang('en')">English</button>
        <button mat-menu-item (click)="setLang('th')">ไทย</button>
      </mat-menu>
      <button mat-icon-button (click)="theme.toggle()" [attr.aria-label]="theme.darkMode() ? 'Light' : 'Dark'">
        <mat-icon>{{ theme.darkMode() ? 'light_mode' : 'dark_mode' }}</mat-icon>
      </button>
      @if (auth.isAuthenticated()) {
        <button mat-icon-button [matMenuTriggerFor]="notifMenu" [matBadge]="notifications.unreadCount()" matBadgeColor="warn">
          <mat-icon>notifications</mat-icon>
        </button>
        <mat-menu #notifMenu="matMenu">
          @for (n of notifications.items(); track n.timestamp) {
            <button mat-menu-item disabled>{{ n.title }}: {{ n.message }}</button>
          } @empty {
            <button mat-menu-item disabled>No notifications</button>
          }
        </mat-menu>
        <span class="user">{{ auth.username() }} ({{ auth.tenant() ?? '—' }})</span>
        <button mat-button (click)="auth.logout()">{{ 'actions.logout' | transloco }}</button>
      } @else {
        <button mat-button (click)="auth.login()">{{ 'actions.login' | transloco }}</button>
      }
    </mat-toolbar>

    <mat-sidenav-container class="container">
      @if (auth.isAuthenticated()) {
        <mat-sidenav mode="side" opened class="sidenav">
          <mat-nav-list>
            @for (item of nav.visibleItems(); track item.route) {
              <a mat-list-item [routerLink]="item.route" routerLinkActive="active">
                <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
                <span matListItemTitle>{{ item.titleKey | transloco }}</span>
              </a>
            }
          </mat-nav-list>
        </mat-sidenav>
      }
      <mat-sidenav-content class="content">
        <router-outlet />
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: `
    .spacer { flex: 1; }
    .container { height: calc(100vh - 64px); }
    .sidenav { width: 240px; }
    .content { padding: 1.5rem; }
    .user { margin-right: 1rem; font-size: 0.9rem; }
    a.active { background: rgba(0, 0, 0, 0.04); }
    :host-context(.dark-theme) a.active { background: rgba(255, 255, 255, 0.08); }
  `,
})
export class ShellComponent {
  readonly auth = inject(AuthService);
  readonly nav = inject(NavService);
  readonly theme = inject(ThemeService);
  readonly notifications = inject(NotificationStore);
  private readonly transloco = inject(TranslocoService);

  setLang(lang: string): void {
    this.transloco.setActiveLang(lang);
  }
}
