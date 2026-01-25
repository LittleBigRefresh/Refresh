import {User} from "./user";

export interface ExtendedUser extends User {
    allowIpAuthentication: boolean;
    role: number;
    banReason: string | null;
    banExpiryDate: Date | null;

    rpcnAuthenticationAllowed: boolean;
    psnAuthenticationAllowed: boolean;

    emailAddress: string | undefined;
    emailAddressVerified: boolean;
}
