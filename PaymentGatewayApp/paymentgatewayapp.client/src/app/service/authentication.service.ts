import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private isAuthenticated = false;
    private isLoggedInSubject = new BehaviorSubject<boolean>(!!localStorage.getItem('authToken'));
    isLoggedIn$ = this.isLoggedInSubject.asObservable();

    constructor(private router: Router) { }

    CheckIfUserAuthenticated(): boolean {
        const token = localStorage.getItem('authToken');
        return this.isAuthenticated = token ? true : false;
    }

    login(token: string) {
        localStorage.setItem('authToken', token);
        this.isLoggedInSubject.next(true);
    }
    logout(): void {
        this.isAuthenticated = false;
        this.isLoggedInSubject.next(false);
        this.router.navigate(['/login']);
    }

    isLoggedIn(): boolean {
        return this.isAuthenticated;
    }

    getToken() {
        return localStorage.getItem('authToken');
    }
}