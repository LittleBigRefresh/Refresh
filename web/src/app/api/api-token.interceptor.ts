import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {Observable} from "rxjs";
import {TokenStorageService} from "./token-storage.service";

@Injectable({
    providedIn: 'root'
})
export class ApiTokenInterceptor implements HttpInterceptor {
    constructor(private tokenStorage: TokenStorageService) {
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const storedToken: string | null = this.tokenStorage.GetStoredGameToken();
        if (storedToken) {
            const reqClone = req.clone({
                setHeaders: {
                    'Authorization': storedToken
                }
            });
            return next.handle(reqClone);
        }

        return next.handle(req);
    }
}
