import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private hubConnection!: signalR.HubConnection;
    private apiUrl = environment.apiUrl;

    public startConnection(): void {
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.apiUrl}/ChatHub`) 
            .withAutomaticReconnect()
            .build();

        this.hubConnection
            .start()
            .then(() => console.log('SignalR connected'))
            .catch(err => console.error('SignalR connection error:', err));
    }

    public onPaymentStatus(callback: (message: string) => void): void {
        this.hubConnection.on('PaymentStatus', callback);
    }
}