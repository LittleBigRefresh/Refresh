import {Component, OnInit} from '@angular/core';
import {UserUpdateRequest} from "../../api/types/user-update-request";
import {
    faCamera,
    faCancel,
    faDesktop,
    faEnvelope,
    faFloppyDisk,
    faGamepad,
    faKey, faLeftRight,
    faPencil,
    faTrash
} from "@fortawesome/free-solid-svg-icons";
import {ExtendedUser} from "../../api/types/extended-user";
import {startWith} from "rxjs";
import {AuthService} from "../../api/auth.service";
import {DropdownOption} from "../../components/form-dropdown/form-dropdown.component";
import {ThemeService} from "../../theme.service";
import {ApiClient, GetAssetImageLink} from "../../api/api-client.service";
import {sha1Async} from "../../hash";
import {BannerService} from "../../banners/banner.service";

@Component({
    selector: 'app-settings',
    templateUrl: './settings.component.html',
})
export class SettingsComponent implements OnInit {
    iconHash: string = "0";
    description: string = "";
    email: string = "";
    emailVerified: boolean = false;

    allowIpAuth: boolean = false;
    allowPsnAuth: boolean = false;
    allowRpcnAuth: boolean = false;

    themingSupported: boolean;

    themes: DropdownOption[] = [
        {
            Name: "Default",
            Value: "default",
        },
        {
            Name: "Hack",
            Value: "hack",
        },
        {
            Name: "Ultra-Dark",
            Value: "ultraDark",
        },
        {
            Name: "Sound Shapes",
            Value: "soundShapes",
        },
        {
            Name: "Lighthouse",
            Value: "lighthouse",
        },
        {
            Name: "Hotdog Stand",
            Value: "hotdogStand",
        },
        {
            Name: "Steam Green",
            Value: "vgui",
        },
        {
            Name: "Chicago",
            Value: "chicago"
        }
    ]

    theme: string;
    useFullPageWidth: boolean;

    constructor(private authService: AuthService, private themeService: ThemeService, private apiClient: ApiClient, private bannerService: BannerService) {
        this.themingSupported = themeService.IsThemingSupported();
        this.theme = themeService.GetTheme();
        this.useFullPageWidth = themeService.GetUseFullPageWidth();
    }

    ngOnInit(): void {
        this.authService.userWatcher
            .pipe(startWith(this.authService.user))
            .subscribe((data) => this.updateInputs(data));
    }

    updateInputs(data: ExtendedUser | undefined) {
        this.iconHash = data?.iconHash ?? "0";
        this.description = data?.description ?? "";
        this.email = data?.emailAddress ?? "";
        this.emailVerified = data?.emailAddressVerified ?? false;

        this.allowIpAuth = data?.allowIpAuthentication ?? false;
        this.allowPsnAuth = data?.psnAuthenticationAllowed ?? false;
        this.allowRpcnAuth = data?.rpcnAuthenticationAllowed ?? false;
    }

    saveChanges() {
        let request: UserUpdateRequest = {
            description: this.description,
            emailAddress: this.email,

            allowIpAuthentication: this.allowIpAuth,
            psnAuthenticationAllowed: this.allowPsnAuth,
            rpcnAuthenticationAllowed: this.allowRpcnAuth,
        };

        this.authService.UpdateUser(request);
    }

    themeChanged() {
        this.themeService.SetTheme(this.theme);
        this.themeService.SetUseFullPageWidth(this.useFullPageWidth);
    }

    async avatarChanged($event: any) {
        const file: File = $event.target.files[0]
        console.log(file);

        const data: ArrayBuffer = await file.arrayBuffer();
        const hash: string = await sha1Async(data);

        this.apiClient.UploadImageAsset(hash, data).subscribe(_ => {
            this.authService.UpdateUserAvatar(hash);
        });
    }

    protected readonly faPencil = faPencil;
    protected readonly faKey = faKey;
    protected readonly faDesktop = faDesktop;
    protected readonly faGamepad = faGamepad;
    protected readonly faEnvelope = faEnvelope;
    protected readonly faTrash = faTrash;
    protected readonly faFloppyDisk = faFloppyDisk;
    protected readonly faCancel = faCancel;
    protected readonly faLeftRight = faLeftRight;
    protected readonly GetAssetImageLink = GetAssetImageLink;
}
