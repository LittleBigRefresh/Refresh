import {Component} from '@angular/core';
import {sha512Async} from 'src/app/hash';
import {PasswordVerificationService} from "../../services/password-verification.service";
import {faEnvelope, faKey, faSignIn, faUserPlus} from "@fortawesome/free-solid-svg-icons";
import {AuthService} from "../../api/auth.service";
import {FormHandler} from "../../helpers/FormHandler";

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent extends FormHandler {
    email: string = "";
    password: string = "";

    constructor(private authService: AuthService, private passwordVerifier: PasswordVerificationService) {
        super();
    }

    login() {
        const formInputs = this.cleanUpFormInputs(this.email, this.password);
        const [email, password] = formInputs;

        if (!this.passwordVerifier.verifyPassword(email, password)) {
            return;
        }

        sha512Async(password).then((hash) => {
            this.authService.LogIn(email, hash)
        });
    }

    protected readonly faEnvelope = faEnvelope;
    protected readonly faKey = faKey;
    protected readonly faSignIn = faSignIn;
    protected readonly faUserPlus = faUserPlus;
}
