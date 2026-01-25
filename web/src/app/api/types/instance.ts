import {Announcement} from "./announcement";
import {ContactInfo} from "./contactInfo";
import {Contest} from "./contests/contest";

export interface Instance {
    instanceName: string;
    instanceDescription: string;

    softwareName: string;
    softwareVersion: string;
    softwareType: string;

    registrationEnabled: boolean;
    maximumAssetSafetyLevel: number;

    announcements: Announcement[];
    maintenanceModeEnabled: boolean;
    grafanaDashboardUrl: string | null;

    contactInfo: ContactInfo;
    activeContest: Contest | null;

    websiteLogoUrl: string | null;
}
