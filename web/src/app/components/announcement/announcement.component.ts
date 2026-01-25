import {Component, EventEmitter, Input, Output} from '@angular/core';
import {faBullhorn, faTrash} from "@fortawesome/free-solid-svg-icons";

@Component({
    selector: 'announcement',
    templateUrl: './announcement.component.html'
})
export class AnnouncementComponent {
    _title: string = "Announcement";
    _body: string = "Loading...";
    _removable: boolean = false;

    @Output() removeEvent = new EventEmitter();

    public remove() {
        this.removeEvent.emit();
    }

    @Input()
    set title(str: string) {
        this._title = str;
    }

    @Input()
    set body(str: string) {
        this._body = str;
    }

    @Input()
    set removable(val: boolean) {
        this._removable = val;
    }

    protected readonly faBullhorn = faBullhorn;
    protected readonly faTrash = faTrash;
}
