import {Component, OnInit} from '@angular/core';
import {faCheckCircle, faKey, faMailReply} from "@fortawesome/free-solid-svg-icons";
import {ActivatedRoute, Params} from "@angular/router";
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-verify',
    templateUrl: './verify.component.html'
})
export class VerifyComponent implements OnInit {
    code: string = "";

    constructor(private authService: AuthService, private route: ActivatedRoute) {
    }

    public ngOnInit() {
        this.route.queryParams.subscribe((params: Params) => {
            const codeParam: string | undefined = params['code'];
            if (codeParam) this.code = codeParam;
        });
    }

    verify() {
        this.authService.VerifyEmail(this.code);
    }

    resend() {
        this.authService.ResendVerificationCode();
    }

    protected readonly faKey = faKey;
    protected readonly faCheckCircle = faCheckCircle;
    protected readonly faMailReply = faMailReply;
}
