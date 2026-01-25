import {Component} from '@angular/core';
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-logout',
    templateUrl: './logout.component.html'
})
export class LogoutComponent {
    constructor(public authService: AuthService) {
    }
}
