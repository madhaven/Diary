import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { App } from '@components/app/app';
import { appRoutes } from 'app.routes';

bootstrapApplication(App, {
  providers: [
    importProvidersFrom(FormsModule),
    provideHttpClient(withInterceptorsFromDi()),
    provideRouter(appRoutes)
  ]
}).catch(err => console.error(err));