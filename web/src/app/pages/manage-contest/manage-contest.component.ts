import {Component, OnInit} from '@angular/core';
import {Contest} from "../../api/types/contests/contest";
import {ApiClient} from "../../api/api-client.service";
import {ActivatedRoute, ParamMap, Router} from "@angular/router";
import {
    faBook,
    faBrush,
    faCalendar,
    faCamera,
    faCancel,
    faCertificate,
    faFloppyDisk,
    faGamepad,
    faHashtag,
    faHourglassEnd,
    faHourglassStart,
    faMedal,
    faPencil,
    faStar,
    faTrash,
    faUser
} from "@fortawesome/free-solid-svg-icons";
import {ContestEditRequest} from "../../api/types/contests/contest-edit-request";
import {ExtendedUser} from "../../api/types/extended-user";
import {AuthService} from "../../api/auth.service";
import {Location} from "@angular/common";
import {GameVersion} from "../../api/types/game-version";

@Component({
    selector: 'app-manage-contest',
    templateUrl: './manage-contest.component.html'
})
export class ManageContestComponent implements OnInit {
    ownUser: ExtendedUser | undefined;
    protected create: boolean = false;
    protected existingContest: Contest | undefined = undefined;
    protected newContest: ContestEditRequest | undefined = undefined;
    protected readonly faFloppyDisk = faFloppyDisk;
    protected readonly faCancel = faCancel;
    protected readonly faMedal = faMedal;
    protected readonly faHashtag = faHashtag;
    protected readonly faCamera = faCamera;
    protected readonly faPencil = faPencil;
    protected readonly faBook = faBook;
    protected readonly faHourglassStart = faHourglassStart;
    protected readonly faHourglassEnd = faHourglassEnd;
    protected readonly faUser = faUser;
    protected readonly faCalendar = faCalendar;
    protected readonly faTrash = faTrash;
    protected readonly faBrush = faBrush;
    protected readonly GameVersion = GameVersion;
    protected readonly faGamepad = faGamepad;
    protected readonly Object = Object;
    protected readonly faStar = faStar;
    protected readonly faCertificate = faCertificate;

    constructor(private api: ApiClient, private route: ActivatedRoute, private router: Router, private authService: AuthService, private location: Location) {
        this.ownUser = this.authService.user;
        this.authService.userWatcher.subscribe((data) => {
            this.ownUser = data;
        });
    }

    ngOnInit(): void {
        this.route.paramMap.subscribe((params: ParamMap) => {
            if (!params.get('id')) {
                this.create = true;
                this.newContest = {
                    bannerUrl: "https://i.imgur.com/XhmwFvC.png",
                    contestDetails: "",
                    contestId: "",
                    contestSummary: "",
                    contestTag: "",
                    contestTitle: "",
                    endDate: undefined,
                    organizerId: this.ownUser?.userId,
                    startDate: undefined,
                    contestTheme: "",
                    allowedGames: [],
                    templateLevelId: undefined,
                };
                return;
            }

            const id: string = params.get('id')!;
            this.api.GetContestById(id).subscribe((contest: Contest) => {
                this.existingContest = contest;
                this.newContest = {
                    bannerUrl: contest.bannerUrl,
                    contestDetails: contest.contestDetails,
                    contestId: contest.contestId,
                    contestSummary: contest.contestSummary,
                    contestTag: contest.contestTag,
                    contestTitle: contest.contestTitle,
                    endDate: contest.endDate,
                    organizerId: contest.organizer.userId,
                    startDate: contest.startDate,
                    contestTheme: contest.contestTheme,
                    allowedGames: contest.allowedGames,
                    templateLevelId: contest.templateLevel?.levelId,
                };
            })
        });
    }

    submit(): void {
        if (!this.newContest) return;

        if (this.create) {
            this.api.CreateContest(this.newContest).subscribe()
        } else {
            this.api.UpdateContest(this.newContest).subscribe()
        }

        this.router.navigateByUrl("contests/" + this.newContest.contestId);
    }

    cancel() {
        this.location.back()
    }

    delete() {
        if (!this.existingContest)
            return;

        this.api.DeleteContest(this.existingContest);
    }

    setAllowedGame(gameVersion: GameVersion, value: boolean) {
        if (this.newContest == null)
            return;

        if (value) {
            this.newContest.allowedGames?.push(gameVersion);
        } else {
            this.newContest.allowedGames = this.newContest.allowedGames?.filter((game) => game != gameVersion);
        }

        console.log(this.newContest?.allowedGames);
    }

    setTemplateLevelId(id: string) {
        if (!this.newContest) return;

        this.newContest.templateLevelId = parseInt(id);
    }
}
