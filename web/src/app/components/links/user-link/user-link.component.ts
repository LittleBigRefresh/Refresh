import {Component, Input} from '@angular/core';
import {User} from "../../../api/types/user";
import {GetAssetImageLink} from "../../../api/api-client.service";

@Component({
    selector: 'user-link',
    templateUrl: './user-link.component.html'
})
export class UserLinkComponent {
    _user: User | undefined = undefined;

    @Input()
    set user(param: User) {
        this._user = param;
    }

    protected readonly GetAssetImageLink = GetAssetImageLink;
}
