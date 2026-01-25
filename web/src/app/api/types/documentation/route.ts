import {Parameter} from "./parameter";
import {PotentialError} from "./error";

export interface Route {
    method: string;
    routeUri: string;
    summary: string;
    authenticationRequired: boolean;
    parameters: Parameter[];
    potentialErrors: PotentialError[];
}
