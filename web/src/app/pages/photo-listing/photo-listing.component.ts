import {Component, OnInit} from '@angular/core';
import {ApiClient} from 'src/app/api/api-client.service';
import {Photo} from 'src/app/api/types/photo';
import {GenerateEmptyList, masonryOptions} from "../../app.component";
import {TitleService} from "../../services/title.service";

const pageSize: number = 10;

@Component({
    selector: 'app-photos',
    templateUrl: './photo-listing.component.html'
})
export class PhotoListingComponent implements OnInit {
    photos: Photo[] | undefined = undefined;
    nextPageIndex: number = pageSize + 1;
    total: number = 0;

    constructor(private apiClient: ApiClient, titleService: TitleService) {
        titleService.setTitle("Photos");
    }

    ngOnInit(): void {
        this.apiClient.GetRecentPhotos(pageSize).subscribe((data) => {
            this.photos = data.items;
            this.total = data.listInfo.totalItems;
        })
    }

    loadNextPage(intersecting: boolean): void {
        if (!intersecting) return;

        if (this.nextPageIndex <= 0) return; // This is the server telling us there's no more data
        this.apiClient.GetRecentPhotos(pageSize, this.nextPageIndex).subscribe((data) => {
            this.photos = this.photos!.concat(data.items);
            this.nextPageIndex = data.listInfo.nextPageIndex;
            this.total = data.listInfo.totalItems;
        })
    }

    protected readonly masonryOptions = masonryOptions;
    protected readonly GenerateEmptyList = GenerateEmptyList;
}
