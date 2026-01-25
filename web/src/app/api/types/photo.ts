import {User} from "./user";
import {PhotoSubject} from "./photo-subject";
import {Level} from "./level";

export interface Photo {
    photoId: number;

    takenAt: Date;
    publishedAt: Date;

    publisher: User;

    smallHash: string;
    largeHash: string;

    level: Level | undefined;
    levelName: string;
    levelType: string;
    levelId: number;

    subjects: PhotoSubject[];
}
