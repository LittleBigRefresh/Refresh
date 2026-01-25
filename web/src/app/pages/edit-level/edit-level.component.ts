import {Component, OnInit} from '@angular/core';
import {Level} from "../../api/types/level";
import {ExtendedUser} from "../../api/types/extended-user";
import {ActivatedRoute, ParamMap, Router} from "@angular/router";
import {catchError, of} from "rxjs";
import {HttpErrorResponse} from "@angular/common/http";
import {AuthService} from "../../api/auth.service";
import {ApiClient, GetAssetImageLink} from "../../api/api-client.service";
import {
    faCancel,
    faCertificate, faClone,
    faFloppyDisk,
    faPencil,
    faTrash,
    faUser
} from "@fortawesome/free-solid-svg-icons";
import {LevelEditRequest} from "../../api/types/level-edit-request";
import {UserRoles} from "../../api/types/user-roles";
import {DropdownOption} from "../../components/form-dropdown/form-dropdown.component";
import {sha1Async} from "../../hash";
import {AdminService} from "../../api/admin.service";

@Component({
    selector: 'edit-level',
    templateUrl: './edit-level.component.html'
})
export class EditLevelComponent implements OnInit {
    level: Level | undefined;
    ownUser: ExtendedUser | undefined;

    iconHash: string = "0";
    title: string = "";
    description: string = "";
    gameVersion: string = "0";
    isReUpload: boolean = false;
    teamPicked: boolean = false;
    originalPublisher: string | undefined = undefined;

    gameVersions: DropdownOption[] = [
        {
            Name: "LittleBigPlanet 1",
            Value: "0",
        },
        {
            Name: "LittleBigPlanet 2",
            Value: "1",
        },
        {
            Name: "LittleBigPlanet 3",
            Value: "2",
        },
        {
            Name: "LittleBigPlanet Vita",
            Value: "3",
        },
        {
            Name: "LittleBigPlanet PSP",
            Value: "4",
        },
        {
            Name: "Beta Build",
            Value: "6",
        }
    ];

    constructor(private authService: AuthService, private apiClient: ApiClient, private adminService: AdminService,
                private router: Router, private route: ActivatedRoute) {
    }

    ngOnInit(): void {
        this.route.paramMap.subscribe((params: ParamMap) => {
            const id = params.get('id') as number | null;
            if (id == null) return;
            this.apiClient.GetLevelById(id)
                .pipe(catchError((error: HttpErrorResponse, caught) => {
                    console.warn(error);
                    if (error.status === 404) {
                        this.router.navigate(["/404"]);
                        return of(undefined);
                    }

                    return caught;
                }))
                .subscribe(data => {
                    this.level = data;
                    if (data === undefined) return;

                    this.title = data.title;
                    this.description = data.description;
                    this.gameVersion = data.gameVersion.toString();
                    this.iconHash = data.iconHash;
                    this.isReUpload = data.isReUpload;
                    this.originalPublisher = data.originalPublisher;
                    this.teamPicked = data.teamPicked;
                });
        });

        this.ownUser = this.authService.user;
        this.authService.userWatcher.subscribe((data) => {
            this.ownUser = data;
        });
    }

    update() {
        if (this.level == undefined) return;

        const isAdmin: boolean = (this.ownUser?.role ?? 0) >= UserRoles.Curator;
        const payload: LevelEditRequest = {
            title: this.title,
            description: this.description,
            iconHash: undefined,
            gameVersion: this.gameVersion,
            originalPublisher: this.originalPublisher,
            isReUpload: this.isReUpload,
        }

        this.apiClient.EditLevel(payload, this.level.levelId, isAdmin);

        if (isAdmin && this.teamPicked != this.level.teamPicked) {
            if (this.teamPicked) this.adminService.AdminAddTeamPick(this.level);
            else this.adminService.AdminRemoveTeamPick(this.level);
        }
    }

    cancel() {
        window.history.back();
    }

    delete() {
        if (this.level == undefined) return;

        if((this.ownUser?.role ?? UserRoles.User) >= UserRoles.Curator) {
            this.adminService.AdminDeleteLevel(this.level);
        }
        else {
            this.apiClient.DeleteLevel(this.level);
        }
    }

    async iconChanged($event: any) {
        if(!this.level) return;

        const file: File = $event.target.files[0]
        console.log(file);

        const data: ArrayBuffer = await file.arrayBuffer();
        const hash: string = await sha1Async(data);

        this.apiClient.UploadImageAsset(hash, data).subscribe(_ => {
            this.apiClient.UpdateLevelIcon(hash, this.level!.levelId, this.ownUser?.role == UserRoles.Admin);
            this.iconHash = hash;
        });
    }

    protected readonly faCertificate = faCertificate;
    protected readonly faPencil = faPencil;
    protected readonly faFloppyDisk = faFloppyDisk;
    protected readonly faTrash = faTrash;
    protected readonly faCancel = faCancel;
    protected readonly UserRoles = UserRoles;
    protected readonly GetAssetImageLink = GetAssetImageLink;
    protected readonly faUser = faUser;
    protected readonly faClone = faClone;
}
