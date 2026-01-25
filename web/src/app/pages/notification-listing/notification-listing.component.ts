import {Component, OnInit} from '@angular/core';
import {ApiClient} from "../../api/api-client.service";
import {catchError, of} from "rxjs";
import {RefreshNotification} from "../../api/types/refresh-notification";
import {Router} from "@angular/router";
import {faTrash} from "@fortawesome/free-solid-svg-icons";
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-notification-listing',
    templateUrl: './notification-listing.component.html'
})
export class NotificationListingComponent implements OnInit {
    notifications: RefreshNotification[] | undefined | null = null;

    constructor(private authService: AuthService, private apiClient: ApiClient, private router: Router) {
    }

    ngOnInit() {
        this.apiClient.GetNotifications()
            .pipe(catchError(() => {
                if (!this.authService.user)
                    this.router.navigate(['/login']);

                return of(undefined);
            }))
            .subscribe((data) => {
                this.notifications = data?.items;
            });
    }

    clearNotification(notificationId: string) {
        if (!this.notifications) return;

        const notificationIndex = this.notifications
            .findIndex(item => item.notificationId == notificationId);

        if (notificationIndex === -1 || notificationIndex == undefined)
            throw "Notification was somehow not found when trying to remove from list. This should never occur.";

        // Remove from array on client
        this.notifications.splice(notificationIndex, 1);

        // Tell server we cleared the notification
        this.apiClient.ClearNotification(notificationId)
            .subscribe();
    }

    clearAllNotifications() {
        if (this.notifications?.length === 0) return;
        this.notifications = [];

        this.apiClient.ClearAllNotifications()
            .subscribe();
    }

    protected readonly faTrash = faTrash;
}
