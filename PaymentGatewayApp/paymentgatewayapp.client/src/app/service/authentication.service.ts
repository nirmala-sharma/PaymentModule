import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import { ApiService } from './api.service';
import { TokenRequest_DTO } from '../DTOs/token-request.dto';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private isAuthenticated = false;
    private isLoggedInSubject = new BehaviorSubject<boolean>(!!localStorage.getItem('authToken'));
    isLoggedIn$ = this.isLoggedInSubject.asObservable();

    constructor(private router: Router, private apiService: ApiService) { }

    CheckIfUserAuthenticated(): boolean {
        const token = localStorage.getItem('accessToken');
        return this.isAuthenticated = token ? true : false;
    }

    setToken(accessToken: string, refreshToken: string) {
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
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

    getAccessToken() {
        return localStorage.getItem('accessToken');
    }
    getRefreshToken() {
        return localStorage.getItem('refreshToken');
    }
    FetchRefreshToken(TokenRequest: TokenRequest_DTO): Observable<any> {
        return this.apiService.getNewToken(TokenRequest).pipe(
            tap((response: any) => {
                if (response && response.accessToken) {
                    this.setToken(response.accessToken, response.refreshToken);
                } else {
                    alert('Invalid Token');
                }
            }),
            catchError((error) => {
                alert('Process failed!');
                return throwError(() => error);
            })
        );
    }
    clearTokens() {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    }
}