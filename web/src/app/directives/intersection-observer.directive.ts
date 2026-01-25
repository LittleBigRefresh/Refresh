import {AfterViewInit, Directive, ElementRef, EventEmitter, Inject, Input, Output, PLATFORM_ID} from '@angular/core';
import {isPlatformBrowser} from "@angular/common";

@Directive({
    selector: '[intersectionObserver]'
})
export class IntersectionObserverDirective implements AfterViewInit {
    private readonly isBrowser: boolean;

    constructor(private element: ElementRef, @Inject(PLATFORM_ID) platformId: Object) {
        this.isBrowser = isPlatformBrowser(platformId);
    }

    @Input() root!: HTMLElement;
    @Output() visibilityChange = new EventEmitter<boolean>;

    ngAfterViewInit(): void {
        if (!this.isBrowser) return;

        const element: HTMLElement = this.element.nativeElement;

        const config: IntersectionObserverInit = {
            root: this.root,
            rootMargin: "0px",
            threshold: 0.5,
        };

        const observer: IntersectionObserver = new IntersectionObserver((entries: IntersectionObserverEntry[]) => {
            for (let entry of entries) {
                this.visibilityChange.emit(entry.isIntersecting);
            }
        }, config)

        observer.observe(element);
    }
}
