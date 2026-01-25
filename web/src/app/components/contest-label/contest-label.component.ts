import {Component, Input} from '@angular/core';

@Component({
    selector: 'app-contest-label',
    templateUrl: './contest-label.component.html',
})
export class ContestLabelComponent {
    @Input() title: string = "Title";
    @Input() text: string = "Text";
}
