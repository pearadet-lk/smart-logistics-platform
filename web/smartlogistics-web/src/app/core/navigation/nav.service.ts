import { Injectable, computed, inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { FeatureFlagStore } from '../stores/feature-flag.store';
import { NAV_ITEMS, NavItem } from './nav-config';

@Injectable({ providedIn: 'root' })
export class NavService {
  private readonly auth = inject(AuthService);
  private readonly flags = inject(FeatureFlagStore);

  readonly visibleItems = computed(() =>
    NAV_ITEMS.filter((item) => this.canSee(item)),
  );

  private canSee(item: NavItem): boolean {
    const roleOk =
      !item.roles?.length || item.roles.some((r) => this.auth.hasRole(r));
    const permOk =
      !item.permissions?.length ||
      item.permissions.some((p) => this.auth.hasPermission(p));
    const flagOk =
      !item.featureFlag || this.flags.flags()[item.featureFlag];
    return roleOk && permOk && flagOk;
  }
}
