import {IconDefinition} from "@fortawesome/fontawesome-svg-core"

export class HeaderLink {
    title: string
    link: string
    icon: IconDefinition
    raw: boolean

    constructor(title: string, link: string, icon: IconDefinition, raw: boolean = false) {
        this.title = title;
        this.link = link;
        this.icon = icon;
        this.raw = raw;
    }
}
