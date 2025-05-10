import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../service/api.service';
import { AuthService } from '../service/authentication.service';
import { TokenRequest_DTO } from '../DTOs/token-request.dto';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {

    loginForm!: FormGroup;
    IsPaymentAllowed: boolean = false;
    token: TokenRequest_DTO = new TokenRequest_DTO();
    constructor(private fb: FormBuilder, private router: Router, private apiService: ApiService, public authService: AuthService) {
        this.loginForm = this.fb.group({
            UserName: ['', [Validators.required]],
            Password: ['', [Validators.required, Validators.minLength(6)]]
        });
    }

    ngOnInit(): void {
    }

    login() {
        if (this.loginForm.valid) {
            this.apiService.Login(this.loginForm.value).subscribe({
                next: (response: any) => {
                    if (response && response.accessToken) {
                        this.token = response;
                        this.authService.setToken(response.accessToken, response.refreshToken);
                        this.IsPaymentAllowed = true;
                    } else {
                        alert('Invalid login response');
                    }
                },
                error: (error) => {
                    alert('Login failed! Please check your credentials.');
                }
            });
        }
    }
}