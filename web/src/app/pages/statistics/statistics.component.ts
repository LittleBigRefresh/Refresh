import {Component} from '@angular/core';
import {Instance} from "../../api/types/instance";
import {Statistics} from "../../api/types/statistics";
import {ApiClient} from "../../api/api-client.service";
import {RequestStatistics} from "../../api/types/request-statistics";

@Component({
    selector: 'app-statistics',
    templateUrl: './statistics.component.html'
})
export class StatisticsComponent {
    instance: Instance = undefined!;
    statistics: Statistics = undefined!;
    requests: RequestStatistics = undefined!;

    constructor(apiClient: ApiClient) {
        apiClient.GetInstanceInformation().subscribe(data => {
            this.instance = data;
        })

        apiClient.GetServerStatistics().subscribe(data => {
            this.statistics = data;
            this.requests = this.statistics.requestStatistics;
        })
    }
}
