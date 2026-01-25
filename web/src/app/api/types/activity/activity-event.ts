export interface ActivityEvent {
    eventId: string;
    eventType: number;
    userId: string;
    occurredAt: Date;

    storedDataType: number;
    storedSequentialId: number | undefined;
    storedObjectId: string | undefined;
}
