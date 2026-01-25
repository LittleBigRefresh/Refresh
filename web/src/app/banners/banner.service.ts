import {Injectable} from "@angular/core";
import {Banner} from "./banner";

@Injectable({providedIn: 'root'})
export class BannerService {
    banners: Banner[] = []

    push(notification: Banner) {
        this.banners.push(notification);
    }

    dismiss(id: number): void {
        this.banners.splice(id, 1);
    }

    pushSuccess(title: string, text: string) {
        this.push({
            Color: 'success',
            Icon: 'check-circle',
            Text: text,
            Title: title,
        })
    }

    pushWarning(title: string, text: string) {
        this.push({
            Color: 'warning',
            Icon: 'warning',
            Text: text,
            Title: title,
        })
    }

    pushError(title: string, text: string) {
        this.push({
            Color: 'dangerous',
            Icon: 'exclamation-circle',
            Text: text,
            Title: title,
        })
    }
}
