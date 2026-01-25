import {Component, Input} from '@angular/core';
import {IconProp} from "@fortawesome/fontawesome-svg-core";
import {faPoo} from "@fortawesome/free-solid-svg-icons";

@Component({
    selector: 'menu-link',
    templateUrl: './menu-link.component.html'
})
export class MenuLinkComponent {
    @Input('link') link: string = "/";
    @Input('title') title: string = "Title";
    @Input('icon') icon: IconProp = faPoo;
    @Input('raw') raw: boolean = false;
}
