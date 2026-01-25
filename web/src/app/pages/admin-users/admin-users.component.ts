import {Component} from '@angular/core';
import {ExtendedUser} from "../../api/types/extended-user";
import {UserRoles} from "../../api/types/user-roles";
import {AdminService} from "../../api/admin.service";

const pageSize: number = 100;

@Component({
    selector: 'app-admin-users',
    templateUrl: './admin-users.component.html'
})
export class AdminUsersComponent {
    users: ExtendedUser[] | undefined;
    nextPageIndex: number = pageSize + 1;
    total: number = 0;

    constructor(private adminService: AdminService) {
    }

    ngOnInit() {
        this.adminService.AdminGetUsers(pageSize).subscribe((data) => {
            this.users = data.items;
            this.total = data.listInfo.totalItems;
        });
    }

    getRole(role: number | undefined) {
        if (role == undefined) return "";
        return UserRoles.getRoleName(role);
    }

    loadNextPage(intersecting: boolean): void {
        if (!intersecting) return;

        if (this.nextPageIndex <= 0) return; // This is the server telling us there's no more data
        this.adminService.AdminGetUsers(pageSize, this.nextPageIndex).subscribe((data) => {
            this.users = this.users!.concat(data.items);
            this.nextPageIndex = data.listInfo.nextPageIndex;
            this.total = data.listInfo.totalItems;
        });
    }
}
