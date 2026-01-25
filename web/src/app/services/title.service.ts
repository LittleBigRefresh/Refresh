import {Injectable} from "@angular/core";
import {Title} from "@angular/platform-browser";
import {ApiClient} from "../api/api-client.service";
import {NavigationStart, Router} from "@angular/router";

@Injectable({providedIn: 'root'})
export class TitleService {
    private instanceName: string = "Refresh";
    private currentTitle: string = "";

    constructor(private title: Title, private apiClient: ApiClient, router: Router) {
        router.events.subscribe((val) => {
            if (!(val instanceof NavigationStart)) return;
            this.setTitle("");
        });

        this.apiClient.GetInstanceInformation().subscribe((data) => {
            this.instanceName = data.instanceName;
            this.setTitle(this.currentTitle);
        });
    }

    public setTitle(title: string) {
        if (title.length == 0) {
            this.title.setTitle(this.instanceName);
            this.currentTitle = "";
            return;
        }

        this.title.setTitle(`${title} - ${this.instanceName}`)
        this.currentTitle = title;
    }
}
