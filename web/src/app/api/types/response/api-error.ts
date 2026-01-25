export interface ApiError {
    name: string;
    message: string;
    statusCode: number;
    warning: boolean | undefined;
}
