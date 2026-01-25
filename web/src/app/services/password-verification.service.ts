import {Injectable} from "@angular/core";
import {BannerService} from "../banners/banner.service";
import {Banner} from "../banners/banner";

@Injectable({providedIn: 'root'})
export class PasswordVerificationService {
    constructor(private bannerService: BannerService) {
    }

    public verifyPassword(email: string | undefined, password: string, username: string | undefined = undefined, confirmPassword: string | undefined = undefined): boolean {
        const error: Banner = {
            Color: 'dangerous',
            Icon: 'exclamation-circle',
            Title: "Skill Issue",
            Text: "",
        }

        if (email && email.length <= 0) {
            error.Text = "Не введена электронная почта."
            this.bannerService.push(error)
            return false;
        }

        if (password.length <= 0) {
            error.Text = "А пароль ты не забыл?"
            this.bannerService.push(error)
            return false;
        }

        if (confirmPassword != undefined) {
            if (password != confirmPassword) {
                error.Text = "Пароли разные."
                this.bannerService.push(error)
                return false;
            }

            if (password.length < 8) {
                error.Text = "Пароль должен быть минимум 8 букв или цифр или да мне пофиг."
                this.bannerService.push(error)
                return false;
            }
        }

        if (username !== undefined && username.length <= 0) {
            error.Text = "Сегодня без имени."
            this.bannerService.push(error)
            return false;
        }

        return true;
    }
}
