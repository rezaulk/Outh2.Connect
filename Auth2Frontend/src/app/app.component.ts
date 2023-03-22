import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { AuthenticationService } from './authentication.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'Auth2 Fronted ';
  user = { userID: '', userName: '', id: '', picture: '', email: '' };
  idToken = '';

  constructor(
    public authenticationService: AuthenticationService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    debugger;
    this.route.queryParamMap.subscribe((params: ParamMap) => {
      this.idToken = params.get('idToken') || '';
      if (this.idToken) {
        this.authenticationService.extractToken(this.idToken).subscribe((res) => {
          this.user = res;
        });

        this.authenticationService.verifyToken(this.idToken).subscribe({
          next: (data) => {
            console.log('This is verified by Google', data);
          },
          error: (error) => {
            console.log(error);
          },
        });
      }
    });
  }

  signInWithGoogle() {
    this.authenticationService.oidcLogin();
  }

  signInWithHDBIAM() {
    this.authenticationService.oidcHDBIAMLogin();
  }

  signInWithAAD() {
    this.authenticationService.oidcAADLogin();
  }
}
