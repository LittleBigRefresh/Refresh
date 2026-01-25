import {Component, Input} from '@angular/core';
import {GetAssetImageLink} from "../../api/api-client.service";
import {User} from "../../api/types/user";

@Component({
    selector: 'user-avatar',
    templateUrl: './user-avatar.component.html'
})
export class UserAvatarComponent {
    _user: User = undefined!;
    _size: string = "";

    @Input() set user(user: User) {
        this._user = user;
    }

    @Input() set size(size: string) {
        this._size = size;
    }

    protected readonly GetAssetImageLink = GetAssetImageLink;
}
