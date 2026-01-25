import {Injectable} from "@angular/core";
import {ExtendedUser} from "./types/extended-user";

@Injectable({
    providedIn: 'root'
})
export class TokenStorageService {
    private readonly GameTokenKey: string = "game_token";
    private readonly RefreshTokenKey: string = "refresh_token";
    private readonly UserKey: string = "cached_user_data";

    public GetStoredGameToken(): string | null {
        return localStorage.getItem(this.GameTokenKey);
    }

    public SetStoredGameToken(token: string): void {
        localStorage.setItem(this.GameTokenKey, token);
    }

    public ClearStoredGameToken(): void {
        localStorage.removeItem(this.GameTokenKey);
    }

    public GetStoredRefreshToken(): string | null {
        return localStorage.getItem(this.RefreshTokenKey);
    }

    public SetStoredRefreshToken(refreshToken: string): void {
        localStorage.setItem(this.RefreshTokenKey, refreshToken);
    }

    public ClearStoredRefreshToken(): void {
        localStorage.removeItem(this.RefreshTokenKey);
    }

    public GetStoredUser(): ExtendedUser | null {
        const str = localStorage.getItem(this.UserKey);
        if (!str) return null;

        try {
            return JSON.parse(str);
        } catch (e) {
            console.error("Stored user is null");
            return null;
        }
    }

    public SetStoredUser(user: ExtendedUser): void {
        localStorage.setItem(this.UserKey, JSON.stringify(user));
    }

    public ClearStoredUser(): void {
        localStorage.removeItem(this.UserKey);
    }
}
