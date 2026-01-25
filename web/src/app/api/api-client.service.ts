import {Injectable} from "@angular/core";
import {Observable, tap} from "rxjs";
import {environment} from "src/environments/environment";
import {BannerService} from "../banners/banner.service";
import {Category} from "./types/category";
import {Level} from "./types/level";
import {User} from "./types/user";
import {Statistics} from "./types/statistics";
import {Room} from "./types/rooms/room";
import {Score} from "./types/score";
import {Photo} from "./types/photo";
import {RefreshNotification} from "./types/refresh-notification";
import {ApiError} from "./types/response/api-error";
import {Route} from "./types/documentation/route";
import {ActivityPage} from "./types/activity/activity-page";
import {ApiListResponse} from "./types/response/api-list-response";
import {IpVerificationRequest} from "./types/auth/ip-verification-request";
import {Instance} from "./types/instance";
import {ApiRequestCreator} from "./api-request.creator";
import {LevelEditRequest} from "./types/level-edit-request";
import {Asset} from "./types/asset";
import {Contest} from "./types/contests/contest";
import {ContestEditRequest} from "./types/contests/contest-edit-request";
import {Params} from "@angular/router";

@Injectable({providedIn: 'root'})
export class ApiClient {
    private categories: Category[] | undefined;

    private statistics: Statistics | undefined;
    private instance: Instance | undefined;

    constructor(private apiRequestCreator: ApiRequestCreator, private bannerService: BannerService) {
    }

    private makeRequest<T>(method: string, endpoint: string, body: any = null, errorHandler: ((error: ApiError) => void) | undefined = undefined): Observable<T> {
        return this.apiRequestCreator.makeRequest<T>(method, endpoint, body, errorHandler);
    }

    private makeListRequest<T>(method: string, endpoint: string, catchErrors: boolean = true): Observable<ApiListResponse<T>> {
        return this.apiRequestCreator.makeListRequest<T>(method, endpoint, catchErrors);
    }

    public GetServerStatistics(): Observable<Statistics> {
        if (this.statistics !== undefined) {
            return new Observable<Statistics>(observer => {
                observer.next(this.statistics!)
            });
        }

        return this.makeRequest<Statistics>("GET", "statistics")
            .pipe(tap(data => {
                this.statistics = data;
            }))
    }

    public GetInstanceInformation(): Observable<Instance> {
        if (this.instance !== undefined) {
            return new Observable<Instance>(observer => {
                observer.next(this.instance!)
            });
        }

        return this.makeRequest<Instance>("GET", "instance")
            .pipe(tap(data => {
                this.instance = data;
            }))
    }

    public GetUserByUsername(username: string): Observable<User> {
        return this.makeRequest<User>("GET", "users/name/" + username)
    }

    public GetUserByUuid(uuid: string): Observable<User> {
        return this.makeRequest<User>("GET", "users/uuid/" + uuid)
    }

    public GetLevelCategories(): Observable<Category[]> {
        if (this.categories !== undefined) {
            return new Observable<Category[]>(observer => {
                observer.next(this.categories!)
            });
        }

        return this.makeRequest<Category[]>("GET", "levels?includePreviews=true");
    }

    public GetLevelListing(route: string, count: number = 20, skip: number = 0, params: Params = {}): Observable<ApiListResponse<Level>> {
        let query: string = `count=${count}&skip=${skip}`;

        for (let param in params) {
            query += `&${param}=${params[param]}`
        }


        return this.makeListRequest<Level>("GET", `levels/${route}?${query}`);
    }

    public GetLevelById(id: number): Observable<Level> {
        return this.makeRequest<Level>("GET", "levels/id/" + id)
    }

    public GetUsersRoom(userUuid: string): Observable<Room> {
        return this.makeRequest<Room>("GET", "rooms/uuid/" + userUuid, null, (_: ApiError) => {
        })
    }

    public GetScoresForLevel(levelId: number, scoreType: number, skip: number): Observable<ApiListResponse<Score>> {
        return this.makeListRequest<Score>("GET", "scores/" + levelId + "/" + scoreType + "?showAll=false&count=10&skip=" + skip)
    }

    public GetRecentPhotos(count: number = 20, skip: number = 0) {
        return this.makeListRequest<Photo>("GET", "photos" + "?count=" + count + "&skip=" + skip);
    }

    public GetPhotoById(id: number) {
        return this.makeRequest<Photo>("GET", "photos/id/" + id);
    }

    public GetNotifications(): Observable<ApiListResponse<RefreshNotification>> {
        return this.makeListRequest<RefreshNotification>("GET", "notifications")
    }

