import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../service/authentication.service';
import { environment } from '../../environments/environment';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.component.html',
    styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit {
    AppName: string = environment.appName;
    IsLoggedIn: boolean = false;
    constructor(private authService: AuthService, private router: Router) {
        this.authService.isLoggedIn$.subscribe(status => {
            this.IsLoggedIn = status;
        });
    }

    ngOnInit(): void {
    }
    login() {
        this.router.navigate(['/login']);
    }
    logout() {
        this.authService.logout();
        this.router.navigate(['/login']);
    }

}