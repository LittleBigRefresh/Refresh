// weird name since JS has a type named Notification
export interface RefreshNotification {
    notificationId: string
    title: string
    text: string

    createdAt: Date

    fontAwesomeIcon: string
}
