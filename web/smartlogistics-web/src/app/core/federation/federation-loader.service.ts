import { Injectable, signal } from '@angular/core';

export type RemoteName = 'shipment-remote' | 'tariff-remote';

/**
 * Module Federation shell loader.
 * When remotes run on :4201/:4202, the shell can delegate routes to federated bundles.
 * Install `@angular-architects/native-federation` in remotes and call `loadRemoteModule` here when ready.
 */
@Injectable({ providedIn: 'root' })
export class FederationLoaderService {
  private readonly manifestUrl = '/federation.manifest.json';
  readonly remotesAvailable = signal<Record<RemoteName, boolean>>({
    'shipment-remote': false,
    'tariff-remote': false,
  });

  async probeRemotes(): Promise<void> {
    try {
      const manifest = await fetch(this.manifestUrl).then((r) => r.json() as Promise<Record<RemoteName, string>>);
      const status: Record<RemoteName, boolean> = {
        'shipment-remote': false,
        'tariff-remote': false,
      };

      await Promise.all(
        (Object.keys(manifest) as RemoteName[]).map(async (name) => {
          try {
            await fetch(manifest[name], { method: 'GET', mode: 'cors' });
            status[name] = true;
          } catch {
            status[name] = false;
          }
        }),
      );

      this.remotesAvailable.set(status);
    } catch {
      /* shell runs standalone when manifest or remotes are offline */
    }
  }
}
