import {animate, animateChild, query, style, transition, trigger} from '@angular/animations';
import {Component, HostBinding, Input} from '@angular/core';
import {IconProp} from '@fortawesome/fontawesome-svg-core';
import {BannerService} from 'src/app/banners/banner.service';

@Component({
    selector: 'app-banner',
    templateUrl: './banner.component.html',
    animations: [
        trigger('fadeIn', [
            transition(':enter', [
                style({opacity: 0, height: 0, 'padding-top': 0, 'padding-bottom': 0}),
                animate('500ms ease-in-out', style({
                    opacity: 1,
                    height: '*',
                    'padding-top': '*',
                    'padding-bottom': '*'
                })),
            ]),
        ]),
        trigger('fadeOut', [
            transition(':leave', [
                query('@*', animateChild(), {optional: true}),
                animate('300ms ease-in-out', style({
                    opacity: 0,
                    height: 0,
                    'padding-top': 0,
                    'padding-bottom': 0,
                    'border-bottom': 0
                })),
            ])
        ])
    ]
})
export class BannerComponent {
    // Animation
    @HostBinding('@fadeOut') animation = true;

    // Styling
    _color: string = 'indigo'
    _icon: IconProp = 'poo'

    @Input()
    set color(param: string) {
        this._color = param;
    }

    @Input()
    set icon(param: IconProp) {
        this._icon = param;
    }

    // Text
    _title: string = "NO TITLE, FIX ME"
    _text: string = "NO TEXT, FIX ME"

    @Input()
    set title(param: string) {
        this._title = param;
    }

    @Input()
    set text(param: string) {
        this._text = param;
    }

    // Logic
    _id: number = -1

    @Input()
    set id(param: number) {
        this._id = param;
    }

    constructor(private bannerService: BannerService) {
    }

    dismiss(): void {
        if (this._id == -1) throw Error("Can't dismiss a banner with no ID");

        this.bannerService.dismiss(this._id);
    }
}
