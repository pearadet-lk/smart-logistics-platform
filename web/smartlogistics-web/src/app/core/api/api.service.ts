import { HttpClient, HttpEvent } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../config/environment';

export interface AuditLogRow {
  id: string;
  userId: string;
  action: string;
  entity: string;
  oldValue?: string;
  newValue?: string;
  ipAddress?: string;
  traceId?: string;
  createdDate: string;
  source: string;
}

export interface DashboardAnalytics {
  totalShipments: number;
  pendingShipments: number;
  inTransitShipments: number;
  revenueMtd: number;
  shipmentsByMonth: Array<{ year: number; month: number; count: number }>;
  revenueByMonth: Array<{ year: number; month: number; amount: number }>;
  topRoutes: Array<{ route: string; count: number }>;
  topCustomers: Array<{ customerId: string; count: number }>;
}

export interface TariffVersion {
  id: string;
  versionNo: number;
  status: string;
  tenantId: string;
}

export interface WorkflowResponse {
  instance: { id: string; status: string; entityId: string };
  history: Array<{ action: string; actor: string; comment?: string; createdDate: string }>;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiGatewayUrl;

  getShipments(): Observable<unknown> {
    return this.http.get(`${this.base}/api/shipments`);
  }

  getTracking(shipmentId: string): Observable<unknown> {
    return this.http.get(`${this.base}/api/shipments/${shipmentId}/tracking`);
  }

  getTariffs(): Observable<unknown> {
    return this.http.get(`${this.base}/api/tariffs`);
  }

  uploadTariff(file: File): Observable<HttpEvent<unknown>> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post(`${this.base}/api/tariffs/upload`, form, {
      reportProgress: true,
      observe: 'events',
    });
  }

  getAuditLogs(take = 100): Observable<AuditLogRow[]> {
    return this.http.get<AuditLogRow[]>(`${this.base}/api/audit`, { params: { take } });
  }

  getDashboard(): Observable<DashboardAnalytics> {
    return this.http.get<DashboardAnalytics>(`${this.base}/api/analytics/dashboard`);
  }

  getTariffWorkflow(tariffVersionId: string): Observable<WorkflowResponse> {
    return this.http.get<WorkflowResponse>(`${this.base}/api/tariffs/workflow/${tariffVersionId}`);
  }

  approveTariff(tariffVersionId: string, comment?: string): Observable<unknown> {
    return this.http.post(`${this.base}/api/tariffs/workflow/${tariffVersionId}/approve`, { comment });
  }

  rejectTariff(tariffVersionId: string, comment: string): Observable<unknown> {
    return this.http.post(`${this.base}/api/tariffs/workflow/${tariffVersionId}/reject`, { comment });
  }

  publishTariff(tariffVersionId: string): Observable<unknown> {
    return this.http.post(`${this.base}/api/tariffs/workflow/${tariffVersionId}/publish`, {});
  }

  askAssistant(question: string): Observable<{ question: string; answer: string }> {
    return this.http.post<{ question: string; answer: string }>(`${this.base}/api/ai/chat`, { question });
  }
}
