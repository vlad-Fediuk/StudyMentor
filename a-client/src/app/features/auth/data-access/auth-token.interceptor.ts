import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { microsoftAuthConfig } from '../auth.config';
import { AuthService } from './auth.service';

const backendApiOrigin = new URL(microsoftAuthConfig.backendAuthUrl).origin;

export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(backendApiOrigin)) {
    return next(req);
  }

  const token = inject(AuthService).getToken();
  if (!token) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    })
  );
};
