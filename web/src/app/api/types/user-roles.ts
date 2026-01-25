export class UserRoles {
    public static Admin: number = 127;
    public static Curator: number = 64;
    public static Trusted: number = 1;
    public static User: number = 0;
    public static Restricted: number = -126;
    public static Banned: number = -127;

    public static getRoleName(roleNumber: number): string {
        for (const roleName in UserRoles) {
            // @ts-ignore
            if (UserRoles[roleName] === roleNumber) {
                return roleName;
            }
        }
        throw new Error(`Role name not found for role number ${roleNumber}`);
    }
}
