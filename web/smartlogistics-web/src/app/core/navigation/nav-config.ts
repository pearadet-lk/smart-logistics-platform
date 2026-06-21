export interface NavItem {
  titleKey: string;
  route: string;
  icon: string;
  roles?: string[];
  permissions?: string[];
  featureFlag?: keyof import('../stores/feature-flag.store').FeatureFlags;
}

export const NAV_ITEMS: NavItem[] = [
  { titleKey: 'nav.dashboard', route: '/dashboard', icon: 'dashboard' },
  {
    titleKey: 'nav.shipments',
    route: '/shipments',
    icon: 'local_shipping',
    permissions: ['Shipment.Read'],
  },
  {
    titleKey: 'nav.tracking',
    route: '/shipments/tracking',
    icon: 'timeline',
    permissions: ['Shipment.Read'],
  },
  {
    titleKey: 'nav.tariffs',
    route: '/tariffs',
    icon: 'request_quote',
    permissions: ['Tariff.Read'],
  },
  {
    titleKey: 'nav.approval',
    route: '/tariffs/approval',
    icon: 'approval',
    permissions: ['Tariff.Approve'],
    roles: ['Admin', 'Finance'],
  },
  {
    titleKey: 'nav.admin',
    route: '/admin',
    icon: 'admin_panel_settings',
    roles: ['Admin'],
  },
  {
    titleKey: 'nav.audit',
    route: '/audit',
    icon: 'history',
    roles: ['Admin', 'Finance', 'Operations'],
  },
  {
    titleKey: 'nav.assistant',
    route: '/assistant',
    icon: 'smart_toy',
    permissions: ['Shipment.Read'],
    featureFlag: 'aiAssistant',
  },
];
