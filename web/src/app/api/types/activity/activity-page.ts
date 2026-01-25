import {User} from "../user";
import {Level} from "../level";
import {ActivityEvent} from "./activity-event";
import {Score} from "../score";
import {Photo} from "../photo";

export interface ActivityPage {
    events: ActivityEvent[];
    users: User[];
    levels: Level[];
    scores: Score[];
    photos: Photo[];
}
