import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-microsoft-login-button',
  standalone: true,
  templateUrl: './microsoft-login-button.component.html',
  styleUrl: './microsoft-login-button.component.scss'
})
export class MicrosoftLoginButtonComponent {
  @Input() loading = false;
  @Output() readonly login = new EventEmitter<void>();
}

