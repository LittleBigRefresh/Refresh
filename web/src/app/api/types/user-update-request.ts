export interface UserUpdateRequest {
    description: string | undefined;
    allowIpAuthentication: boolean | undefined;

    psnAuthenticationAllowed: boolean | undefined;
    rpcnAuthenticationAllowed: boolean | undefined;

    emailAddress: string | undefined;
}
