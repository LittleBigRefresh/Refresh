import {User} from "./user";

export interface PhotoSubject {
    user: User | null;
    displayName: string;
    bounds: number[];
}
