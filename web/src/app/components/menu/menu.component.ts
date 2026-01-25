import {Component, Input} from '@angular/core';
import {HeaderLink} from "../../header-link";
import {faSignOut, faUser} from "@fortawesome/free-solid-svg-icons";

@Component({
    selector: 'app-menu',
    templateUrl: './menu.component.html'
})
export class MenuComponent {
    @Input('links') links: HeaderLink[] = [];

    protected readonly faSignOut = faSignOut;
    protected readonly faUser = faUser;
}
