import {User} from "../user";
import {GameVersion} from "../game-version";
import {Level} from "../level";

export interface Contest {
    contestId: string;
    organizer: User;

    creationDate: Date;
    startDate: Date;
    endDate: Date;

    contestTag: string;
    bannerUrl: string;

    contestTitle: string;
    contestSummary: string;
    contestDetails: string;
    contestTheme: string;

    templateLevel: Level
    allowedGames: GameVersion[];
}
