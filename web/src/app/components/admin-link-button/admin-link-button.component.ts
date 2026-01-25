import {Component} from '@angular/core';
import {faWrench} from "@fortawesome/free-solid-svg-icons";

@Component({
    selector: 'admin-link-button',
    templateUrl: './admin-link-button.component.html'
})
export class AdminLinkButtonComponent {

    protected readonly faWrench = faWrench;
}
