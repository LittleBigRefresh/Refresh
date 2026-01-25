import { Component } from '@angular/core';
import {Photo} from "../../api/types/photo";
import {ApiClient} from "../../api/api-client.service";
import {TitleService} from "../../services/title.service";
import {Contest} from "../../api/types/contests/contest";

@Component({
  selector: 'app-contests',
  templateUrl: './contests.component.html'
})
export class ContestsComponent {
    contests: Contest[] | undefined = undefined;
    total: number = 0;

    constructor(private apiClient: ApiClient, titleService: TitleService) {
        titleService.setTitle("Contests");
    }

    ngOnInit(): void {
        this.apiClient.GetContests().subscribe((data) => {
            this.contests = data;
        })
    }
}
