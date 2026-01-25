import {Component} from '@angular/core';
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-delete-account',
    templateUrl: './delete-account.component.html'
})
export class DeleteAccountComponent {
    constructor(private authService: AuthService) {
    }

    delete(): void {
        this.authService.DeleteAccount();
    }

    protected readonly window = window;
}
