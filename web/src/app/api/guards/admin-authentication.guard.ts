import {CanActivateFn, Router} from "@angular/router";
import {inject} from "@angular/core";
import {AuthService} from "../auth.service";
import {BannerService} from "../../banners/banner.service";
import {ExtendedUser} from "../types/extended-user";
import {UserRoles} from "../types/user-roles";

export const adminAuthenticationGuard: CanActivateFn = () => {
    const user: ExtendedUser | undefined = inject(AuthService).user;
    if (!user || user.role < UserRoles.Admin) {
        inject(Router).navigate(['/']);
        inject(BannerService).pushError("Unauthorized", "You lack the permissions to view this page.")
        return false;
    }

    return true;
}
