import {Component, OnInit} from '@angular/core';
import {ActivityPage} from "../../api/types/activity/activity-page";
import {ApiClient} from "../../api/api-client.service";
import {GenerateEmptyList} from "../../app.component";
import {TitleService} from "../../services/title.service";

const pageSize: number = 20;

@Component({
    selector: 'app-activity',
    templateUrl: './activity.component.html'
})
export class ActivityComponent implements OnInit {
    constructor(private apiClient: ApiClient, titleService: TitleService) {
        titleService.setTitle("Recent Activity");
    }

    activity: ActivityPage[] | undefined;
    private pageNumber: number = 0;
    nextPageIndex: number = pageSize + 1;

    ngOnInit(): void {
        this.getActivity();
    }

    getActivity(skip: number = 0) {
        this.apiClient.GetActivity(pageSize, skip).subscribe((data) => {
            if (this.activity == undefined) this.activity = [];
            this.activity = this.activity.concat(data);

            if (data.events.length !== 0) {
                this.pageNumber++;
                this.nextPageIndex = (pageSize * this.pageNumber) + 1;
            } else {
                this.nextPageIndex = 0;
            }
        })
    }

    loadNextPage(intersecting: boolean): void {
        if (!intersecting) return;

        if (this.nextPageIndex <= 0) return; // This is the server telling us there's no more data

        this.getActivity(this.nextPageIndex);
    }

    protected readonly GenerateEmptyList = GenerateEmptyList;
}
