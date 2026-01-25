import {User} from "./user"

export interface Level {
    levelId: number
    title: string
    description: string
    iconHash: string
    publishDate: Date
    updateDate: Date
    booRatings: number
    yayRatings: number
    hearts: number
    totalPlays: number
    uniquePlays: number
    publisher: User | undefined
    originalPublisher: string | undefined
    isReUpload: boolean
    teamPicked: boolean
    gameVersion: number
    score: number
}
