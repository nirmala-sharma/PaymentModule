import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../service/api.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {

    loginForm!: FormGroup;
    constructor(private fb: FormBuilder, private router: Router, private apiService: ApiService) {
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
                        console.log(response);
                        // this.authService.login(response.accessToken);
                        // this.router.navigate(['/dashboard']);
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