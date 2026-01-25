import {Component, OnInit} from '@angular/core';
import {ApiClient} from 'src/app/api/api-client.service';
import {Statistics} from 'src/app/api/types/statistics';
import {Instance} from "../../api/types/instance";
import {Level} from "../../api/types/level";
import {
    faArrowRightToBracket,
    faBullhorn,
    faCalendar,
    faFire,
    faFireAlt,
    faPlayCircle,
    faUserPlus
} from "@fortawesome/free-solid-svg-icons";
import {ActivityPage} from "../../api/types/activity/activity-page";
import {ApiListResponse} from "../../api/types/response/api-list-response";
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-main',
    templateUrl: './main.component.html',
})
export class MainComponent implements OnInit {
    statistics: Statistics | undefined;
    instance: Instance | undefined;

    busyLevels: ApiListResponse<Level> | undefined;
    coolLevels: ApiListResponse<Level> | undefined;
    pickedLevels: ApiListResponse<Level> | undefined;

    activity: ActivityPage | undefined;

    constructor(public authService: AuthService, public apiClient: ApiClient) {
    }

    ngOnInit(): void {
        this.apiClient.GetServerStatistics()
            .subscribe(data => {
                this.statistics = data;
            });

        this.apiClient.GetInstanceInformation()
            .subscribe(data => {
                this.instance = data;
            });

        this.apiClient.GetLevelListing("teamPicks", 10, 0)
            .subscribe(data => {
                this.pickedLevels = data;
            });

        this.apiClient.GetLevelListing("currentlyPlaying", 10, 0)
            .subscribe(data => {
                this.busyLevels = data;
            });

        this.apiClient.GetLevelListing("coolLevels", 10, 0)
            .subscribe(data => {
                this.coolLevels = data;
            });

        this.apiClient.GetActivity(5, 0)
            .subscribe(data => {
                this.activity = data;
            })
    }

    protected readonly faFireAlt = faFireAlt;
    protected readonly faBullhorn = faBullhorn;
    protected readonly faPlayCircle = faPlayCircle;
    protected readonly faArrowRightToBracket = faArrowRightToBracket;
    protected readonly faUserPlus = faUserPlus;
    protected readonly faCalendar = faCalendar;
    protected readonly faFire = faFire;
}
