import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ApiService } from '../../core/api/api.service';

interface ChatMessage {
  role: 'user' | 'assistant';
  text: string;
}

@Component({
  selector: 'app-assistant',
  imports: [
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
  ],
  template: `
    <h1>Logistics AI Assistant</h1>
    <p class="hint">Ask about shipments, tariffs, containers, and invoices. Uses Azure OpenAI when configured; mock fallback otherwise.</p>
    <div class="chat">
      @for (msg of messages(); track $index) {
        <mat-card [class.user]="msg.role === 'user'">
          <strong>{{ msg.role === 'user' ? 'You' : 'Assistant' }}</strong>
          <pre>{{ msg.text }}</pre>
        </mat-card>
      }
    </div>
    @if (loading()) {
      <mat-progress-bar mode="indeterminate" />
    }
    <mat-form-field class="input">
      <mat-label>Your question</mat-label>
      <input matInput [(ngModel)]="question" (keyup.enter)="send()" [disabled]="loading()" />
    </mat-form-field>
    <button mat-raised-button color="primary" (click)="send()" [disabled]="loading() || !question.trim()">Send</button>
  `,
  styles: `
    .chat { display: flex; flex-direction: column; gap: 0.75rem; max-height: 50vh; overflow-y: auto; margin: 1rem 0; }
    mat-card.user { background: color-mix(in srgb, var(--mat-sys-primary) 12%, transparent); }
    pre { white-space: pre-wrap; margin: 0.5rem 0 0; font-family: inherit; }
    .input { width: 100%; max-width: 40rem; display: block; }
    .hint { color: #666; }
  `,
})
export class AssistantComponent {
  private readonly api = inject(ApiService);
  readonly messages = signal<ChatMessage[]>([]);
  readonly loading = signal(false);
  question = '';

  send(): void {
    const q = this.question.trim();
    if (!q || this.loading()) return;

    this.messages.update((m) => [...m, { role: 'user', text: q }]);
    this.question = '';
    this.loading.set(true);

    this.api.askAssistant(q).subscribe({
      next: (res) => {
        this.messages.update((m) => [...m, { role: 'assistant', text: res.answer }]);
        this.loading.set(false);
      },
      error: () => {
        this.messages.update((m) => [...m, { role: 'assistant', text: 'Assistant unavailable. Check gateway and ai-api.' }]);
        this.loading.set(false);
      },
    });
  }
}
