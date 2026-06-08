export interface MicrosoftAuthConfig {
  clientId: string;
  authority: string;
  redirectUri: string;
  scopes: string[];
  backendAuthUrl: string;
}

export const microsoftAuthConfig: MicrosoftAuthConfig = {
  clientId: 'ce989cd0-ae4f-4db8-8d93-9fbbf77be0fc',
  authority: 'https://login.microsoftonline.com/563a2444-2ef7-4863-8169-d4cefe814113',
  redirectUri: 'http://localhost:4200/auth',
  scopes: ['openid', 'profile', 'email', 'User.Read'],
  backendAuthUrl: 'http://localhost:5132/auth'
};
