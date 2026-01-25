import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {sha512Async} from 'src/app/hash';
import {PasswordVerificationService} from "../../services/password-verification.service";
import {faCancel, faKey} from "@fortawesome/free-solid-svg-icons";
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-reset-password',
    templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent implements OnInit {
    password: string = ""
    confirmPassword: string = ""

    constructor(private authService: AuthService, private route: ActivatedRoute, private passwordVerifier: PasswordVerificationService) {
    }

    ngOnInit() {
        this.route.queryParams.subscribe((params) => {
            let tokenParam: string | undefined = params['token'];
            if (tokenParam) {
                tokenParam = tokenParam.replaceAll(' ', '+');
                this.authService.resetToken = tokenParam;
                console.log(tokenParam);
            }
        })
    }

    reset(): void {
        if (!this.passwordVerifier.verifyPassword(undefined, this.password, undefined, this.confirmPassword)) {
            return;
        }

        sha512Async(this.password).then((hash) => {
            this.authService.ResetPassword(hash)
        })
    }

    protected readonly faKey = faKey;
    protected readonly faCancel = faCancel;
}
