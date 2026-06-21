import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../config/environment';
import { AuthService } from '../auth/auth.service';
import { NotificationStore } from '../stores/notification.store';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationStore);
  private hub?: signalR.HubConnection;

  async connect(): Promise<void> {
    if (!this.auth.isAuthenticated() || this.hub) return;

    const token = await this.auth.getToken();
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiGatewayUrl}/hubs/notifications`, {
        accessTokenFactory: () => token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('ReceiveNotification', (payload: { type: string; title: string; message: string; timestamp: string }) => {
      this.notifications.push(payload);
    });

    await this.hub.start();

    const tenant = this.auth.tenant();
    if (tenant) {
      await this.hub.invoke('SubscribeToTenant', tenant);
    }
  }

  async disconnect(): Promise<void> {
    await this.hub?.stop();
    this.hub = undefined;
  }
}
