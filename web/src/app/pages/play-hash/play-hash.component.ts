import { Component } from '@angular/core';
import {faCertificate, faPlay, faTriangleExclamation} from "@fortawesome/free-solid-svg-icons";
import {ApiClient} from "../../api/api-client.service";
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'app-play-hash',
  templateUrl: './play-hash.component.html'
})
export class PlayHashComponent {
    protected readonly faCertificate = faCertificate;
    protected readonly faPlay = faPlay;

    levelHash: string = "";

    constructor(private api: ApiClient, private route: ActivatedRoute) {
        this.route.queryParams.subscribe(params => {
            if (params['hash'] !== undefined) {
                this.levelHash = params['hash'];
            }
        });
    }

    setAsOverride() {
        if(this.levelHash.length <= 0) return;

        this.api.SetLevelHashAsOverride(this.levelHash);
    }

    protected readonly faTriangleExclamation = faTriangleExclamation;
}
