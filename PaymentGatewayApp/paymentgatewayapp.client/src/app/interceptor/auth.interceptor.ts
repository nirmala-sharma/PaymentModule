import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from "rxjs";
import { TokenRequest_DTO } from "../DTOs/token-request.dto";
import { AuthService } from "../service/authentication.service";
import { ApiService } from "../service/api.service";
import { Injectable } from "@angular/core";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    private isRefreshing = false;
    private refreshTokenSubject = new BehaviorSubject<string | null>(null);

    constructor(private authService: AuthService, private apiService: ApiService) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const accessToken = this.authService.getAccessToken();

        // Clone and attach access token
        let authReq = req;
        if (accessToken) {
            authReq = req.clone({
                setHeaders: {
                    Authorization: `Bearer ${accessToken}`
                }
            });
        }

        // Proceed with request
        return next.handle(authReq).pipe(
            catchError(error => {
                if (error instanceof HttpErrorResponse && error.status === 401) {
                    return this.handle401Error(authReq, next);
                } else {
                    return throwError(() => error);
                }
            })
        );
    }

    private handle401Error(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (!this.isRefreshing) {
            this.isRefreshing = true;
            this.refreshTokenSubject.next(null);

            const tokenRequest: TokenRequest_DTO = {
                AccessToken: this.authService.getAccessToken(),
                RefreshToken: this.authService.getRefreshToken()
            };

            return this.authService.FetchRefreshToken(tokenRequest).pipe(
                switchMap((res: any) => {
                    if (res && res.accessToken) {
                        this.isRefreshing = false;
                        this.authService.setToken(res.accessToken, res.refreshToken); // Save new tokens
                        this.refreshTokenSubject.next(res.accessToken);

                        // Retry the original request with new token
                        const retryReq = req.clone({
                            setHeaders: {
                                Authorization: `Bearer ${res.accessToken}`
                            }
                        });

                        return next.handle(retryReq);
                    } else {
                        // Invalid response - maybe no access token
                        this.handleAuthError();
                        return throwError(() => new Error('Invalid token refresh response'));
                    }
                })
            ).pipe(
                catchError(err => {
                    // This catchError only triggers if refresh fails
                    this.handleAuthError();
                    return throwError(() => err);
                })
            );
        } else {
            // Wait for the refreshing process to complete
            return this.refreshTokenSubject.pipe(
                filter(token => token !== null),
                take(1),
                switchMap((token) => {
                    const retryReq = req.clone({
                        setHeaders: {
                            Authorization: `Bearer ${token}`
                        }
                    });
                    return next.handle(retryReq);
                })
            );
        }
    }

    private handleAuthError() {
        this.authService.clearTokens();
        window.location.href = '/login';
    }
}