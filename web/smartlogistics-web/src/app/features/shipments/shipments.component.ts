import { Component, inject, OnInit, signal } from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef } from 'ag-grid-community';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '../../core/api/api.service';

@Component({
  selector: 'app-shipments',
  imports: [AgGridAngular, MatButtonModule],
  template: `
    <h1>Shipments</h1>
    <button mat-raised-button color="primary" (click)="load()">Refresh</button>
    @if (error()) {
      <p class="error">{{ error() }}</p>
    }
    <ag-grid-angular
      class="ag-theme-quartz grid"
      [rowData]="rows()"
      [columnDefs]="columnDefs"
      [pagination]="true"
      [paginationPageSize]="10"
    />
  `,
  styles: `
    .grid { width: 100%; height: 420px; margin-top: 1rem; }
    .error { color: #c62828; }
  `,
})
export class ShipmentsComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly rows = signal<Record<string, unknown>[]>([]);
  readonly error = signal<string | null>(null);

  readonly columnDefs: ColDef[] = [
    { field: 'id', filter: true, sortable: true },
    { field: 'tenantId', headerName: 'Tenant', filter: true },
    { field: 'customerId', headerName: 'Customer', filter: true },
    { field: 'originPort', headerName: 'Origin' },
    { field: 'destinationPort', headerName: 'Destination' },
    { field: 'status', filter: true },
  ];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.error.set(null);
    this.api.getShipments().subscribe({
      next: (res) => this.rows.set(Array.isArray(res) ? res : []),
      error: (err) => this.error.set(err.message ?? 'Failed to load shipments'),
    });
  }
}
