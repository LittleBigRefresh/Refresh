import {CanActivateFn, Router} from "@angular/router";
import {AuthService} from "../auth.service";
import {inject} from "@angular/core";
import {BannerService} from "../../banners/banner.service";

// Adapted from SoundShapes-web
// https://github.com/turecross321/soundshapes-web/blob/master/src/app/auth/auth.guard.ts

export const authenticationGuard: CanActivateFn = () => {
    if (!inject(AuthService).user) {
        inject(Router).navigate(['/login']);
        inject(BannerService).pushWarning("Not logged in", "This page requires that you log in or register.")
        return false;
    }

    return true;
}
