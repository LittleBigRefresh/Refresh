import {Component, Input} from '@angular/core';

@Component({
    selector: 'tooltip',
    templateUrl: './tooltip.component.html',
})
export class TooltipComponent {
    _text: string = "";
    _active: boolean = true;

    @Input()
    set text(text: string) {
        this._text = text;
    }

    @Input()
    set active(active: boolean) {
        this._active = active;
    }
}
