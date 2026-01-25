import {EventEmitter, Injectable} from "@angular/core";
import {ApiRequestCreator} from "./api-request.creator";
import {ExtendedUser} from "./types/extended-user";
import {BannerService} from "../banners/banner.service";
import {Router} from "@angular/router";
import {ApiAuthenticationRequest} from "./types/auth/auth-request";
import {ApiError} from "./types/response/api-error";
import {ApiAuthenticationResponse} from "./types/auth/auth-response";
import {catchError, Observable, of, tap} from "rxjs";
import {ApiPasswordResetRequest} from "./types/auth/reset-request";
import {UserUpdateRequest} from "./types/user-update-request";
import {TokenStorageService} from "./token-storage.service";
import {ApiAuthenticationRefreshRequest} from "./types/auth/auth-refresh-request";
import {ApiResetPasswordRequest} from "./types/auth/send-reset-request";
import {Room} from "./types/rooms/room";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private _userId: string | undefined = undefined;
    private _loggedIn = false;

    user: ExtendedUser | undefined = undefined;
    resetToken: string | undefined = undefined;

    userWatcher: EventEmitter<ExtendedUser | undefined>

    private lastRoomUpdate: number = Date.now();
    private currentRoom: Room | undefined;

    constructor(private apiRequestCreator: ApiRequestCreator, private tokenStorage: TokenStorageService, private bannerService: BannerService, private router: Router) {
        this.userWatcher = new EventEmitter<ExtendedUser | undefined>();

        const storedToken: string | null = this.tokenStorage.GetStoredGameToken();
        const storedUser: ExtendedUser | null = this.tokenStorage.GetStoredUser();

        this._loggedIn = storedToken !== null;
        if (storedToken) {
            if (storedUser) {
                this._userId = storedUser.userId;
                this.user = storedUser;
                this.userWatcher.emit(this.user);
            }

            this.GetMyUser(() => {
                // only subscribe after getting user
                this.userWatcher.subscribe((user) => this.onUserUpdate(user));
            });
        } else {
            this.userWatcher.emit(undefined);
            this.userWatcher.subscribe((user) => this.onUserUpdate(user));
        }
    }

    public RefreshGameToken(callback: Function, refreshToken: string): string | null {
        const payload: ApiAuthenticationRefreshRequest = {
            tokenData: refreshToken
        };

        this.apiRequestCreator.makeRequest<ApiAuthenticationResponse>("POST", "refreshToken", payload, error => {
            if (error.statusCode !== 403) return;
            this.tokenStorage.ClearStoredRefreshToken();

            this.bannerService.pushWarning("Session Expired", "Your session has expired, please sign in again.")
            this.router.navigate(['/login']);
        }).subscribe((authResponse) => {
            this._userId = authResponse.userId;
            this.tokenStorage.SetStoredGameToken(authResponse.tokenData);

            callback();
        });
        return null;
    }

    onUserUpdate(user: ExtendedUser | undefined): void {
        console.log("Handling user change: ", user)
        if (user !== undefined) {
            if (!this._loggedIn) {
                this._loggedIn = true;
                this.bannerService.pushSuccess(`Hi, ${user.username}!`, 'You have been successfully signed in.')
                this.router.navigate(['/'])
            }
        } else {
            this._loggedIn = false;
            this.bannerService.push({
                Title: 'Signed out',
                Icon: 'right-from-bracket',
                Color: 'warning',
                Text: 'You have been logged out.'
            })

            this.router.navigate(['/login'])
        }
    }

    public LogIn(emailAddress: string, passwordSha512: string): boolean {
        if (this._userId !== undefined) throw Error("Cannot sign in when already signed in as someone."); // should never happen hopefully

        const body: ApiAuthenticationRequest = {
            username: undefined,
            emailAddress,
            passwordSha512: passwordSha512,
        }

        const errorHandler = (err: ApiError) => {
            if (err.warning) {
                this.bannerService.pushWarning('Warning', err.message);
                console.warn(err);
                return;
            }

            this.bannerService.pushError('Failed to sign in', err.message ?? "No error was provided by the server. Check the console for more details.")
            console.error(err);
        }

        this.apiRequestCreator.makeRequest<ApiAuthenticationResponse>("POST", "login", body, errorHandler)
            .pipe(catchError(() => {
                return of(undefined);
            }))
            .subscribe((authResponse) => {
                if (authResponse === undefined) return;

                if (authResponse.resetToken !== undefined) {
                    this.resetToken = authResponse.resetToken;
                    this.router.navigateByUrl("/resetPassword");
                    this.bannerService.pushWarning("Create a password", "The account you are trying to sign into is a legacy account. Please set a password.");
                    return;
                }

                this._userId = authResponse.userId;
                this.tokenStorage.SetStoredGameToken(authResponse.tokenData);
                this.tokenStorage.SetStoredRefreshToken(authResponse.refreshTokenData)
                this.GetMyUser();
            });

        return true;
    }

    public Register(username: string, emailAddress: string, passwordSha512: string): boolean {
        if (this._userId !== undefined) throw Error("Cannot register when already signed in as someone."); // should never happen hopefully

        const body: ApiAuthenticationRequest = {
            username,
            emailAddress,
            passwordSha512,
        }
        const errorHandler = (err: ApiError) => {
            if (err.warning) {
                this.bannerService.pushWarning('Warning', err.message);
                console.warn(err);
                return;
            }

            this.bannerService.pushError('Failed to register', err.message ?? "No error was provided by the server. Check the console for more details.")
            console.error(err);
        }

        this.apiRequestCreator.makeRequest<ApiAuthenticationResponse>("POST", "register", body, errorHandler)
            .pipe(catchError(() => {
                return of(undefined);
            }))
            .subscribe((authResponse) => {
                if (authResponse === undefined) return;

                this._userId = authResponse.userId;
                this.tokenStorage.SetStoredGameToken(authResponse.tokenData);
                this.tokenStorage.SetStoredRefreshToken(authResponse.refreshTokenData)
                this.GetMyUser();
            });

        return true;
    }

    private GetMyUser(callback: Function | null = null, tryingToRefresh: boolean = false) {
        this.apiRequestCreator.makeRequest<ExtendedUser>("GET", "users/me", undefined, (err) => {
            if (err.statusCode) {
                this.tokenStorage.ClearStoredUser();
                const refreshToken: string | null = this.tokenStorage.GetStoredRefreshToken();

                if (!tryingToRefresh && refreshToken !== null) {
                    this.RefreshGameToken(() => {
                        this.GetMyUser(callback, tryingToRefresh = true);
                    }, refreshToken);
                }

                this.tokenStorage.ClearStoredGameToken();
            }
            return of(undefined);
        })
            .subscribe((data) => {
                this.user = data;
                this.userWatcher.emit(this.user);
                this.tokenStorage.SetStoredUser(this.user);
                if (callback) callback();
            });
    }

    public GetMyRoom(): Observable<Room | undefined> {
        if (!this.user) return of(undefined);

        // If 5 minutes have passed, and we have a room cached
        if ((this.lastRoomUpdate > (Date.now() - 300 * 1000)) && this.currentRoom) {
            return of(this.currentRoom);
        }

        return this.apiRequestCreator.makeRequest<Room>("GET", `rooms/uuid/${this.user.userId}`, null, (_: ApiError) => {
        })
            .pipe(tap(data => {
                this.lastRoomUpdate = Date.now();
                this.currentRoom = data;
            }),);
    }

    public LogOut() {
        this._userId = undefined;
        this.user = undefined;

        this.userWatcher.emit(undefined);
        this.apiRequestCreator.makeRequest("PUT", "logout", {}).subscribe();

        this.tokenStorage.ClearStoredGameToken();
        this.tokenStorage.ClearStoredUser();
    }

    public ResetPassword(passwordSha512: string): void {
        if (this.user == undefined && this.resetToken == undefined) {
            this.bannerService.pushError('Could not reset password', 'There was no token to authorize this action.')
            return;
        }

        const body: ApiPasswordResetRequest = {
            passwordSha512: passwordSha512,
            resetToken: this.resetToken,
        }

        this.apiRequestCreator.makeRequest("PUT", "resetPassword", body)
            .subscribe(() => {
                if (!this.user) this.router.navigateByUrl('/login');
                else this.router.navigateByUrl('/');

                this.bannerService.push({
                    Color: 'success',
                    Icon: 'key',
                    Title: "Password reset successful",
                    Text: "Your account's password has been reset.",
                })
            });
    }

    public SendPasswordResetRequest(email: string): void {
        const body: ApiResetPasswordRequest = {
            emailAddress: email
        }

        this.apiRequestCreator.makeRequest("PUT", "sendPasswordResetEmail", body)
            .subscribe(() => {
                this.router.navigateByUrl('/login');

                this.bannerService.push({
                    Color: 'success',
                    Icon: 'reply',
                    Title: "Sent Reset Code",
                    Text: "If the email you entered match an account, we will send a password reset request to that inbox.",
                })
            });
    }

    public VerifyEmail(code: string): void {
        this.apiRequestCreator.makeRequest("POST", "verify?code=" + code)
            .subscribe(() => {
                if (this.user !== undefined) {
                    this.user.emailAddressVerified = true;
                }

                this.bannerService.push({
                    Color: 'success',
                    Icon: 'key',
                    Title: "Email verification successful",
                    Text: "Your account's email has been verified.",
                })
            });
    }

    public ResendVerificationCode(): void {
        this.apiRequestCreator.makeRequest("POST", "verify/resend")
            .subscribe(() => {
                this.bannerService.push({
                    Color: 'success',
                    Icon: 'key',
                    Title: "Resent verification code",
                    Text: "The verification email has been sent to your email address.",
                })
            });
    }

    public DeleteAccount(): void {
        this.apiRequestCreator.makeRequest("DELETE", "users/me")
            .subscribe(() => {
                this.bannerService.push({
                    Color: 'dangerous',
                    Icon: 'trash',
                    Title: "Account Deleted.",
                    Text: "Your account has been successfully deleted. Goodbye.",
                });

                this._userId = undefined;
                this.user = undefined;

                this.userWatcher.emit(undefined);
                this.tokenStorage.ClearStoredGameToken();
                this.tokenStorage.ClearStoredUser();
            });
    }

    public UpdateUser(data: UserUpdateRequest): void {
        this.apiRequestCreator.makeRequest<ExtendedUser>("PATCH", "users/me", data)
            .subscribe(data => {
                this.bannerService.pushSuccess("User updated", "Your profile was successfully updated.");

                this.user = data;
                this.userWatcher.emit(data);
                this.tokenStorage.SetStoredUser(data);
            });
    }

    public UpdateUserAvatar(hash: string): void {
        this.apiRequestCreator.makeRequest<ExtendedUser>("PATCH", "users/me", {iconHash: hash})
            .subscribe(data => {
                this.bannerService.pushSuccess("Avatar updated", "Your avatar was successfully updated.");
                this.user!.iconHash = data.iconHash;
                this.userWatcher.emit(data);
                this.tokenStorage.SetStoredUser(data);
            });
    }
}
