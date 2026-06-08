import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { firstValueFrom } from 'rxjs';
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
      const result = await this.msalService.instance.handleRedirectPromise();

      if (result && result.idToken) {
        await firstValueFrom(
          this.authService.authenticateWithMicrosoftIdToken(result.idToken, result.account)
        );
        this.msalService.instance.setActiveAccount(result.account);
        this.router.navigate(['/chat']);
      } else {
        this.redirectToLoginPageWithError('No account information was returned by Microsoft.');
      }
    } catch (_error) {
      this.redirectToLoginPageWithError('Microsoft authentication failed. Please try again.');
    }
  }

  private redirectToLoginPageWithError(error: string): void {
    this.router.navigate(['/login'], {
      queryParams: { error }
    });
  }
}
