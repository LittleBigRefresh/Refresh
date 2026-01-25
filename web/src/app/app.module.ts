import {NgModule} from '@angular/core';
import {BrowserModule,} from '@angular/platform-browser';
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {TransferHttpCacheModule} from '@nguniversal/common';

import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {LevelCategoriesComponent} from './pages/level-categories/level-categories.component';
import {LevelListingComponent} from './pages/level-listing/level-listing.component';
import {LevelComponent} from './pages/level/level.component';
import {NotFoundComponent} from './pages/not-found/not-found.component';
import {MainComponent} from './pages/main/main.component';
import {FaIconLibrary, FontAwesomeModule} from '@fortawesome/angular-fontawesome';
import {SpanGentleComponent} from './components/span-gentle/span-gentle.component';
import {ParagraphGentleComponent} from './components/p-gentle/p-gentle.component';
import {LoginComponent} from './pages/login/login.component';
import {DividerComponent} from './components/divider/divider.component';
import {FormInputComponent} from './components/form-input/form-input.component';
import {PrimaryButtonComponent} from './components/primary-button/primary-button.component';
import {SecondaryButtonComponent} from './components/secondary-button/secondary-button.component';
import {BannerComponent} from './components/notification/banner.component';
import {ResetPasswordComponent} from './pages/reset-password/reset-password.component';
import {ApiTokenInterceptor} from './api/api-token.interceptor';
import {UserComponent} from './pages/user/user.component';
import {LogoutComponent} from './pages/logout/logout.component';
import {DangerousButtonComponent} from './components/dangerous-button/dangerous-button.component';
import {UiDebugComponent} from './pages/ui-debug/ui-debug.component';
import {FormDropdownComponent} from './components/form-dropdown/form-dropdown.component';
import {FormHolderComponent} from './components/form-holder/form-holder.component';
import {PageHeaderComponent} from './components/page-header/page-header.component';
import {LinkComponent} from './components/link/link.component';
import {PageHeaderBlockComponent} from './components/page-header-block/page-header-block.component';
import {SettingsComponent} from './pages/settings/settings.component';
import {PhotoListingComponent} from './pages/photo-listing/photo-listing.component';
import {PhotoPageComponent} from './pages/photo-page/photo-page.component';
import {UserLinkComponent} from './components/links/user-link/user-link.component';
import {LevelLinkComponent} from './components/links/level-link/level-link.component';
import {PhotoComponent} from './components/photo/photo.component';
import {NotificationListingComponent} from './pages/notification-listing/notification-listing.component';
import {RefreshNotificationComponent} from './components/refresh-notification/refresh-notification.component';
import {DocumentationComponent} from './pages/documentation/documentation.component';
import {LevelPreviewComponent} from './components/level-preview/level-preview.component';
import {NgxMasonryModule} from "ngx-masonry";
import {UserAvatarComponent} from './components/user-avatar/user-avatar.component';
import {LevelAvatarComponent} from './components/level-avatar/level-avatar.component';
import {NgOptimizedImage} from "@angular/common";
import {IntersectionObserverDirective} from './directives/intersection-observer.directive';
import {ActivityComponent} from './pages/activity/activity.component';
import {ActivityEventComponent} from './components/activity-event/activity-event.component';
import {AuthenticationComponent} from './pages/authentication/authentication.component';
import {FormCheckboxComponent} from './components/form-checkbox/form-checkbox.component';
import {RegisterComponent} from './pages/register/register.component';
import {AdminPanelComponent} from './pages/admin-panel/admin-panel.component';
import {AnnouncementComponent} from './components/announcement/announcement.component';
import {AdminUserComponent} from './pages/admin-user/admin-user.component';
import {VerifyComponent} from './pages/verify/verify.component';
import {DeleteAccountComponent} from './pages/delete-account/delete-account.component';
import {AdminUsersComponent} from './pages/admin-users/admin-users.component';
import {AdminRegistrationsComponent} from './pages/admin-registrations/admin-registrations.component';
import {TooltipComponent} from "./components/tooltip/tooltip.component";
import {CategoryPreviewComponent} from './components/category-preview/category-preview.component';
import {CarouselComponent} from './components/carousel/carousel.component';
import {CarouselCycleButtonComponent} from './components/carousel-cycle-button/carousel-cycle-button.component';
import {LevelStatisticsComponent} from './components/level-statistics/level-statistics.component';
import {MenuComponent} from './components/menu/menu.component';
import {MenuLinkComponent} from './components/menu-link/menu-link.component';
import {AdminLinkButtonComponent} from './components/admin-link-button/admin-link-button.component';
import {EditLevelComponent} from './pages/edit-level/edit-level.component';
import {FormsModule} from "@angular/forms";
import {ForgotPasswordComponent} from './pages/forgot-password/forgot-password.component';
import {DateComponent} from './components/date/date.component';
import {StatisticsComponent} from './pages/statistics/statistics.component';
import {ContainerComponent} from './components/container/container.component';
import {ContestsComponent} from './pages/contests/contests.component';
import {ManageContestComponent} from './pages/manage-contest/manage-contest.component';
import {ContestComponent} from './pages/contest/contest.component';
import {ContestBannerComponent} from './components/contest-banner/contest-banner.component';
import {MarkdownModule, MarkedOptions} from "ngx-markdown";
import {markedOptionsFactory} from "./markdown.config";
import {ContestPreviewComponent} from "./components/contest-preview/contest-preview.component";
import {ContestLabelComponent} from "./components/contest-label/contest-label.component";
import { PlayHashComponent } from './pages/play-hash/play-hash.component';
import {fas} from "@fortawesome/free-solid-svg-icons";

