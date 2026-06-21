import { CurrencyPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts/core';
import { BarChart, LineChart } from 'echarts/charts';
import { GridComponent, LegendComponent, TooltipComponent } from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { AuthService } from '../../core/auth/auth.service';
import { ApiService, DashboardAnalytics } from '../../core/api/api.service';

echarts.use([BarChart, LineChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer]);

@Component({
  selector: 'app-dashboard',
  imports: [MatCardModule, MatProgressBarModule, NgxEchartsDirective, CurrencyPipe],
  providers: [provideEchartsCore({ echarts })],
  template: `
    <h1>Logistics Dashboard</h1>
    @if (loading()) {
      <mat-progress-bar mode="indeterminate" />
    }
    @if (data(); as d) {
      <div class="widgets">
        <mat-card><mat-card-title>Total Shipments</mat-card-title><mat-card-content>{{ d.totalShipments }}</mat-card-content></mat-card>
        <mat-card><mat-card-title>Pending</mat-card-title><mat-card-content>{{ d.pendingShipments }}</mat-card-content></mat-card>
        <mat-card><mat-card-title>In Transit</mat-card-title><mat-card-content>{{ d.inTransitShipments }}</mat-card-content></mat-card>
        <mat-card><mat-card-title>Revenue (MTD)</mat-card-title><mat-card-content>{{ d.revenueMtd | currency:'USD':'symbol':'1.0-0' }}</mat-card-content></mat-card>
      </div>
      <div class="charts">
        <div echarts [options]="shipmentsChart()" class="chart"></div>
        <div echarts [options]="revenueChart()" class="chart"></div>
      </div>
      <div class="lists">
        <mat-card>
          <mat-card-title>Top Routes</mat-card-title>
          <mat-card-content>
            <ul>
              @for (r of d.topRoutes; track r.route) {
                <li>{{ r.route }} — {{ r.count }}</li>
              }
            </ul>
          </mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-title>Top Customers</mat-card-title>
          <mat-card-content>
            <ul>
              @for (c of d.topCustomers; track c.customerId) {
                <li>{{ c.customerId }} — {{ c.count }}</li>
              }
            </ul>
          </mat-card-content>
        </mat-card>
      </div>
    }
    <p class="meta">Tenant: {{ auth.tenant() ?? 'All' }} | Roles: {{ auth.roles().join(', ') }}</p>
  `,
  styles: `
    .widgets { display: grid; gap: 1rem; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); margin: 1rem 0; }
    mat-card-content { font-size: 2rem; font-weight: 600; }
    .charts { display: grid; gap: 1rem; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); }
    .chart { height: 280px; }
    .lists { display: grid; gap: 1rem; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); margin-top: 1rem; }
    ul { margin: 0; padding-left: 1.25rem; }
    .meta { margin-top: 2rem; color: #666; }
  `,
})
export class DashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);
  readonly loading = signal(true);
  readonly data = signal<DashboardAnalytics | null>(null);
  readonly shipmentsChart = signal<echarts.EChartsCoreOption>({});
  readonly revenueChart = signal<echarts.EChartsCoreOption>({});

  ngOnInit(): void {
    this.api.getDashboard().subscribe({
      next: (d) => {
        this.data.set(d);
        this.loading.set(false);
        const months = d.shipmentsByMonth.map((x) => `${x.month}/${x.year}`);
        this.shipmentsChart.set({
          title: { text: 'Shipments by Month' },
          xAxis: { type: 'category', data: months },
          yAxis: { type: 'value' },
          series: [{ type: 'bar', data: d.shipmentsByMonth.map((x) => x.count) }],
        });
        this.revenueChart.set({
          title: { text: 'Revenue Trend' },
          tooltip: { trigger: 'axis' },
          xAxis: { type: 'category', data: months },
          yAxis: { type: 'value' },
          series: [{ type: 'line', data: d.revenueByMonth.map((x) => x.amount), smooth: true }],
        });
      },
      error: () => this.loading.set(false),
    });
  }
}
