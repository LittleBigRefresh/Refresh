import {Level} from "./level"
import {User} from "./user"

export interface Score {
    scoreId: string
    score: number
    scoreType: number
    scoreSubmitted: Date

    level: Level
    players: User[]

    rank: number | undefined
}
