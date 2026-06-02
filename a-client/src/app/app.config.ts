import {
  APP_INITIALIZER,
  ApplicationConfig,
  importProvidersFrom,
  provideZoneChangeDetection
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideClientHydration } from '@angular/platform-browser';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import {
  BrowserCacheLocation,
  IPublicClientApplication,
  PublicClientApplication
} from '@azure/msal-browser';
import { MSAL_INSTANCE, MsalModule, MsalService } from '@azure/msal-angular';
import { routes } from './app.routes';
import { microsoftAuthConfig } from './features/auth/auth.config';
import { authTokenInterceptor } from './features/auth/data-access/auth-token.interceptor';

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: microsoftAuthConfig.clientId,
      authority: microsoftAuthConfig.authority,
      redirectUri: microsoftAuthConfig.redirectUri,
      navigateToLoginRequestUrl: false
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage,
      storeAuthStateInCookie: false
    }
  });
}

export async function initializeMsalInstance(
  msalInstance: IPublicClientApplication
): Promise<void> {
  await msalInstance.initialize();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(),
    provideHttpClient(withFetch(), withInterceptors([authTokenInterceptor])),
    importProvidersFrom([MsalModule]),
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory
    },
    {
      provide: APP_INITIALIZER,
      useFactory: (msalInstance: IPublicClientApplication) => {
        return () => initializeMsalInstance(msalInstance);
      },
      deps: [MSAL_INSTANCE],
      multi: true
    },
    MsalService
  ]
};
