import {Component} from '@angular/core';
import {BannerService} from 'src/app/banners/banner.service';

@Component({
    selector: 'app-ui-debug',
    templateUrl: './ui-debug.component.html'
})
export class UiDebugComponent {
    constructor(private bannerService: BannerService) {
    }

    successBanner(): void {
        this.bannerService.pushSuccess("Nice", "Rocket Launch Good");
    }

    warnBanner(): void {
        this.bannerService.pushWarning("Uh", "You done goofed");
    }

    errorBanner(): void {
        this.bannerService.pushError("BAD", "You screwed up so bad bro");
    }
}
