import {HttpErrorResponse} from '@angular/common/http';
import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, ParamMap, Router} from '@angular/router';
import {catchError, of, tap} from 'rxjs';
import {ApiClient, GetAssetImageLink} from 'src/app/api/api-client.service';
import {Level} from 'src/app/api/types/level';
import {Score} from 'src/app/api/types/score';
import {DropdownOption} from 'src/app/components/form-dropdown/form-dropdown.component';
import {ActivityPage} from "../../api/types/activity/activity-page";
import {GenerateEmptyList} from "../../app.component";
import {faCircleCheck, faPencil, faPlay} from "@fortawesome/free-solid-svg-icons";
import {ExtendedUser} from "../../api/types/extended-user";
import {UserRoles} from "../../api/types/user-roles";
import {EmbedService} from "../../services/embed.service";
import {TitleService} from "../../services/title.service";
import {GameVersion} from "../../api/types/game-version";
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-level',
    templateUrl: './level.component.html'
})
export class LevelComponent implements OnInit {
    level: Level | undefined
    scores: Score[] | undefined
    activity: ActivityPage | undefined
    ownUser: ExtendedUser | undefined;

    isOwnUserOnline: boolean = false;

    scoreType: string = "1";

    scoreTypes: DropdownOption[] = [
        {
            Name: "1-player",
            Value: "1",
        },
        {
            Name: "2-players",
            Value: "2",
        },
        {
            Name: "3-players",
            Value: "3",
        },
        {
            Name: "4-players",
            Value: "4",
        },
    ]

    constructor(private authService: AuthService, private apiClient: ApiClient, private router: Router, private route: ActivatedRoute, private embedService: EmbedService, private titleService: TitleService) {
    }

    ngOnInit(): void {
        this.route.paramMap.subscribe((params: ParamMap) => {
            const id = params.get('id') as number | null;
            if (id == null) return;
            this.apiClient.GetLevelById(id)
                .pipe(catchError((error: HttpErrorResponse, caught) => {
                    console.warn(error)
                    if (error.status === 404) {
                        this.router.navigate(["/404"]);
                        return of(undefined)
                    }

                    return caught;
                }))
                .subscribe(data => {
                    this.level = data;
                    if (this.level === undefined) return;

                    this.getScores(this.level.levelId).subscribe();
                    this.getActivity(this.level.levelId);
                    this.embedService.embedLevel(this.level);
                    this.titleService.setTitle(this.level.title);
                });
        });

        this.ownUser = this.authService.user;
        this.authService.userWatcher.subscribe((data) => {
            this.ownUser = data;
        });

        this.authService.GetMyRoom().subscribe(data => {
            this.isOwnUserOnline = data != undefined;
        })
    }

    formChanged() {
        if (this.level === undefined) return;
        this.getScores(this.level.levelId).subscribe()
    }

    loadMoreScores() {
        if (this.level === undefined) return;
        if (this.scores === undefined) return;

        this.getScores(this.level.levelId, false, this.scores.length + 1).subscribe();
    }

    getGameVersion(version: number): string {
        return GameVersion[version].replace("LittleBigPlanet", "LBP");
    }

    getScores(levelId: number, clear: boolean = true, skip: number = 0) {
        return this.apiClient.GetScoresForLevel(levelId, Number(this.scoreType), skip)
            .pipe(
                catchError((error: HttpErrorResponse) => {
                    console.warn(error);
                    return of(undefined);
                }),
                tap((data) => {
                    if (data === undefined) return;

                    if (clear || this.scores == undefined) {
                        this.scores = data.items;
                    } else {
                        this.scores = this.scores.concat(data.items);
                    }


                    let rank = 0;
                    let i = 0;
                    for (let score of this.scores) {
                        const lastScore: Score | undefined = this.scores[i - 1];
                        if (lastScore?.score == score.score) {
                            rank -= 1;
                        }

                        rank++;
                        i++;
                        score.rank = rank;
                    }
                })
            );
    }

    getActivity(levelId: number, skip: number = 0) {
        this.apiClient.GetActivityForLevel(levelId, 10, skip).subscribe((data) => {
            this.activity = data;
        })
    }

    setAsOverride() {
        if (!this.level) return;
        this.apiClient.SetLevelAsOverride(this.level);
    }

    protected readonly GetAssetImageLink = GetAssetImageLink;
    protected readonly GenerateEmptyList = GenerateEmptyList;
    protected readonly UserRoles = UserRoles;
    protected readonly faCircleCheck = faCircleCheck;
    protected readonly faPencil = faPencil;
    protected readonly faPlay = faPlay;
}
