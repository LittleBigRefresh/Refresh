import {Component, OnInit} from '@angular/core';
import {AdminQueuedRegistration} from "../../api/types/admin/admin-queued-registration";
import {ApiClient} from "../../api/api-client.service";
import {faTrash} from "@fortawesome/free-solid-svg-icons";
import {AdminService} from "../../api/admin.service";

@Component({
    selector: 'app-admin-registrations',
    templateUrl: './admin-registrations.component.html'
})
export class AdminRegistrationsComponent implements OnInit {
    registrations: AdminQueuedRegistration[] | undefined;

    constructor(private apiClient: ApiClient, private adminService: AdminService) {
    }

    ngOnInit() {
        this.adminService.AdminGetQueuedRegistrations().subscribe((data) => {
            this.registrations = data.items;
        });
    }

    remove(registration: AdminQueuedRegistration): void {
        this.adminService.AdminRemoveQueuedRegistration(registration);
        this.ngOnInit();
    }

    clearAll(): void {
        this.adminService.AdminRemoveAllQueuedRegistrations();
        this.ngOnInit();
    }

    protected readonly faTrash = faTrash;
}
