import {Component, Input} from '@angular/core';
import {Contest} from "../../api/types/contests/contest";

@Component({
  selector: 'app-contest-banner',
  templateUrl: './contest-banner.component.html'
})
export class ContestBannerComponent {
    @Input({required: true}) contest: Contest = null!;
}
