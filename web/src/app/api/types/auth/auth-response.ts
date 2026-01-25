export interface ApiAuthenticationResponse {
    tokenData: string;
    refreshTokenData: string;
    userId: string;
    expiresAt: Date;

    resetToken: string | undefined;
}
