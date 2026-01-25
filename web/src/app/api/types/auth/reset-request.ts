export interface ApiPasswordResetRequest {
    passwordSha512: string
    resetToken: string | undefined
}
