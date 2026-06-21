import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly darkMode = signal(localStorage.getItem('theme') === 'dark');

  constructor() {
    this.apply();
  }

  toggle(): void {
    this.darkMode.update((v) => !v);
    localStorage.setItem('theme', this.darkMode() ? 'dark' : 'light');
    this.apply();
  }

  private apply(): void {
    document.body.classList.toggle('dark-theme', this.darkMode());
  }
}
