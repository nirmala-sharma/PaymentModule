import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '../service/api.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { SignalRService } from '../service/signalr.service';

@Component({
    selector: 'app-payment-process',
    templateUrl: './payment-process.component.html'
})
export class PaymentProcessComponent implements OnInit {
    checkoutForm!: FormGroup;
    PaymentModes = ['Card', 'Bank'];
    SelectedPaymentMode: string = '';
    @Output('payment-response-callback') PaymentResponseCallBack = new EventEmitter<object>();
    statusMessage: string = '';

    constructor(private fb: FormBuilder, private apiService: ApiService, private sanitizer: DomSanitizer, private signalRService: SignalRService) {
        this.checkoutForm = this.fb.group({
            FullName: ['', Validators.required],
            Email: ['', [Validators.required, Validators.email]],
            Currency: ['', Validators.required],
            Amount: [0, Validators.required],
            PaymentMode: ['', Validators.required],
            CardNumber: [''],
            ExpiryDate: [''],
            CVV: [''],
            AccountNumber: [''],
            BankName: ['']
        });

        this.checkoutForm.get('PaymentMode')?.valueChanges.subscribe((value) => {
            this.SelectedPaymentMode = value;
            this.UpdateFormValidators();
        });
    }
    ngOnInit(): void {
    }
    // getSanitizedHtml(): SafeHtml {
    //     return this.sanitizer.bypassSecurityTrustHtml(this.checkoutForm.get('FullName')?.value || '');
    // }
    UpdateFormValidators(): void {
        const cardNumberControl = this.checkoutForm.get('CardNumber');
        const expiryDateControl = this.checkoutForm.get('ExpiryDate');
        const cvvControl = this.checkoutForm.get('CVV');
        const accountNumberControl = this.checkoutForm.get('AccountNumber');
        const bankNameControl = this.checkoutForm.get('BankName');

        if (this.SelectedPaymentMode === 'Card') {
            cardNumberControl?.setValidators([Validators.required, Validators.pattern(/^\d{16}$/)]);
            expiryDateControl?.setValidators([Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]);
            cvvControl?.setValidators([Validators.required, Validators.pattern(/^\d{3}$/)]);
            accountNumberControl?.clearValidators();
            bankNameControl?.clearValidators();
        }
        else if (this.SelectedPaymentMode === 'Bank') {
            accountNumberControl?.setValidators([Validators.required, Validators.pattern(/^\d{9,18}$/)]);
            bankNameControl?.setValidators([Validators.required]);
            cardNumberControl?.clearValidators();
            expiryDateControl?.clearValidators();
            cvvControl?.clearValidators();
        }

        cardNumberControl?.updateValueAndValidity();
        expiryDateControl?.updateValueAndValidity();
        cvvControl?.updateValueAndValidity();
        accountNumberControl?.updateValueAndValidity();
        bankNameControl?.updateValueAndValidity();
    }

    submitPayment() {
        if (this.checkoutForm.valid) {
            let idempotencyKey = sessionStorage.getItem('idempotencyKey');

            // Generate key only if not already present
            if (!idempotencyKey) {
                this.generateIdempotentKey();
            }

            // Establish a real-time SignalR connection with the server.
            // Listen for live payment status updates pushed by the server
            // and update the statusMessage variable accordingly (e.g., "Processing", "Success", "Failed").
            this.signalRService.startConnection();
            this.signalRService.onPaymentStatus((msg: string) => {
                this.statusMessage = msg;
            });

            this.apiService.processPayment(this.checkoutForm.value).subscribe({
                next: (response: any) => {
                    if (response) {
                        alert('Payment Successful');
                        //this.checkoutForm.reset();
                        this.PaymentResponseCallBack.emit();
                        this.removeIdempotentKey();
                    } else {
                        alert('Invalid login response');
                    }
                },
                error: (error) => {
                    const errors = error?.error?.errors;
                    if (errors && Array.isArray(errors)) {
                        alert('Validation Errors: ' + errors);
                    } else {
                        alert('Payment process failed.');
                    }
                }
            });
        }
    }
    ngOnDestroy(): void {
        this.removeIdempotentKey();
    }
    generateIdempotentKey() {
        const idempotencyKey = crypto.randomUUID();
        sessionStorage.setItem('idempotencyKey', idempotencyKey);
    }
    removeIdempotentKey() {
        sessionStorage.removeItem('idempotencyKey');
    }
}

