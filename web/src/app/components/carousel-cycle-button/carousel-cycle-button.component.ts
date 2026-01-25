import {Component, Input} from '@angular/core';
import {faChevronCircleLeft, faChevronCircleRight} from "@fortawesome/free-solid-svg-icons";
import {IconProp} from "@fortawesome/fontawesome-svg-core";

@Component({
    selector: 'app-carousel-cycle-button',
    templateUrl: './carousel-cycle-button.component.html'
})
export class CarouselCycleButtonComponent {
    @Input("side") side: "left" | "right" = "left";

    getIconForSide(): IconProp {
        switch (this.side) {
            case "left":
                return faChevronCircleLeft;
            case "right":
                return faChevronCircleRight;
        }
    }
}
