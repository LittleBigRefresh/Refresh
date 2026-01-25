import {Component, OnInit} from '@angular/core';
import {IpVerificationRequest} from "../../api/types/auth/ip-verification-request";
import {ApiClient} from "../../api/api-client.service";
import {faCheck, faXmark} from "@fortawesome/free-solid-svg-icons";

@Component({
    selector: 'app-authentication',
    templateUrl: './authentication.component.html'
})
export class AuthenticationComponent implements OnInit {
    requests: IpVerificationRequest[] | undefined = undefined;

    constructor(private apiClient: ApiClient) {
    }

    ngOnInit(): void {
        this.refresh();
    }

    refresh() {
        this.apiClient.GetIpVerificationRequests()
            .subscribe(data => {
                if (data === undefined) return;
                this.requests = data.items;
            })
    }

    approve(ipAddress: string) {
        this.apiClient.ApproveIpVerificationRequests(ipAddress).subscribe(() => {
            window.location.reload(); // TODO: reload the requests dynamically instead of ...this.
        })
    }

    deny(ipAddress: string) {
        this.apiClient.DenyIpVerificationRequests(ipAddress).subscribe(() => {
            window.location.reload(); // TODO: reload the requests dynamically instead of ...this.
        })
    }

    protected readonly faCheck = faCheck;
    protected readonly faXmark = faXmark;
}
