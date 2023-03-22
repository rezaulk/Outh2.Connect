import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

import jwtDecode from 'jwt-decode';
import { Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  constructor(private http: HttpClient, private router: Router) {}

  oidcLogin(): void {
    const client: google.accounts.oauth2.CodeClient = google.accounts.oauth2.initCodeClient({
      client_id: environment.clientId,
      scope: 'openid profile email',
      ux_mode: 'redirect',
      redirect_uri: environment.apiUrl + '/api/Auth/oidc/signin',
      state: '12345GG',
    });
    client.requestCode();
  }

  oidcHDBIAMLogin(): void {
    this.http
      .get(environment.apiUrl + '/api/Auth/oidcHDBIAMAuthorizeCall/HDBIAMAuthorize', {
        responseType: 'text',
      })
      .subscribe(
        (url) => {
          window.location.href = url;
        },
        (error) => {
          console.error('HTTP call failed', error);
        }
      );
  }

  oidcAADLogin(): void {
    this.http
      .get(environment.apiUrl + '/api/Auth/oidcAADAuthorizeCall/AADAuthorize', {
        responseType: 'text',
      })
      .subscribe(
        (url) => {
          window.location.href = url;
        },
        (error) => {
          console.error('HTTP call failed', error);
        }
      );
  }

  extractToken(idToken: string): Observable<{
    userID: string;
    userName: string;
    id: string;
    picture: string;
    email: string;
  }> {
    const { sub: userID, name: userName, id: id, picture, email } = jwtDecode<any>(idToken);
    console.log(idToken);
    return of({
      userID,
      userName,
      id,
      picture,
      email,
    });
  }

  verifyToken(idToken: string): Observable<unknown> {
    return this.http.get('https://oauth2.googleapis.com/tokeninfo', {
      params: { id_token: idToken },
    });
  }
}
