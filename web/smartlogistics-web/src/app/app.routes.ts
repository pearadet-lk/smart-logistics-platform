import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { permissionGuard, roleGuard } from './core/auth/role.guard';
import { ShellComponent } from './core/layout/shell.component';
import { HomeComponent } from './features/home/home.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ShipmentsComponent } from './features/shipments/shipments.component';
import { ShipmentTrackingComponent } from './features/shipments/shipment-tracking.component';
import { TariffsComponent } from './features/tariffs/tariffs.component';
import { TariffApprovalComponent } from './features/tariffs/tariff-approval.component';
import { AdminComponent } from './features/admin/admin.component';
import { AuditComponent } from './features/audit/audit.component';
import { AssistantComponent } from './features/assistant/assistant.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
      {
        path: 'shipments',
        component: ShipmentsComponent,
        canActivate: [authGuard, permissionGuard(['Shipment.Read'])],
      },
      {
        path: 'shipments/tracking',
        component: ShipmentTrackingComponent,
        canActivate: [authGuard, permissionGuard(['Shipment.Read'])],
      },
      {
        path: 'tariffs',
        component: TariffsComponent,
        canActivate: [authGuard, permissionGuard(['Tariff.Read'])],
      },
      {
        path: 'tariffs/approval',
        component: TariffApprovalComponent,
        canActivate: [authGuard, permissionGuard(['Tariff.Approve'])],
      },
      {
        path: 'admin',
        component: AdminComponent,
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'audit',
        component: AuditComponent,
        canActivate: [authGuard, roleGuard(['Admin', 'Finance', 'Operations'])],
      },
      {
        path: 'assistant',
        component: AssistantComponent,
        canActivate: [authGuard],
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
