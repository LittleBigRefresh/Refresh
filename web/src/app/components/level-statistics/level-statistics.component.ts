import {Component, Input} from '@angular/core';
import {faHeart, faPlay, faStar, faThumbsDown, faThumbsUp} from "@fortawesome/free-solid-svg-icons";
import {Level} from "../../api/types/level";

@Component({
    selector: 'level-statistics',
    templateUrl: './level-statistics.component.html'
})
export class LevelStatisticsComponent {
    @Input('level') level: Level | undefined;

    protected readonly faHeart = faHeart;
    protected readonly faPlay = faPlay;
    protected readonly faThumbsUp = faThumbsUp;
    protected readonly faThumbsDown = faThumbsDown;
    protected readonly faStar = faStar;
}
