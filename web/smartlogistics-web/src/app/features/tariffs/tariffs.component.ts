import { JsonPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpEventType } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ApiService } from '../../core/api/api.service';
import { FeatureFlagStore } from '../../core/stores/feature-flag.store';

@Component({
  selector: 'app-tariffs',
  imports: [MatCardModule, MatButtonModule, MatProgressBarModule, JsonPipe],
  template: `
    <h1>Tariff Versions</h1>
    @if (flags.flags().newTariffScreen) {
      <mat-card class="upload">
        <mat-card-title>Upload Excel Tariff</mat-card-title>
        <input type="file" accept=".csv,.xlsx,.json" (change)="onFile($event)" />
        @if (uploadProgress() >= 0) {
          <mat-progress-bar mode="determinate" [value]="uploadProgress()"></mat-progress-bar>
        }
      </mat-card>
    }
    <button mat-raised-button (click)="load()">Load versions</button>
    <pre>{{ data() | json }}</pre>
  `,
  styles: `
    .upload { margin-bottom: 1rem; padding: 1rem; }
    pre { margin-top: 1rem; background: #f5f5f5; padding: 1rem; border-radius: 8px; }
  `,
})
export class TariffsComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly flags = inject(FeatureFlagStore);
  readonly data = signal<unknown>(null);
  readonly uploadProgress = signal(-1);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getTariffs().subscribe((res) => this.data.set(res));
  }

  onFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.api.uploadTariff(file).subscribe({
      next: (evt) => {
        if (evt.type === HttpEventType.UploadProgress && evt.total) {
          this.uploadProgress.set(Math.round((100 * evt.loaded) / evt.total));
        }
        if (evt.type === HttpEventType.Response) {
          this.uploadProgress.set(-1);
          this.load();
        }
      },
    });
  }
}
