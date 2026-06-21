import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';

export interface AppNotification {
  type: string;
  title: string;
  message: string;
  timestamp: string;
}

interface NotificationState {
  items: AppNotification[];
}

const initialState: NotificationState = { items: [] };

export const NotificationStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ items }) => ({
    unreadCount: computed(() => items().length),
  })),
  withMethods((store) => ({
    push(notification: AppNotification) {
      patchState(store, { items: [notification, ...store.items()].slice(0, 20) });
    },
    clear() {
      patchState(store, { items: [] });
    },
  })),
);
