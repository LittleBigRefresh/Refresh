import {AfterViewInit, Component, ElementRef, Inject, PLATFORM_ID, ViewChild} from '@angular/core';
import {isPlatformBrowser} from "@angular/common";

@Component({
    selector: 'carousel',
    templateUrl: './carousel.component.html'
})
export class CarouselComponent implements AfterViewInit {
    private readonly isBrowser: boolean;

    constructor(@Inject(PLATFORM_ID) platformId: Object) {
        this.isBrowser = isPlatformBrowser(platformId);
    }

    @ViewChild("items") itemsHolder: ElementRef = null!;
    carouselItems: HTMLElement[] = [];

    currentIndex: number = 0;
    shownItems: number = 0;

    ngAfterViewInit(): void {
        this.carouselItems = Array.from<HTMLElement>(this.itemsHolder.nativeElement.children);
        this.updateCarousel()
    }

    updateCarousel(): void {
        if(!this.isBrowser) return;
        const i: number = this.currentIndex;

        for (let carouselItem of this.carouselItems) {
            this.hideItem(carouselItem);
        }

        let top: number = 0;
        this.shownItems = 0;
        for (let carouselItem of this.carouselItems.slice(i, this.carouselItems.length)) {
            this.showItem(carouselItem);

            const thisTop: number = carouselItem.getBoundingClientRect().top;
            if (top !== 0 && thisTop !== top) {
                this.hideItem(carouselItem);
                break;
            }

            top = thisTop;
            this.shownItems++;
        }
    }

    hideItem(item: HTMLElement) {
        item.hidden = true;
    }

    showItem(item: HTMLElement) {
        item.hidden = false;
    }

    private clampIndex(value: number): number {
        const min: number = 0;
        const max: number = Math.min(this.carouselItems.length, this.carouselItems.length - this.shownItems);

        return Math.min(Math.max(value, min), max);
    }

    increment(): void {
        this.currentIndex = this.clampIndex(this.currentIndex + 1);
        this.updateCarousel();
    }

    canIncrement(): boolean {
        return this.clampIndex(this.currentIndex + 1) > this.currentIndex;
    }

    decrement(): void {
        this.currentIndex = this.clampIndex(this.currentIndex - 1);
        this.updateCarousel();
    }

    canDecrement(): boolean {
        return this.clampIndex(this.currentIndex - 1) < this.currentIndex;
    }
}
