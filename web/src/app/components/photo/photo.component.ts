import {Component, Input} from '@angular/core';
import {Photo} from "../../api/types/photo";
import {GetPhotoLink} from "../../api/api-client.service";

@Component({
    selector: 'photo',
    templateUrl: './photo.component.html'
})
export class PhotoComponent {
    _photo: Photo | undefined = undefined;
    _link: boolean = true;

    @Input()
    set photo(param: Photo | undefined) {
        this._photo = param;
    }

    @Input()
    set link(param: boolean) {
        this._link = param;
    }

    protected readonly GetPhotoLink = GetPhotoLink;
    protected readonly undefined = undefined;
}
