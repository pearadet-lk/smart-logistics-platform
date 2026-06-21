import { Component, inject, OnInit, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ApiService, AuditLogRow } from '../../core/api/api.service';

@Component({
  selector: 'app-audit',
  imports: [MatTableModule, MatProgressBarModule],
  template: `
    <h1>Audit History</h1>
    @if (loading()) {
      <mat-progress-bar mode="indeterminate" />
    }
    <table mat-table [dataSource]="rows()" class="mat-elevation-z1">
      <ng-container matColumnDef="user">
        <th mat-header-cell *matHeaderCellDef>User</th>
        <td mat-cell *matCellDef="let row">{{ row.userId }}</td>
      </ng-container>
      <ng-container matColumnDef="action">
        <th mat-header-cell *matHeaderCellDef>Action</th>
        <td mat-cell *matCellDef="let row">{{ row.action }}</td>
      </ng-container>
      <ng-container matColumnDef="entity">
        <th mat-header-cell *matHeaderCellDef>Entity</th>
        <td mat-cell *matCellDef="let row">{{ row.entity }}</td>
      </ng-container>
      <ng-container matColumnDef="change">
        <th mat-header-cell *matHeaderCellDef>Change</th>
        <td mat-cell *matCellDef="let row">{{ row.newValue ?? row.oldValue ?? '—' }}</td>
      </ng-container>
      <ng-container matColumnDef="ip">
        <th mat-header-cell *matHeaderCellDef>IP</th>
        <td mat-cell *matCellDef="let row">{{ row.ipAddress ?? '—' }}</td>
      </ng-container>
      <ng-container matColumnDef="source">
        <th mat-header-cell *matHeaderCellDef>Source</th>
        <td mat-cell *matCellDef="let row">{{ row.source }}</td>
      </ng-container>
      <ng-container matColumnDef="trace">
        <th mat-header-cell *matHeaderCellDef>Trace</th>
        <td mat-cell *matCellDef="let row">{{ row.traceId ?? '—' }}</td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="columns"></tr>
      <tr mat-row *matRowDef="let row; columns: columns"></tr>
    </table>
  `,
  styles: `table { width: 100%; margin-top: 1rem; } td { font-size: 0.875rem; }`,
})
export class AuditComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly columns = ['user', 'action', 'entity', 'change', 'ip', 'source', 'trace'];
  readonly rows = signal<AuditLogRow[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.api.getAuditLogs().subscribe({
      next: (logs) => {
        this.rows.set(logs);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
