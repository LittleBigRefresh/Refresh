import {Component, Input} from '@angular/core';
import {Level} from "../../api/types/level";
import {IconProp} from "@fortawesome/fontawesome-svg-core";
import {Params} from "@angular/router";

@Component({
    selector: 'category-preview',
    templateUrl: './category-preview.component.html'
})
export class CategoryPreviewComponent {
    @Input("icon") public icon: IconProp | undefined;
    @Input("category-title") public title: string = "";
    @Input("route") public route: string = "";
    @Input("levels") public levels: Level[] | undefined;
    @Input("total") public total: number = 0;
    @Input("query") public params: Params = {};
    @Input("type") public type: CategoryPreviewType = CategoryPreviewType.Carousel;

    protected readonly CategoryPreviewType = CategoryPreviewType;
}

export enum CategoryPreviewType {
    Carousel,
    List
}
