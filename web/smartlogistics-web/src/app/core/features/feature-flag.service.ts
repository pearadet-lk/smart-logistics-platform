import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../config/environment';
import { FeatureFlagStore } from '../stores/feature-flag.store';

@Injectable({ providedIn: 'root' })
export class FeatureFlagService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(FeatureFlagStore);

  load() {
    return this.http
      .get<{
        newTariffScreen: boolean;
        billingModule: boolean;
        aiAssistant: boolean;
        pwaOfflineLookups: boolean;
      }>(`${environment.apiGatewayUrl}/api/feature-flags`)
      .pipe(
        tap((flags) =>
          this.store.setFlags({
            newTariffScreen: flags.newTariffScreen,
            billingModule: flags.billingModule,
            aiAssistant: flags.aiAssistant,
            pwaOfflineLookups: flags.pwaOfflineLookups,
          }),
        ),
      );
  }
}
