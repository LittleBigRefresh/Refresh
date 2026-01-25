import {Component} from '@angular/core';
import {sha512Async} from "../../hash";
import {PasswordVerificationService} from "../../services/password-verification.service";
import {
    faEnvelope,
    faKey,
    faSignIn,
    faTriangleExclamation,
    faUser,
    faUserPlus
} from "@fortawesome/free-solid-svg-icons";
import {AuthService} from "../../api/auth.service";
import {FormHandler} from "../../helpers/FormHandler";

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html'
})
export class RegisterComponent extends FormHandler {
    username: string = "";
    email: string = "";
    password: string = "";
    confirmPassword: string = "";

    constructor(private authService: AuthService, private passwordVerifier: PasswordVerificationService) {
        super();
    }

    register() {
        const formInputs = this.cleanUpFormInputs(this.username, this.email, this.password, this.confirmPassword);
        const [username, email, password, confirmPassword] = formInputs;

        if (!this.passwordVerifier.verifyPassword(username, password, username, confirmPassword)) {
            return;
        }

        sha512Async(password).then((hash) => {
            this.authService.Register(username, email, hash)
        });
    }
    protected readonly faEnvelope = faEnvelope;
    protected readonly faTriangleExclamation = faTriangleExclamation;
    protected readonly faSignIn = faSignIn;
    protected readonly faUser = faUser;
    protected readonly faKey = faKey;
    protected readonly faUserPlus = faUserPlus;
}
