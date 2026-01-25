import {ApiList} from "./api-list";

export interface ApiListResponse<TData> {
    items: TData[];
    listInfo: ApiList;
}
