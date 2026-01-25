export interface LevelEditRequest {
    title: string | undefined;
    description: string | undefined;
    iconHash: string | undefined;
    gameVersion: string | undefined;
    originalPublisher: string | undefined;
    isReUpload: boolean | undefined;
}