    public ClearNotification(notificationId: string) {
        return this.makeRequest("DELETE", "notifications/" + notificationId);
    }

    public ClearAllNotifications() {
        return this.makeRequest("DELETE", "notifications");
    }

    public GetDocumentation() {
        return this.makeListRequest<Route>("GET", "documentation");
    }

    public GetActivity(count: number, skip: number): Observable<ActivityPage> {
        return this.makeRequest<ActivityPage>("GET", `activity?skip=${skip}&count=${count}`);
    }

    public GetActivityForLevel(levelId: number, count: number, skip: number): Observable<ActivityPage> {
        return this.makeRequest<ActivityPage>("GET", "levels/id/" + levelId + "/activity?skip=" + skip + "&count=" + count);
    }

    public GetIpVerificationRequests(): Observable<ApiListResponse<IpVerificationRequest>> {
        return this.makeListRequest<IpVerificationRequest>("GET", "verificationRequests?skip=0&count=100");
    }

    public ApproveIpVerificationRequests(ipAddress: string): Observable<IpVerificationRequest> {
        return this.makeRequest<IpVerificationRequest>("PUT", "verificationRequests/approve", ipAddress);
    }

    public DenyIpVerificationRequests(ipAddress: string): Observable<IpVerificationRequest> {
        return this.makeRequest<IpVerificationRequest>("PUT", "verificationRequests/deny", ipAddress);
    }

    public EditLevel(level: LevelEditRequest, id: number, admin: boolean = false): void {
        let endpoint: string = "levels/id/" + id;
        if (admin) endpoint = "admin/" + endpoint;
        this.apiRequestCreator.makeRequest("PATCH", endpoint, level)
            .subscribe(_ => {
                this.bannerService.pushSuccess("Level Updated", `${level.title} was successfully updated.`);
            });
    }

    public UpdateLevelIcon(hash: string, id: number, admin: boolean = false): void {
        let endpoint: string = "levels/id/" + id;
        if (admin) endpoint = "admin/" + endpoint;
        this.apiRequestCreator.makeRequest("PATCH", endpoint, {iconHash: hash})
            .subscribe(_ => {
                this.bannerService.pushSuccess("Icon updated", "The level's icon was successfully updated.");
            });
    }

    public DeleteLevel(level: Level): void {
        this.apiRequestCreator.makeRequest("DELETE", "levels/id/" + level.levelId)
            .subscribe(_ => {
                this.bannerService.pushWarning("Level Deleted", `${level.title} was successfully removed.`);
            });
    }

    public SetLevelAsOverride(level: Level): void {
        this.apiRequestCreator.makeRequest("POST", `levels/id/${level.levelId}/setAsOverride`)
            .subscribe(_ => {
                this.bannerService.pushSuccess("Check your game!", `In LBP, head to 'Lucky Dip' (or any category) and '${level.title}' will show up!`);
            });
    }

    public SetLevelHashAsOverride(hash: string): void {
        this.apiRequestCreator.makeRequest("POST", `levels/hash/${hash}/setAsOverride`)
            .subscribe(_ => {
                this.bannerService.pushSuccess("Check your game!", `In LBP, head to 'Lucky Dip' (or any category) and that level will show up!`);
            });
    }

    public UploadImageAsset(hash: string, data: ArrayBuffer): Observable<Asset> {
        return this.apiRequestCreator.makeRequest("POST", `assets/${hash}`, data);
    }

    public GetContests(): Observable<Contest[]> {
        return this.makeRequest<Contest[]>("GET", "contests");
    }

    public GetContestById(contestId: string): Observable<Contest> {
        return this.makeRequest<Contest>("GET", "contests/" + contestId);
    }

    public CreateContest(contest: ContestEditRequest): Observable<Contest> {
        return this.makeRequest<Contest>("POST", "admin/contests/" + contest.contestId, contest);
    }

    public UpdateContest(contest: ContestEditRequest): Observable<Contest> {
        return this.makeRequest<Contest>("PATCH", "contests/" + contest.contestId, contest);
    }

    public DeleteContest(contest: Contest): void {
        this.apiRequestCreator.makeRequest("DELETE", "admin/contests/" + contest.contestId)
            .subscribe(_ => {
                this.bannerService.pushWarning("Contest Deleted", `${contest.contestTitle} was successfully removed.`);
            });
    }
}

export function GetPhotoLink(photo: Photo, large: boolean = true): string {
    return GetAssetImageLink(large ? photo.largeHash : photo.smallHash);
}

export function GetAssetImageLink(hash: string | undefined): string {
    if (hash === undefined || hash === null || hash === "0" || hash.startsWith('g')) return "";
    return environment.apiBaseUrl + "/assets/" + hash + "/image";
}
