import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, ParamMap, Params, Router} from '@angular/router';
import {catchError, of} from 'rxjs';
import {ApiClient} from 'src/app/api/api-client.service';
import {Level} from 'src/app/api/types/level';
import {HttpErrorResponse} from '@angular/common/http';
import {GenerateEmptyList, masonryOptions} from "../../app.component";
import {Category} from "../../api/types/category";
import {TitleService} from "../../services/title.service";
import {EmbedService} from "../../services/embed.service";

const pageSize: number = 10;

@Component({
    selector: 'app-level-listing',
    templateUrl: './level-listing.component.html',
})
export class LevelListingComponent implements OnInit {
    levels: Level[] | undefined = undefined;
    routeCategory: Category | undefined = undefined;
    apiRoute: string = '';
    categories: Category[] = [];
    params: Params = {};
    nextPageIndex: number = pageSize + 1;
    total: number = 0;

    constructor(private apiClient: ApiClient, private router: Router, private route: ActivatedRoute, private titleService: TitleService, private embedService: EmbedService) {
    }

    ngOnInit(): void {
        this.route.paramMap.subscribe((params: ParamMap) => {
            const apiRoute: string | null = params.get('route');
            if (apiRoute == null) return;
            this.apiRoute = apiRoute;

            this.route.queryParams.subscribe((params: Params) => {
                this.params = params;
                this.loadLevels();
            });
        })
    }

    loadLevels() {
        const pipe = this.apiClient.GetLevelListing(this.apiRoute, pageSize, 0, this.params)
            .pipe(catchError((error: HttpErrorResponse, caught) => {
                console.warn(error)
                if (error.status === 404) {
                    this.router.navigate(["/404"]);
                    return of();
                }
                return caught;
            }));

        pipe.subscribe(data => {
            this.levels = data.items;
            this.total = data.listInfo.totalItems;
        })

        const categoryPipe = this.apiClient.GetLevelCategories().pipe(catchError((error: HttpErrorResponse, caught) => {
            console.warn(error)
            return caught;
        }));
        categoryPipe.subscribe(data => {
            this.categories = data;
            this.routeCategory = this.getCategory(this.apiRoute);
            this.titleService.setTitle(this.routeCategory.name);
            this.embedService.embed(this.routeCategory.name, this.routeCategory.description);
        })
    }

    loadNextPage(intersecting: boolean): void {
        if (!intersecting) return;

        if (this.nextPageIndex <= 0) return; // This is the server telling us there's no more data

        this.apiClient.GetLevelListing(this.apiRoute, pageSize, this.nextPageIndex, this.params).subscribe((data) => {
            this.levels = this.levels!.concat(data.items);
            this.nextPageIndex = data.listInfo.nextPageIndex;
            this.total = data.listInfo.totalItems;
        });
    }

    // Instead of just showing the route in PascalCase in the level category, we can process the route and make it look nicer.
    private getCategory(apiRoute: string): Category {
        const category = this.categories.find(cat => cat.apiRoute === apiRoute);
        if (!category) {
            throw new Error("Matching category was not found in API")
        }
        return category;
    }

    protected readonly masonryOptions = masonryOptions;
    protected readonly GenerateEmptyList = GenerateEmptyList;
}
