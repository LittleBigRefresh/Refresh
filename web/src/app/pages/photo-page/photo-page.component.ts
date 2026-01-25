import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, ParamMap} from '@angular/router';
import {catchError, EMPTY, of, switchMap} from 'rxjs';
import {ApiClient} from 'src/app/api/api-client.service';
import {Photo} from 'src/app/api/types/photo';
import {EmbedService} from "../../services/embed.service";
import {TitleService} from "../../services/title.service";

@Component({
    selector: 'app-photo',
    templateUrl: './photo-page.component.html'
})
export class PhotoPageComponent implements OnInit {
    photo: Photo | undefined | null = null

    constructor(private apiClient: ApiClient, private route: ActivatedRoute, private embedService: EmbedService, private titleService: TitleService) {
    }

    ngOnInit(): void {
        this.route.paramMap.pipe(switchMap((params: ParamMap) => {
            const id = Number(params.get("id")) as number | undefined;
            if (id === undefined) {
                return EMPTY;
            }

            return this.apiClient.GetPhotoById(id);
        }))
            .pipe(catchError(() => {
                return of(undefined);
            }))
            .subscribe((data) => {
                this.photo = data;
                if (data !== undefined) {
                    this.embedService.embedPhoto(data);
                    this.titleService.setTitle("Photo by " + data.publisher.username);
                }
            });
    }
}
