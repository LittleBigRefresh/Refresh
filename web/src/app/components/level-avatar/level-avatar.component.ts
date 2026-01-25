import {Component, Input} from '@angular/core';
import {GetAssetImageLink} from "../../api/api-client.service";
import {Level} from "../../api/types/level";

@Component({
    selector: 'level-avatar',
    templateUrl: './level-avatar.component.html'
})
export class LevelAvatarComponent {
    _level: Level = undefined!;
    _size: string = "";

    @Input() set level(level: Level) {
        this._level = level;
    }

    @Input() set size(size: string) {
        this._size = size;
    }

    protected readonly GetAssetImageLink = GetAssetImageLink;
}
