import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { AuthService } from '../../data-access/auth.service';
import { MicrosoftLoginButtonComponent } from '../../ui/microsoft-login-button/microsoft-login-button.component';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [MicrosoftLoginButtonComponent],
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.scss'
})
export class AuthPageComponent {
  readonly auth = inject(AuthService);
  private readonly msalService = inject(MsalService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  constructor() {
    const error = this.route.snapshot.queryParamMap.get('error');

    if (error) {
      this.auth.setError(error);
      return;
    }

    if (this.auth.getToken()) {
      this.router.navigate(['/chat']);
    }
  }

  loginSSO(): void {
    this.auth.startMicrosoftLogin();
    this.msalService.loginRedirect().subscribe({
      error: () => {
        this.auth.setError('Microsoft authentication failed. Please try again.');
      }
    });
  }
}

