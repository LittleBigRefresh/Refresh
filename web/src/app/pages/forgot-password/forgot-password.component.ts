import {Component} from '@angular/core';
import {faCancel, faEnvelope, faKey, faReply, faSkull} from "@fortawesome/free-solid-svg-icons";
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-forgot-password',
    templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
    email: string = "";

    constructor(private authService: AuthService) {
    }

    send(): void {
        this.authService.SendPasswordResetRequest(this.email);
    }


    protected readonly faKey = faKey;
    protected readonly faCancel = faCancel;
    protected readonly faEnvelope = faEnvelope;
    protected readonly faReply = faReply;
    protected readonly faSkull = faSkull;
}