@NgModule({
    declarations: [
        AppComponent,
        LevelComponent,
        LevelCategoriesComponent,
        LevelListingComponent,
        NotFoundComponent,
        MainComponent,
        SpanGentleComponent,
        ParagraphGentleComponent,
        LoginComponent,
        DividerComponent,
        FormInputComponent,
        PrimaryButtonComponent,
        SecondaryButtonComponent,
        BannerComponent,
        ResetPasswordComponent,
        UserComponent,
        LogoutComponent,
        DangerousButtonComponent,
        UiDebugComponent,
        FormDropdownComponent,
        FormHolderComponent,
        PageHeaderComponent,
        LinkComponent,
        PageHeaderBlockComponent,
        SettingsComponent,
        PhotoListingComponent,
        PhotoPageComponent,
        UserLinkComponent,
        LevelLinkComponent,
        PhotoComponent,
        NotificationListingComponent,
        RefreshNotificationComponent,
        DocumentationComponent,
        LevelPreviewComponent,
        UserAvatarComponent,
        LevelAvatarComponent,
        IntersectionObserverDirective,
        ActivityComponent,
        ActivityEventComponent,
        AuthenticationComponent,
        FormCheckboxComponent,
        RegisterComponent,
        AdminPanelComponent,
        AnnouncementComponent,
        AdminUserComponent,
        VerifyComponent,
        DeleteAccountComponent,
        AdminUsersComponent,
        AdminRegistrationsComponent,
        TooltipComponent,
        CategoryPreviewComponent,
        CarouselComponent,
        CarouselCycleButtonComponent,
        LevelStatisticsComponent,
        MenuComponent,
        MenuLinkComponent,
        AdminLinkButtonComponent,
        EditLevelComponent,
        ForgotPasswordComponent,
        DateComponent,
        StatisticsComponent,
        ContainerComponent,
        ContestsComponent,
        ManageContestComponent,
        ContestComponent,
        ContestBannerComponent,
        ContestPreviewComponent,
        ContestLabelComponent,
        PlayHashComponent,
    ],
    imports: [
        BrowserModule,
        AppRoutingModule,
        HttpClientModule,
        FontAwesomeModule,
        BrowserAnimationsModule,
        TransferHttpCacheModule,
        NgxMasonryModule,
        NgOptimizedImage,
        FormsModule,
        MarkdownModule.forRoot({
            markedOptions: {
                provide: MarkedOptions,
                useFactory: markedOptionsFactory,
            },
        })
    ],
    providers: [
        {provide: HTTP_INTERCEPTORS, useClass: ApiTokenInterceptor, multi: true},
    ],
    bootstrap: [AppComponent]
})
export class AppModule {
    constructor(library: FaIconLibrary) {
        library.addIconPacks(fas)
    }
}
