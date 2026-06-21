import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ApiService, TariffVersion, WorkflowResponse } from '../../core/api/api.service';

@Component({
  selector: 'app-tariff-approval',
  imports: [MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule, FormsModule, DatePipe],
  template: `
    <h1>Tariff Approval Workflow</h1>
    @if (workflow(); as wf) {
      <p>Status: <strong>{{ wf.instance.status }}</strong></p>
      <div class="flow">
        @for (step of steps; track step) {
          <mat-card [class.active]="wf.instance.status === step">{{ step }}</mat-card>
          @if (!$last) {
            <span>→</span>
          }
        }
      </div>
      <mat-form-field class="comment">
        <mat-label>Comment</mat-label>
        <textarea matInput rows="2" [(ngModel)]="comment"></textarea>
      </mat-form-field>
      <div class="actions">
        <button mat-raised-button color="primary" (click)="approve()" [disabled]="busy()">Approve</button>
        <button mat-raised-button color="warn" (click)="reject()" [disabled]="busy()">Reject</button>
        <button mat-stroked-button (click)="publish()" [disabled]="busy()">Publish</button>
      </div>
      @if (wf.history.length) {
        <h3>History</h3>
        <ul>
          @for (h of wf.history; track h.createdDate) {
            <li>{{ h.createdDate | date: 'short' }} — {{ h.actor }}: {{ h.action }} {{ h.comment ?? '' }}</li>
          }
        </ul>
      }
    } @else {
      <p>Loading workflow…</p>
    }
  `,
  styles: `
    .flow { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; margin: 1rem 0; }
    mat-card.active { border: 2px solid var(--mat-sys-primary); }
    .actions { display: flex; gap: 1rem; margin-top: 1rem; }
    .comment { width: 100%; max-width: 32rem; display: block; margin-top: 1rem; }
  `,
})
export class TariffApprovalComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly workflow = signal<WorkflowResponse | null>(null);
  readonly busy = signal(false);
  readonly steps = ['Draft', 'Submitted', 'Approved', 'Published'];
  comment = '';
  private tariffId = '';

  ngOnInit(): void {
    this.api.getTariffs().subscribe({
      next: (tariffs) => {
        const list = tariffs as TariffVersion[];
        const first = list[0];
        if (!first) return;
        this.tariffId = first.id;
        this.loadWorkflow();
      },
    });
  }

  private loadWorkflow(): void {
    this.api.getTariffWorkflow(this.tariffId).subscribe({
      next: (wf) => this.workflow.set(wf),
    });
  }

  approve(): void {
    this.run(() => this.api.approveTariff(this.tariffId, this.comment || undefined));
  }

  reject(): void {
    this.run(() => this.api.rejectTariff(this.tariffId, this.comment || 'Rejected'));
  }

  publish(): void {
    this.run(() => this.api.publishTariff(this.tariffId));
  }

  private run(action: () => ReturnType<ApiService['approveTariff']>): void {
    this.busy.set(true);
    action().subscribe({
      next: () => {
        this.busy.set(false);
        this.loadWorkflow();
      },
      error: () => this.busy.set(false),
    });
  }
}
