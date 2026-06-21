import { JsonPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { MatStepperModule } from '@angular/material/stepper';
import { MatCardModule } from '@angular/material/card';
import * as L from 'leaflet';
import { ApiService } from '../../core/api/api.service';

@Component({
  selector: 'app-shipment-tracking',
  imports: [MatStepperModule, MatCardModule, JsonPipe],
  template: `
    <h1>Shipment Tracking</h1>
    <div #map class="map"></div>
    @if (tracking(); as t) {
      <h2>Timeline</h2>
      <pre>{{ t | json }}</pre>
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    }
  `,
  styles: `
    .map { height: 320px; border-radius: 8px; margin: 1rem 0; border: 1px solid #ddd; }
    .error { color: #c62828; }
    pre { background: #f5f5f5; padding: 1rem; border-radius: 8px; }
  `,
})
export class ShipmentTrackingComponent implements OnInit, AfterViewInit {
  private readonly api = inject(ApiService);
  private readonly mapRef = viewChild<ElementRef<HTMLDivElement>>('map');
  readonly tracking = signal<unknown>(null);
  readonly error = signal<string | null>(null);
  private readonly demoId = '11111111-1111-1111-1111-111111111111';

  ngOnInit(): void {
    this.api.getTracking(this.demoId).subscribe({
      next: (res) => this.tracking.set(res),
      error: (err) => this.error.set(err.message ?? 'Failed to load tracking'),
    });
  }

  ngAfterViewInit(): void {
    const el = this.mapRef()?.nativeElement;
    if (!el) return;

    const map = L.map(el).setView([13.75, 100.5], 3);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap',
    }).addTo(map);

    L.marker([13.75, 100.5]).addTo(map).bindPopup('BKK Origin');
    L.marker([34.05, -118.24]).addTo(map).bindPopup('LAX Destination');
    L.polyline([
      [13.75, 100.5],
      [20, 150],
      [34.05, -118.24],
    ]).addTo(map);
  }
}
