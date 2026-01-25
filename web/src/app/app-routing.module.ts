import {isDevMode, NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {ResetPasswordComponent} from './pages/reset-password/reset-password.component';
import {LevelCategoriesComponent} from './pages/level-categories/level-categories.component';
import {LevelListingComponent} from './pages/level-listing/level-listing.component';
import {LevelComponent} from './pages/level/level.component';
import {LoginComponent} from './pages/login/login.component';
import {LogoutComponent} from './pages/logout/logout.component';
import {MainComponent} from './pages/main/main.component';
import {NotFoundComponent} from './pages/not-found/not-found.component';
import {UiDebugComponent} from './pages/ui-debug/ui-debug.component';
import {UserComponent} from './pages/user/user.component';
import {SettingsComponent} from './pages/settings/settings.component';
import {PhotoPageComponent} from './pages/photo-page/photo-page.component';
import {PhotoListingComponent} from "./pages/photo-listing/photo-listing.component";
import {NotificationListingComponent} from "./pages/notification-listing/notification-listing.component";
import {DocumentationComponent} from "./pages/documentation/documentation.component";
import {ActivityComponent} from "./pages/activity/activity.component";
import {AuthenticationComponent} from "./pages/authentication/authentication.component";
import {RegisterComponent} from "./pages/register/register.component";
import {AdminPanelComponent} from "./pages/admin-panel/admin-panel.component";
import {AdminUserComponent} from "./pages/admin-user/admin-user.component";
import {VerifyComponent} from "./pages/verify/verify.component";
import {DeleteAccountComponent} from "./pages/delete-account/delete-account.component";
import {AdminRegistrationsComponent} from "./pages/admin-registrations/admin-registrations.component";
import {AdminUsersComponent} from "./pages/admin-users/admin-users.component";
import {EditLevelComponent} from "./pages/edit-level/edit-level.component";
import {authenticationGuard} from "./api/guards/authentication.guard";
import {adminAuthenticationGuard} from "./api/guards/admin-authentication.guard";
import {noAuthenticationGuard} from "./api/guards/no-authentication.guard";
import {ForgotPasswordComponent} from "./pages/forgot-password/forgot-password.component";
import {StatisticsComponent} from "./pages/statistics/statistics.component";
import {ContestsComponent} from "./pages/contests/contests.component";
import {ManageContestComponent} from "./pages/manage-contest/manage-contest.component";
import {ContestComponent} from "./pages/contest/contest.component";
import {PlayHashComponent} from "./pages/play-hash/play-hash.component";

const routes: Routes = [
    {path: "", component: MainComponent},

    {path: "levels", component: LevelCategoriesComponent},
    {path: "slots", redirectTo: "levels"},

    {path: "levels/:route", component: LevelListingComponent},
    {path: "level/:id", component: LevelComponent},
    {path: "level/:id/edit", component: EditLevelComponent, canActivate: [authenticationGuard]},
    {path: "slot/:id", redirectTo: "level/:id"},
    {path: "slot/:id/edit", redirectTo: "level/:id/edit"},

    {path: "user/:username", component: UserComponent},
    {path: "u/:uuid", component: UserComponent},

    {path: "login", component: LoginComponent, canActivate: [noAuthenticationGuard]},
    {path: "logout", component: LogoutComponent, canActivate: [authenticationGuard]},
    {path: "resetPassword", component: ResetPasswordComponent},
    {path: "forgorPassword", redirectTo: "forgotPassword"}, // I'm sorry but I have to
    {path: "forgotPassword", component: ForgotPasswordComponent},
    {path: "register", component: RegisterComponent, canActivate: [noAuthenticationGuard]},

    {path: "settings", component: SettingsComponent, canActivate: [authenticationGuard]},
    {path: "settings/delete", component: DeleteAccountComponent, canActivate: [authenticationGuard]},
    {path: "verify", redirectTo: "settings/verifyEmail"},
    {path: "settings/verifyEmail", component: VerifyComponent, canActivate: [authenticationGuard]},
    {path: "auth", redirectTo: "settings/authentication"},
    {path: "authentication", redirectTo: "settings/authentication"},
    {path: "settings/authentication", component: AuthenticationComponent, canActivate: [authenticationGuard]},

    {path: "photos", component: PhotoListingComponent},
    {path: "photo/:id", component: PhotoPageComponent},
    {path: "notifications", component: NotificationListingComponent, canActivate: [authenticationGuard]},
    {path: "activity", component: ActivityComponent},

    {path: "docs", redirectTo: "documentation"},
    {path: "documentation", component: DocumentationComponent},
    {path: "statistics", component: StatisticsComponent},

    {path: "admin", component: AdminPanelComponent, canActivate: [adminAuthenticationGuard]},
    {path: "admin/level/:id", redirectTo: "level/:id/edit"},
    {path: "admin/user/:uuid", component: AdminUserComponent, canActivate: [adminAuthenticationGuard]},
    {path: "admin/users", component: AdminUsersComponent, canActivate: [adminAuthenticationGuard]},
    {path: "admin/registrations", redirectTo: "admin/queuedRegistrations"},
    {path: "admin/queuedRegistrations", component: AdminRegistrationsComponent, canActivate: [adminAuthenticationGuard]},
    {path: "admin/newContest", component: ManageContestComponent, canActivate: [adminAuthenticationGuard]},

    {path: "contests", component: ContestsComponent},
    {path: "contests/:id", component: ContestComponent},
    {path: "contests/:id/manage", component: ManageContestComponent},

    {path: "playhash", component: PlayHashComponent, canActivate: [authenticationGuard]},
];

if (isDevMode()) {
    routes.push({path: "debug/ui", component: UiDebugComponent});
}

// 404 handling
routes.push({path: "**", component: NotFoundComponent});

@NgModule({
    imports: [RouterModule.forRoot(routes, {
        initialNavigation: 'enabledBlocking'
    })],
    exports: [RouterModule]
})
export class AppRoutingModule {
}
