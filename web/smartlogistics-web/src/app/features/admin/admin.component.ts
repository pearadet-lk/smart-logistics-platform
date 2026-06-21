import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-admin',
  imports: [MatCardModule],
  template: `
    <h1>Admin Portal</h1>
    <mat-card>
      <mat-card-title>User Management</mat-card-title>
      <mat-card-content>Keycloak Admin Console integration placeholder</mat-card-content>
    </mat-card>
    <mat-card>
      <mat-card-title>Feature Flags</mat-card-title>
      <mat-card-content>Enable/disable modules at runtime (placeholder)</mat-card-content>
    </mat-card>
  `,
  styles: `mat-card { margin-bottom: 1rem; }`,
})
export class AdminComponent {}
