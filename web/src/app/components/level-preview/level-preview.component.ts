import {Component, Input} from '@angular/core';
import {Level} from "../../api/types/level";
import {GetAssetImageLink} from "../../api/api-client.service";
import {GameVersion} from "../../api/types/game-version";
import {faCircleCheck} from "@fortawesome/free-solid-svg-icons";

@Component({
    selector: 'level-preview',
    templateUrl: './level-preview.component.html'
})
export class LevelPreviewComponent {
    _level: Level | undefined = undefined;
    _description: boolean = false;

    @Input()
    set level(level: Level | undefined) {
        this._level = level;
    }

    @Input()
    set description(description: boolean) {
        this._description = description;
    }

    getGameVersion(version: number): string {
        return GameVersion[version].replace("LittleBigPlanet", "LBP");
    }

    protected readonly GetAssetImageLink = GetAssetImageLink;
    protected readonly faCircleCheck = faCircleCheck;
}
