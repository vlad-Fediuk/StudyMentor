import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import type { AccountInfo } from '@azure/msal-browser';
import { firstValueFrom } from 'rxjs';
import { microsoftAuthConfig } from '../../auth.config';
import { AuthService } from '../../data-access/auth.service';

@Component({
  selector: 'app-auth-redirect-page',
  standalone: true,
  templateUrl: './auth-redirect-page.component.html',
  styleUrl: './auth-redirect-page.component.scss'
})
export class AuthRedirectPageComponent implements OnInit {
  private readonly msalService = inject(MsalService);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  async ngOnInit(): Promise<void> {
    try {
      if (this.authService.getToken()) {
        await this.router.navigate(['/chat']);
        return;
      }

      const result = await this.msalService.instance.handleRedirectPromise();
      const account =
        result?.account ??
        this.msalService.instance.getActiveAccount() ??
        this.msalService.instance.getAllAccounts()[0] ??
        null;
      const idToken = result?.idToken ?? (await this.getIdTokenForAccount(account));

      if (account && idToken) {
        this.msalService.instance.setActiveAccount(account);
        await firstValueFrom(
          this.authService.authenticateWithMicrosoftIdToken(idToken, account)
        );
        await this.router.navigate(['/chat']);
      } else {
        this.redirectToLoginPageWithError('No account information was returned by Microsoft.');
      }
    } catch (error) {
      this.redirectToLoginPageWithError(this.getAuthenticationErrorMessage(error));
    }
  }

  private redirectToLoginPageWithError(error: string): void {
    this.router.navigate(['/login'], {
      queryParams: { error }
    });
  }

  private async getIdTokenForAccount(account: AccountInfo | null): Promise<string> {
    if (!account) {
      return '';
    }

    try {
      const result = await this.msalService.instance.acquireTokenSilent({
        account,
        scopes: microsoftAuthConfig.scopes
      });
      return result.idToken;
    } catch {
      return '';
    }
  }

  private getAuthenticationErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const backendMessage = typeof error.error?.message === 'string' ? error.error.message : '';
      return backendMessage || 'Backend authentication failed. Please try again.';
    }

    return 'Microsoft authentication failed. Please try again.';
  }
}
