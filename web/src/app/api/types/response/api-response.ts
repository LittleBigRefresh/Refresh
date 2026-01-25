import {ApiError} from "./api-error";
import {ApiList} from "./api-list";

export interface ApiResponse<TData> {
    data: TData | undefined;
    listInfo: ApiList | undefined;

    success: boolean;
    error: ApiError | undefined;
}
