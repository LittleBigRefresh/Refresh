import {Component, Input} from '@angular/core';
import {Level} from "../../../api/types/level";
import {GetAssetImageLink} from "../../../api/api-client.service";

@Component({
    selector: 'level-link',
    templateUrl: './level-link.component.html'
})
export class LevelLinkComponent {
    _level: Level | undefined = undefined;

    @Input()
    set level(param: Level) {
        this._level = param;
    }

    protected readonly GetAssetImageLink = GetAssetImageLink;
}
