import {animate, group, query, style, transition, trigger} from '@angular/animations';
import {AfterViewInit, Component, ElementRef, ViewChild} from '@angular/core';
import {
    faBell,
    faBookBookmark,
    faCameraAlt,
    faCertificate,
    faEnvelope,
    faExclamationTriangle,
    faFireAlt,
    faGear,
    faMedal,
    faSignIn,
    faSquarePollVertical,
    faWrench
} from '@fortawesome/free-solid-svg-icons';
import {ApiClient, GetAssetImageLink} from './api/api-client.service';
import {HeaderLink} from './header-link';
import {BannerService} from './banners/banner.service';
import {NgxMasonryOptions} from "ngx-masonry";
import {ExtendedUser} from "./api/types/extended-user";
import {UserRoles} from "./api/types/user-roles";
import {Instance} from "./api/types/instance";
import {EmbedService} from "./services/embed.service";
import {TitleService} from "./services/title.service";
import {AuthService} from "./api/auth.service";
import {ThemeService} from "./theme.service";

const fadeLength: string = "50ms";

export const masonryOptions: NgxMasonryOptions = {
    resize: true,
    animations: {
        show: [
            style({opacity: 0}),
            animate(fadeLength, style({opacity: 1}))
        ]
    },
    horizontalOrder: true,
};

export function GenerateEmptyList(i: number): any[] {
    return new Array(i);
}

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    animations: [
        trigger('routeAnimations', [
            transition('* => *', [
                group([
                    query(':leave', [
                        style({opacity: 1, 'grid-column': 1, 'grid-row': 1}),
                        animate(fadeLength, style({opacity: 0}))
                    ], {optional: true}),
                    query(':enter', [
                        style({opacity: 0, 'grid-column': 1, 'grid-row': 1}),
                        animate(fadeLength, style({opacity: 1}))
                    ], {optional: true}),
                ])
            ])
        ])
    ],
})
export class AppComponent implements AfterViewInit {
    title: string = 'Refresh Website';
    user: ExtendedUser | undefined = undefined;

    instance: Instance | undefined = undefined;

    logoUrl: string = "https://raw.githubusercontent.com/LittleBigRefresh/Branding/refs/heads/main/icons/refresh_transparent.svg";

    @ViewChild("login") login!: ElementRef;
    @ViewChild("menu") menu!: ElementRef;
    @ViewChild("notifications") notifications!: ElementRef;

    routerLinks: HeaderLink[] = [
        new HeaderLink("Levels", "/levels", faCertificate),
        new HeaderLink("Photos", "/photos", faCameraAlt),
        new HeaderLink("Activity", "/activity", faFireAlt),
        // new HeaderLink("Ranking", "/ranking", faListUl),
    ];

    rightSideRouterLinks: HeaderLink[] = []
    protected readonly GetAssetImageLink = GetAssetImageLink;
    protected readonly faSignIn = faSignIn;
    protected readonly faExclamationTriangle = faExclamationTriangle;
    protected readonly UserRoles = UserRoles;
    protected readonly faBell = faBell;

    constructor(authService: AuthService, private apiClient: ApiClient, public bannerService: BannerService, private embedService: EmbedService, private titleService: TitleService, public themeService: ThemeService) {
        this.apiClient.GetInstanceInformation().subscribe((data: Instance) => {
            this.instance = data;
            this.embedService.embedInstance(data);
            this.titleService.setTitle("");
            if (this.themeService.IsThemingSupported()) {
                const theme: string | null = localStorage.getItem("theme");
                if (theme) {
                    this.themeService.SetTheme(theme);
                } else {
                    this.themeService.SetTheme("default");
                }
            }

            if(data.websiteLogoUrl)
                this.logoUrl = data.websiteLogoUrl;
        });

        authService.userWatcher.subscribe((data) => this.handleUserUpdate(data));
    }

    ngAfterViewInit(): void {
        this.handleUserUpdate(this.user);
    }

    getTheme(): string {
        return this.themeService.GetTheme();
    }

    handleUserUpdate(user: ExtendedUser | undefined) {
        this.user = user;

        if (user && this.login?.nativeElement) {
            this.login.nativeElement.hidden = true;

            this.addRightSideRouterLinks(user);
        }

    }

    addRightSideRouterLinks(user: ExtendedUser) {
        if (user.role >= UserRoles.Admin) {
            this.rightSideRouterLinks.push(new HeaderLink("Admin Panel", "/admin", faWrench))
        }

        this.rightSideRouterLinks.push(new HeaderLink("API Documentation", "/documentation", faBookBookmark));
        this.rightSideRouterLinks.push(new HeaderLink("Server Statistics", "/statistics", faSquarePollVertical));
        this.rightSideRouterLinks.push(new HeaderLink("Contests", "/contests", faMedal));
        this.rightSideRouterLinks.push(new HeaderLink("Notifications", "/notifications", faBell));
        this.rightSideRouterLinks.push(new HeaderLink("Settings", "/settings", faGear));
        this.rightSideRouterLinks.push(new HeaderLink("Contact Us", `mailto:${this.instance?.contactInfo.emailAddress}`, faEnvelope, true));
    }


    toggleLogin(): void {
        this.login.nativeElement.hidden = !this.login.nativeElement.hidden;
    }

    toggleMenu(): void {
        this.menu.nativeElement.hidden = !this.menu.nativeElement.hidden;
        this.notifications.nativeElement.hidden = true;
    }

    toggleNotifications(): void {
        this.menu.nativeElement.hidden = true;
        this.notifications.nativeElement.hidden = !this.notifications.nativeElement.hidden;
    }
}
