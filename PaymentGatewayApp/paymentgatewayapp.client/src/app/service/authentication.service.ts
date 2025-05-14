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
    private isLoggedInSubject = new BehaviorSubject<boolean>(!!sessionStorage.getItem('authToken'));
    isLoggedIn$ = this.isLoggedInSubject.asObservable();

    constructor(private router: Router, private apiService: ApiService) { }

    CheckIfUserAuthenticated(): boolean {
        const token = sessionStorage.getItem('accessToken');
        return this.isAuthenticated = token ? true : false;
    }

    setToken(accessToken: string) {
        sessionStorage.setItem('accessToken', accessToken);
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
        return sessionStorage.getItem('accessToken');
    }
    FetchRefreshToken(TokenRequest: TokenRequest_DTO): Observable<any> {
        return this.apiService.getNewToken(TokenRequest).pipe(
            tap((response: any) => {
                if (response) {
                    this.setToken(response);
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
        sessionStorage.removeItem('accessToken');
    }
}