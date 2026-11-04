import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import type { AccountInfo } from '@azure/msal-browser';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { microsoftAuthConfig } from '../auth.config';

export type AuthStatus = 'idle' | 'loading' | 'authenticated' | 'error';

export interface AuthState {
  status: AuthStatus;
  accountName: string;
  jwtToken: string;
  error: string;
}

export interface AuthResponse {
  token: string;
  name?: string;
  email?: string;
}

export interface RegisterDto {
  email: string;
  name: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

const emptyState: AuthState = {
  status: 'idle',
  accountName: '',
  jwtToken: '',
  error: ''
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private static readonly TokenStorageKey = 'studyMentorJwtToken';
  private static readonly TokenExpiresAtStorageKey = 'studyMentorJwtTokenExpiresAt';
  private static readonly AccountNameStorageKey = 'studyMentorAccountName';
  private static readonly MonthInMs = 30 * 24 * 60 * 60 * 1000;

  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);
  private readonly http = inject(HttpClient);

  readonly state = signal<AuthState>(this.readStoredState());

  startMicrosoftLogin(): void {
    this.state.set({ ...emptyState, status: 'loading' });
  }

  authenticateWithMicrosoftIdToken(
    microsoftIdToken: string,
    account: AccountInfo | null
  ): Observable<AuthResponse> {
    const accountName = account?.name || account?.username || 'Microsoft account';

    return this.http
      .post<AuthResponse>(microsoftAuthConfig.backendAuthUrl, { token: microsoftIdToken })
      .pipe(tap((response) => this.completeBackendLogin(response.token, accountName)));
  }

  register(dto: RegisterDto): Observable<AuthResponse> {
    this.state.set({ ...emptyState, status: 'loading' });
    const url = `${environment.apiUrl}/auth/register`;
    return this.http.post<AuthResponse>(url, dto).pipe(
      tap({
        next: (res) => this.completeBackendLogin(res.token, res.name || dto.name || dto.email),
        error: (err) => {
          const msg = err?.error?.message || 'Помилка реєстрації. Перевірте введені дані.';
          this.setError(msg);
        }
      })
    );
  }

  login(dto: LoginDto): Observable<AuthResponse> {
    this.state.set({ ...emptyState, status: 'loading' });
    const url = `${environment.apiUrl}/auth/login`;
    return this.http.post<AuthResponse>(url, dto).pipe(
      tap({
        next: (res) => this.completeBackendLogin(res.token, res.name || res.email || dto.email),
        error: (err) => {
          const msg = err?.error?.message || 'Невірний email або пароль.';
          this.setError(msg);
        }
      })
    );
  }

  signOut(): void {
    this.clearStoredAuth();
    this.state.set(emptyState);
  }

  getToken(): string {
    const storedState = this.readStoredState();
    if (storedState.status !== 'authenticated') {
      this.state.set(emptyState);
      return '';
    }

    this.state.set(storedState);
    return storedState.jwtToken;
  }

  setError(error: string): void {
    this.state.set({
      ...emptyState,
      status: 'error',
      error
    });
  }

  private readStoredState(): AuthState {
    if (!this.isBrowser) {
      return emptyState;
    }

    const jwtToken = localStorage.getItem(AuthService.TokenStorageKey) ?? '';
    const expiresAt = Number(localStorage.getItem(AuthService.TokenExpiresAtStorageKey) ?? 0);
    const accountName = localStorage.getItem(AuthService.AccountNameStorageKey) ?? '';

    if (!jwtToken || !expiresAt || Date.now() >= expiresAt) {
      this.clearStoredAuth();
      return emptyState;
    }

    return {
      status: 'authenticated',
      accountName,
      jwtToken,
      error: ''
    };
  }

  private completeBackendLogin(jwtToken: string, accountName: string): void {
    const expiresAt = Date.now() + AuthService.MonthInMs;

    if (this.isBrowser) {
      localStorage.setItem(AuthService.TokenStorageKey, jwtToken);
      localStorage.setItem(AuthService.TokenExpiresAtStorageKey, expiresAt.toString());
      localStorage.setItem(AuthService.AccountNameStorageKey, accountName);
    }

    this.state.set({
      status: 'authenticated',
      accountName,
      jwtToken,
      error: ''
    });
  }

  private clearStoredAuth(): void {
    if (this.isBrowser) {
      localStorage.removeItem(AuthService.TokenStorageKey);
      localStorage.removeItem(AuthService.TokenExpiresAtStorageKey);
      localStorage.removeItem(AuthService.AccountNameStorageKey);
    }
  }
}
