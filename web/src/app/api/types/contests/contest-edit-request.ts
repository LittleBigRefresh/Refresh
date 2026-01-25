import {GameVersion} from "../game-version";

export interface ContestEditRequest {
    contestId: string | undefined;
    organizerId: string | undefined;

    startDate: Date | undefined;
    endDate: Date | undefined;

    contestTag: string | undefined;
    bannerUrl: string | undefined;

    contestTitle: string | undefined;
    contestSummary: string | undefined;
    contestDetails: string | undefined;
    contestTheme: string | undefined;

    templateLevelId: number | undefined;

    allowedGames: GameVersion[] | undefined;
}
