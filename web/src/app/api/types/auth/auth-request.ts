export interface ApiAuthenticationRequest {
    username: string | undefined
    emailAddress: string
    passwordSha512: string
}
