import {Component, EventEmitter, Input, Output} from '@angular/core';
import {RefreshNotification} from "../../api/types/refresh-notification";
import {IconName, IconProp} from "@fortawesome/fontawesome-svg-core";

@Component({
    selector: 'refresh-notification',
    templateUrl: './refresh-notification.component.html'
})
export class RefreshNotificationComponent {
    _notification: RefreshNotification = null!;

    @Output() clearEvent = new EventEmitter<string>(); // string: NotificationId

    @Input()
    set notification(param: RefreshNotification) {
        this._notification = param;
    }

    getIcon(name: string): IconProp {
        return name as IconName;
    }

    raiseClearEvent(): void {
        this.clearEvent.emit(this._notification.notificationId)
    }
}
