import {IEnvironment} from "./environment-interface";

let baseUrl: string;

if (typeof window !== 'undefined') {
    baseUrl = window.location.protocol + '//' + window.location.host + "/api/v3";
} else {
    // @ts-ignore
    baseUrl = "https://" + process.env.HOST + "/api/v3";
}

export const environment: IEnvironment = {
    production: true,
    apiBaseUrl: baseUrl,
};
