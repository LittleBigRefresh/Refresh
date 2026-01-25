import {User} from "./user";

export interface Asset {
    assetHash: string;
    originalUploader: User;
    uploadDate: Date;
    assetType: number;
}
