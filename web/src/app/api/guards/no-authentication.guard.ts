import {CanActivateFn, Router} from "@angular/router";
import {ExtendedUser} from "../types/extended-user";
import {inject} from "@angular/core";
import {AuthService} from "../auth.service";

export const noAuthenticationGuard: CanActivateFn = () => {
    const user: ExtendedUser | undefined = inject(AuthService).user;
    if (user) {
        inject(Router).navigate(['/']);
        return false;
    }

    return true;
}
