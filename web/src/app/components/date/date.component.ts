import {Component, Input} from '@angular/core';
import * as moment from "dayjs";

@Component({
    selector: 'date',
    templateUrl: './date.component.html'
})
export class DateComponent {
    private _date: Date = new Date();

    @Input("date")
    set date(value: Date) {
        this._date = new Date(value);
    }

    getMoment(): string {
        return moment(this._date).fromNow();
    }

    getFormattedDate(): string {
        return `${this._date.toLocaleDateString()} @ ${this._date.toLocaleTimeString()}`;
    }
}
