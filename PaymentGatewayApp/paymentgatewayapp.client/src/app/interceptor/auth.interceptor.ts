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
       const idempotencyKey = sessionStorage.getItem('idempotencyKey');
        let headers: { [name: string]: string } = {};
        // Clone and attach access token
        if (accessToken) {
            headers['Authorization'] = `Bearer ${accessToken}`;
        }
        const isPaymentPost = req.method === 'POST' &&
            req.url.includes('/Payment/ProcessPayment');

        if (idempotencyKey && isPaymentPost) {
            headers['Idempotency-Key'] = idempotencyKey;
        }
        const authReq = req.clone({
            setHeaders: headers
        });

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
                AccessToken: this.authService.getAccessToken()
            };

            return this.authService.FetchRefreshToken(tokenRequest).
                pipe(
                    catchError(err => {
                        this.isRefreshing = false;
                        this.handleAuthError(); // Only runs if refresh fails
                        return throwError(() => err);
                    }),
                    switchMap((res: any) => {
                        if (res && res.newAccessToken) {
                            this.isRefreshing = false;
                            this.authService.setToken(res.newAccessToken); // Save new tokens
                            this.refreshTokenSubject.next(res.newAccessToken);

                            // Retry the original request with new token
                            const retryReq = req.clone({
                                setHeaders: {
                                    Authorization: `Bearer ${res.newAccessToken}`
                                }
                            });

                            return next.handle(retryReq);
                        } else {
                            // Invalid response - maybe no access token
                            this.handleAuthError();
                            return throwError(() => new Error('Invalid token refresh response'));
                        }
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