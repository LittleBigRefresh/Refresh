import {ApiResponse} from "./types/response/api-response";
import {ApiError} from "./types/response/api-error";
import {catchError, Observable, of, switchMap} from "rxjs";
import {environment} from "../../environments/environment";
import {ApiListResponse} from "./types/response/api-list-response";
import {HttpClient} from "@angular/common/http";
import {BannerService} from "../banners/banner.service";
import {Injectable} from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class ApiRequestCreator {
    private handledConnectionError: boolean = false;

    constructor(private httpClient: HttpClient, private bannerService: BannerService) {
    }

    private handleRequestError<T>(data: ApiResponse<T>, err: any, catchErrors: boolean) {
        if (!catchErrors) {
            const respError: ApiError = data.error!;
            return of(respError);
        }

        if (err.status == 0) {
            if (!this.handledConnectionError) {
                this.bannerService.pushError("Failed to connect", "We couldn't reach Refresh's backend. Please try again later in just a couple moments.")
            }

            this.handledConnectionError = true;
            return of(undefined);
        }

        if (data?.error === undefined && err.status == 404) {
            this.bannerService.pushError("Not Found", "The requested resource could not be found.")
            return of(undefined);
        }

        if (data?.error === undefined && err.status == 403) {
            this.bannerService.pushError("Forbidden", "Unfortunately, you lack the permissions to access this data.")
            return of(undefined);
        }

        if (data?.error === undefined && err.status == 500) {
            this.bannerService.pushError("Internal Server Error", "The remote server couldn't handle your request.")
            return of(undefined);
        }

        this.bannerService.pushError(`API Error: ${data?.error?.name} (${err.status})`, data?.error?.message ?? "Unknown error")
        return of(undefined);
    }

    public makeRequest<T>(method: string, endpoint: string, body: any = null, errorHandler: ((error: ApiError) => void) | undefined = undefined): Observable<T> {
        let result: Observable<ApiResponse<T> | (T | undefined)> = this.httpClient.request<ApiResponse<T>>(method, environment.apiBaseUrl + '/' + endpoint, {
            body: body
        });

        // @ts-ignore
        result = result.pipe(
            // @ts-ignore
            catchError((err) => {
                if (!err.success) {
                    console.log("Handling error")
                    if (errorHandler) {
                        let error: ApiError | undefined = err.error?.error;
                        if (error == undefined) {
                            error = {
                                warning: err.ok,
                                message: err.message,
                                name: err.statusText,
                                statusCode: err.status,
                            }
                        }

                        errorHandler(error!);
                    }
                    return this.handleRequestError(err.error, err, errorHandler == undefined);
                }

                return of(undefined);
            }),
            switchMap((resp: ApiResponse<T>) => {
                    if (resp === undefined) return of(undefined);
                    return of(resp.data);
                }
            ));

        // @ts-ignore
        return result;
    }

    public makeListRequest<T>(method: string, endpoint: string, catchErrors: boolean = true): Observable<ApiListResponse<T>> {
        let result: Observable<ApiResponse<T[]> | (T[] | undefined)> = this.httpClient.request<ApiResponse<T[]>>(method, environment.apiBaseUrl + '/' + endpoint);

        // @ts-ignore
        result = result.pipe(
            // @ts-ignore
            catchError((err) => {
                if (!err.success) {
                    console.log("Handling error")
                    return this.handleRequestError(err.error, err, catchErrors);
                }

                return of(undefined);
            }),
            switchMap((respData: ApiResponse<T[]>) => {
                    if (respData === undefined) return of(undefined);

                    const resp: ApiListResponse<T> = {
                        items: respData.data!,
                        listInfo: respData.listInfo!,
                    };

                    return of(resp);
                }
            ));

        // @ts-ignore
        return result;
    }
}
