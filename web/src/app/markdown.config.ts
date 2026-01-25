import {MarkedOptions, MarkedRenderer} from "ngx-markdown";

export function markedOptionsFactory(): MarkedOptions {
    const renderer = new MarkedRenderer();

    renderer.heading = (text, level) => {
        let tailwindFontSize: string;

        switch (level) {
            case 1:
                tailwindFontSize = "3xl"
                break;
            case 2:
                tailwindFontSize = "2xl"
                break;
            case 3:
                tailwindFontSize = "xl"
                break;
            default:
                tailwindFontSize = "1xl";
                break;
        }

        return `<h${level} class="font-bold text-${tailwindFontSize} mt-2.5">${text}</h${level}>`;
    }

    renderer.link = (href, title, text) => {
        return `<a href="${href}" class="text-secondary-bright hover:underline">${text}</a>`;
    }

    renderer.list = (body, ordered) => {
        if(ordered)
            return `<ol class="list-decimal list-inside">${body}</ol>`;
        else
            return `<ul class="list-disc list-inside">${body}</ul>`;
    }

    return {
        renderer: renderer,
        gfm: true,
        breaks: false,
        pedantic: false,
    };
}
