import {Component, Input} from '@angular/core';
import {IconProp} from "@fortawesome/fontawesome-svg-core";

@Component({
    selector: 'primary-button',
    templateUrl: './primary-button.component.html'
})
export class PrimaryButtonComponent {
    _text: string = "NOT SET, FIX ME"
    _icon: IconProp | undefined;

    @Input()
    set icon(param: IconProp) {
        this._icon = param;
    }

    @Input()
    set text(param: string) {
        this._text = param;
    }
}
