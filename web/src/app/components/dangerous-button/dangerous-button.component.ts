import {Component, Input} from '@angular/core';
import {IconProp} from "@fortawesome/fontawesome-svg-core";

@Component({
    selector: 'dangerous-button',
    templateUrl: './dangerous-button.component.html',
})
export class DangerousButtonComponent {
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
