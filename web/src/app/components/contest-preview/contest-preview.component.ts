import {Component, Input} from '@angular/core';
import {Level} from "../../api/types/level";
import {Contest} from "../../api/types/contests/contest";

@Component({
  selector: 'app-contest-preview',
  templateUrl: './contest-preview.component.html',
})
export class ContestPreviewComponent {
    _contest: Contest | undefined = undefined;
    @Input()
    set contest(contest: Contest | undefined) {
        this._contest = contest;
    }
}
