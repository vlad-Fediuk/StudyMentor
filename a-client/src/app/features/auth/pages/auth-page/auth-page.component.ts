import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../data-access/auth.service';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.scss'
})
export class AuthPageComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  isRegisterMode = false;
  name = '';
  email = '';
  password = '';

  constructor() {
    if (this.auth.getToken()) {
      this.router.navigate(['/chat']);
    }
  }

  setMode(register: boolean): void {
    this.isRegisterMode = register;
    this.auth.setError('');
  }

  submit(): void {
    if (!this.email || !this.password) {
      return;
    }

    if (this.isRegisterMode) {
      this.auth.register({
        email: this.email,
        name: this.name,
        password: this.password
      }).subscribe({
        next: () => this.router.navigate(['/chat'])
      });
    } else {
      this.auth.login({
        email: this.email,
        password: this.password
      }).subscribe({
        next: () => this.router.navigate(['/chat'])
      });
    }
  }
}
