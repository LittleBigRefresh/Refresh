import {Component, OnInit} from '@angular/core';
import {ApiClient} from "../../api/api-client.service";
import {Route} from "../../api/types/documentation/route";

@Component({
    selector: 'app-documentation',
    templateUrl: './documentation.component.html'
})
export class DocumentationComponent implements OnInit {
    routes: Route[] | undefined = undefined;

    constructor(private apiClient: ApiClient) {
    }

    ngOnInit() {
        this.apiClient.GetDocumentation().subscribe((data) => {
            this.routes = data.items;
        })
    }
}
