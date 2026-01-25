import {RoomPlayer} from "./roomplayer"

export interface Room {
    RoomId: string
    PlayerIds: RoomPlayer[]
    State: string
    LevelType: string
    LevelId: number
}
