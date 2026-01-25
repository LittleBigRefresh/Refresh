import {ChangeDetectorRef, Component, inject, Inject, NgZone, OnInit, PLATFORM_ID} from '@angular/core';
import {Contest} from "../../api/types/contests/contest";
import {ApiClient} from "../../api/api-client.service";
import {ActivatedRoute, ParamMap} from "@angular/router";
import {EmbedService} from "../../services/embed.service";
import {TitleService} from "../../services/title.service";
import {UserRoles} from "../../api/types/user-roles";
import {ExtendedUser} from "../../api/types/extended-user";
import {AuthService} from "../../api/auth.service";
import {faCertificate, faPen} from "@fortawesome/free-solid-svg-icons";
import {ApiListResponse} from "../../api/types/response/api-list-response";
import {Level} from "../../api/types/level";
import {GameVersion} from "../../api/types/game-version";
import {CategoryPreviewType} from "../../components/category-preview/category-preview.component";
import {isPlatformBrowser} from "@angular/common";

@Component({
    selector: 'app-contest',
    templateUrl: './contest.component.html'
})
export class ContestComponent implements OnInit {
    contest: Contest | undefined = undefined;
    ownUser: ExtendedUser | undefined;

    levelEntries: ApiListResponse<Level> | undefined;
    endsIn: string = "";
    startsIn: string = "";
    protected readonly UserRoles = UserRoles;
    protected readonly faPen = faPen;
    protected readonly faCertificate = faCertificate;
    protected readonly CategoryPreviewType = CategoryPreviewType;

    constructor(private route: ActivatedRoute, private authService: AuthService, private api: ApiClient, private embed: EmbedService, private title: TitleService,
                @Inject(PLATFORM_ID) platformId: Object, changeDetector: ChangeDetectorRef) {
        if(isPlatformBrowser(platformId)) {
            inject(NgZone).runOutsideAngular(() => {
                setInterval(() => {
                    if (!this.contest) return;

                    this.setEndsIn();
                    this.setStartsIn();

                    changeDetector.detectChanges();
                }, 1000);
            })
        }
    }

    hasStarted(): boolean | undefined {
        if (!this.contest)
            return undefined;

        let now: Date = new Date();
        let start: Date = new Date(this.contest.startDate);

        return now > start;
    }

    hasEnded(): boolean | undefined {
        if (!this.contest)
            return undefined;

        let now: Date = new Date();
        let end: Date = new Date(this.contest.endDate);

        return now > end;
    }

    setStartsIn(): void {
        if (!this.contest) return;

        const startDate: Date = new Date(this.contest.startDate);

        if (this.hasStarted()) {
            this.startsIn = `Started ${startDate.toLocaleDateString()} @ ${startDate.toLocaleTimeString()}`;
            return;
        }

        const difference = this.dateRemaining(new Date(), startDate);

        this.startsIn = "Starts in " + difference;
    }

    setEndsIn(): void {
        if (!this.contest) return;

        const endDate: Date = new Date(this.contest.endDate);

        if (!this.hasStarted()) {
            this.endsIn = this.dateDuration(new Date(this.contest.startDate), endDate) + " long";
            return;
        }

        if (this.hasEnded()) {
            this.endsIn = `Ended ${endDate.toLocaleDateString()} @ ${endDate.toLocaleTimeString()}`;
            return;
        }

        const difference = this.dateRemaining(new Date(), endDate);

        this.endsIn = "Ends in " + difference;
    }

    dateDuration(from: Date, to: Date) {
        const milliseconds: number = to.getTime() - from.getTime();

        const seconds = Math.floor((milliseconds / 1000) % 60);
        const minutes = Math.floor((milliseconds / (1000 * 60)) % 60);
        const hours = Math.floor((milliseconds / (1000 * 60 * 60)) % 24);
        const days = Math.floor(milliseconds / (1000 * 60 * 60 * 24));

        if (days > 0) return `${days} days`;
        if (hours > 0) return `${hours} hours`;
        if (minutes > 0) return `${minutes} minutes`;

        return `${seconds} seconds`;
    }

    dateRemaining(from: Date, to: Date): string {
        const milliseconds: number = to.getTime() - from.getTime();

        const seconds = Math.floor((milliseconds / 1000) % 60);
        const minutes = Math.floor((milliseconds / (1000 * 60)) % 60);
        const hours = Math.floor((milliseconds / (1000 * 60 * 60)) % 24);
        const days = Math.floor(milliseconds / (1000 * 60 * 60 * 24));

        const formattedDays = days < 10 ? `0${days}` : `${days}`;
        const formattedHours = hours < 10 ? `0${hours}` : `${hours}`;
        const formattedMinutes = minutes < 10 ? `0${minutes}` : `${minutes}`;
        const formattedSeconds = seconds < 10 ? `0${seconds}` : `${seconds}`

        if (days > 0)
            return `${formattedDays}:${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
        if (hours > 0)
            return `${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
        if (minutes > 0)
            return `${formattedMinutes}:${formattedSeconds}`;

        return `${formattedSeconds}`;

    }

    formatAllowedGames(): string {
        let response = "";
        this.contest?.allowedGames.forEach(game => {
            let name = GameVersion[game];
            if (game === GameVersion.BetaBuild)
                name = "Beta Builds";

            response += name.replace("LittleBigPlanet", "LBP") + ", ";
        });

        return response.substring(0, response.length - 2); // remove last comma & space
    }

    ngOnInit(): void {
        this.route.paramMap.subscribe((params: ParamMap) => {
            const id: string = params.get("id")!;
            this.api.GetContestById(id).subscribe(contest => {
                this.contest = contest;
                this.title.setTitle(contest.contestTitle);
                this.embed.embedContest(contest);
                this.setStartsIn();
                this.setEndsIn();

                this.api.GetLevelListing("contest", 10, 0, {"contest": this.contest?.contestId})
                    .subscribe(data => {
                        this.levelEntries = data;
                    });
            })
        });

        this.ownUser = this.authService.user;
        this.authService.userWatcher.subscribe((data) => {
            this.ownUser = data;
        });
    }
}
