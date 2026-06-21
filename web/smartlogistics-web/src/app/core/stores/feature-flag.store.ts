import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';

export interface FeatureFlags {
  newTariffScreen: boolean;
  billingModule: boolean;
  aiAssistant: boolean;
  pwaOfflineLookups: boolean;
}

const defaults: FeatureFlags = {
  newTariffScreen: true,
  billingModule: true,
  aiAssistant: false,
  pwaOfflineLookups: true,
};

export const FeatureFlagStore = signalStore(
  { providedIn: 'root' },
  withState({ flags: defaults as FeatureFlags, loaded: false }),
  withMethods((store) => ({
    setFlags(flags: FeatureFlags) {
      patchState(store, { flags, loaded: true });
    },
  })),
);
