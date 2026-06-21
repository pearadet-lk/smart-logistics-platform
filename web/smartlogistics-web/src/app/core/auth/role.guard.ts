import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const roleGuard = (roles: string[]): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (roles.some((r) => auth.hasRole(r))) {
    return true;
  }

  return router.parseUrl('/dashboard');
};

export const permissionGuard = (permissions: string[]): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (permissions.some((p) => auth.hasPermission(p))) {
    return true;
  }

  return router.parseUrl('/dashboard');
};
