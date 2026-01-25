import {Injectable} from "@angular/core";
import {Announcement} from "./types/announcement";
import {User} from "./types/user";
import {AdminPunishUserRequest} from "./types/admin/admin-punish-user-request";
import {Level} from "./types/level";
import {BannerService} from "../banners/banner.service";
import {ApiRequestCreator} from "./api-request.creator";
import {AdminQueuedRegistration} from "./types/admin/admin-queued-registration";
import {ExtendedUser} from "./types/extended-user";
import {Instance} from "./types/instance";
import {ApiClient} from "./api-client.service";
import {Observable} from "rxjs";

@Injectable({providedIn: 'root'})
export class AdminService {
    private instance: Instance = undefined!;

    constructor(private bannerService: BannerService, private apiRequestCreator: ApiRequestCreator, apiClient: ApiClient) {
        apiClient.GetInstanceInformation().subscribe(data => {
            this.instance = data;
        })
    }

    public AdminAddAnnouncement(title: string, body: string) {
        this.apiRequestCreator.makeRequest<Announcement>("POST", "admin/announcements", {title, text: body})
            .subscribe(data => {
                this.instance?.announcements.push({title, text: body, announcementId: data.announcementId})
                this.bannerService.pushSuccess("Posted announcement", "The announcement was successfully posted.");
            });
    }

    public AdminRemoveAnnouncement(id: string) {
        this.apiRequestCreator.makeRequest("DELETE", "admin/announcements/" + id).subscribe(() => {
            this.bannerService.pushWarning("Removed announcement", "The announcement was successfully removed.");
        });

        if (!this.instance) return;
        let index = this.instance.announcements.findIndex(announcement => announcement.announcementId === id);

        if (index >= 0) {
            this.instance.announcements.splice(index, 1);
        }
    }

    public AdminPunishUser(user: User, punishmentType: 'restrict' | 'ban', expiryDate: Date, reason: string) {
        const body: AdminPunishUserRequest = {
            expiryDate,
            reason
        };

        this.apiRequestCreator.makeRequest("POST", `admin/users/uuid/${user.userId}/${punishmentType}`, body).subscribe(() => {
            this.bannerService.pushSuccess(user.username + " is punished", "The punishment was successfully applied.");
        });
    }

    public AdminPardonUser(user: User) {
        this.apiRequestCreator.makeRequest("POST", `admin/users/uuid/${user.userId}/pardon`).subscribe(() => {
            this.bannerService.pushSuccess(user.username + " is forgiven", "The punishment was successfully removed.");
        });
    }

    public AdminAddTeamPick(level: Level) {
        this.apiRequestCreator.makeRequest("POST", `admin/levels/id/${level.levelId}/teamPick`).subscribe(() => {
            this.bannerService.pushSuccess("Team Picked", "The level was successfully team picked.");
        });

        level.teamPicked = true;
    }

    public AdminRemoveTeamPick(level: Level) {
        this.apiRequestCreator.makeRequest("POST", `admin/levels/id/${level.levelId}/removeTeamPick`).subscribe(() => {
            this.bannerService.pushWarning("Team Pick Removed", "The team pick was successfully removed.");
        });

        level.teamPicked = false;
    }

    public AdminDeleteLevel(level: Level) {
        this.apiRequestCreator.makeRequest("DELETE", `admin/levels/id/${level.levelId}`).subscribe(() => {
            this.bannerService.pushSuccess("Level Removed", "The level was successfully deleted.");
        });
    }

    public AdminGetQueuedRegistrations() {
        return this.apiRequestCreator.makeListRequest<AdminQueuedRegistration>("GET", "admin/registrations");
    }

    public AdminRemoveQueuedRegistration(registration: AdminQueuedRegistration) {
        this.apiRequestCreator.makeRequest("DELETE", `admin/registrations/${registration.registrationId}`).subscribe(() => {
            this.bannerService.pushSuccess(`Registration Removed`, `The queued registration for ${registration.username}/${registration.emailAddress} has been removed.`);
        });
    }

    public AdminRemoveAllQueuedRegistrations() {
        this.apiRequestCreator.makeRequest("DELETE", "admin/registrations").subscribe(() => {
            this.bannerService.pushSuccess(`All Registrations Removed`, `All queued registrations have been removed.`);
        });
    }

    public AdminDeleteUserPlanets(user: User) {
        this.apiRequestCreator.makeRequest("DELETE", `admin/users/uuid/${user.userId}/planets`).subscribe(() => {
            this.bannerService.pushSuccess(`Planets Removed`, `${user.username}'s planets have been removed.`);
        });
    }

    public AdminDeleteUser(user: User) {
        this.apiRequestCreator.makeRequest("DELETE", `admin/users/uuid/${user.userId}`).subscribe(() => {
            this.bannerService.pushSuccess(`User Deleted`, `${user.username}'s account has been deleted.`);
        });
    }

    public AdminGetUsers(count: number, skip: number = 0) {
        return this.apiRequestCreator.makeListRequest<ExtendedUser>("GET", `admin/users?count=${count}&skip=${skip}`);
    }

    public GetExtendedUserByUuid(uuid: string): Observable<ExtendedUser> {
        return this.apiRequestCreator.makeRequest<ExtendedUser>("GET", "admin/users/uuid/" + uuid)
    }
}
