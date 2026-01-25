import {Component, OnInit} from '@angular/core';
import {IconName, IconProp} from '@fortawesome/fontawesome-svg-core';
import {ApiClient, GetAssetImageLink} from 'src/app/api/api-client.service';
import {Category} from 'src/app/api/types/category';
import {GenerateEmptyList, masonryOptions} from "../../app.component";
import {TitleService} from "../../services/title.service";
import {faLink} from "@fortawesome/free-solid-svg-icons";
import {EmbedService} from "../../services/embed.service";
import {AuthService} from "../../api/auth.service";

@Component({
    selector: 'app-level-categories',
    templateUrl: './level-categories.component.html',
})
export class LevelCategoriesComponent implements OnInit {
    categories: Category[] | undefined = undefined!

    title: string = "Level Categories";
    description: string = "Discover and browse through new levels using categories!";

    constructor(private apiClient: ApiClient, private authService: AuthService, titleService: TitleService, embedService: EmbedService) {
        titleService.setTitle(this.title);
        embedService.embed(this.title, this.description);
    }

    ngOnInit(): void {
        this.apiClient.GetLevelCategories()
            .subscribe(data => {
                if (data == undefined) return;
                this.categories = []

                for (let c of data) {
                    // if the endpoint requires a user, but we're not signed in, skip it
                    // if either of those conditions aren't met add it to the list

                    if (c.requiresUser) {
                        if (this.authService.user !== undefined)
                            this.categories.push(c);
                    } else if (c.hidden) {
                        // do nothing
                    } else {
                        this.categories.push(c);
                    }
                }
            });
    }

    getIcon(name: string): IconProp {
        return name as IconName;
    }

    protected readonly GetAssetImageLink = GetAssetImageLink;
    protected readonly GenerateEmptyList = GenerateEmptyList;
    protected readonly masonryOptions = masonryOptions;
    protected readonly faLink = faLink;
}
