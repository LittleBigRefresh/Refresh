const { createThemes } = require('tw-colors');

const defaultColors = {
  "background": "#0F0814",
  "header-background": "#261731",
  "form-background": "#2A1936",
  "backdrop": "#1B0F23",

  "divider": "#13071A",

  "header-foreground": "#F7F7F7",
  "foreground": "#F7F7F7",
  "gentle": "#C3C3C3",

  "primary": "#A13DE3",
  "secondary": "#87748A",
  "teritary": "#100915",

  "secondary-bright": "#E2CFE5",

  "success": "#52BC24",
  "dangerous": "#E52E2E",
  "warning": "#F2AA00",

  "rank-gold": "#FFD234",
  "rank-silver": "#F2F2F2",
  "rank-bronze": "#FF8845",
  "rank-other": "#ABABAB",

  "api-retrieve": "#52BC24",
  "api-remove": "#E52E2E",
  "api-push": "#2D43E5"
}

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {},
    fontFamily: {
      'display': ['Rubik'],
      'body': ['Rubik']
    },
    borderRadius: {
      'none': '0',
      DEFAULT: '11px',
      'md': '0.375rem',
      'lg': '0.5rem',
      'full': '9999px',
    },
    colors: defaultColors,
    backgroundImage: {
      "hero": "url('/assets/hero.svg')",
      "ss-hero": "url('/assets/ss-hero.png')",
      "logo": "url('/assets/logo.svg')",
    },
  },
  plugins: [
    createThemes({
      default: defaultColors,
      hack: {
        "background": "#010101",
        "header-background": "#010101",
        "form-background": "#001100",
        "backdrop": "#000000",

        "divider": "#00EE00",

        "header-foreground": "#00FF00",
        "foreground": "#00FF00",
        "gentle": "#00DD00",

        "primary": "#009900",
        "secondary": "#008800",
        "teritary": "#001100",

        "success": "#009900",
        "dangerous": "#005500",
        "warning": "#005500",

        "rank-gold": "#00FF00",
        "rank-silver": "#00AA00",
        "rank-bronze": "#009900",
        "rank-other": "#005500",
      },
      ultraDark: {
        "background": "#000000",
        "header-background": "#040404",
        "form-background": "#030303",
        "backdrop": "#000000",

        "divider": "#000000",

        "header-foreground": "#F7F7F7",
        "foreground": "#F7F7F7",
        "gentle": "#C3C3C3",

        "primary": "#723896",
        "secondary": "#373737",
        "teritary": "#0f0f0f",

        "secondary-bright": "#AAAAAA",

        "success": "#52BC24",
        "dangerous": "#E52E2E",
        "warning": "#d99800",

        "rank-gold": "#FFD234",
        "rank-silver": "#F2F2F2",
        "rank-bronze": "#FF8845",
        "rank-other": "#ABABAB",

        "api-retrieve": "#52BC24",
        "api-remove": "#E52E2E",
        "api-push": "#2D43E5"
      },
      soundShapes: {
        "background": "#2F2A2A",
        "header-background": "#181515",
        "form-background": "#EED4BF",
        "backdrop": "#FFF0E4",

        "divider": "#61493C",

        "header-foreground": "#EED4BF",
        "foreground": "#181515",
        "gentle": "#8A7261",

        "primary": "#F07167",
        "secondary": "#F4DECC",
        "teritary": "#e5d2c3",

        "secondary-bright": "#E2CFE5",

        "success": "#79c501",
        "dangerous": "#ea574d",
        "warning": "#eeb231",

        "rank-gold": "#eeb231",
        "rank-silver": "#FFF0E4",
        "rank-bronze": "#FF8845",
        "rank-other": "#8A7261",

        "api-retrieve": "#79c501",
        "api-remove": "#ea574d",
        "api-push": "#eeb231",
      },
      hotdogStand: {
        "background": "yellow",
        "header-background": "#c53c38",
        "form-background": "#c53c38",
        "backdrop": "red",

        "divider": "yellow",

        "header-foreground": "yellow",
        "foreground": "#f58c78",
        "gentle": "#9F8775",

        "primary": "#EE0000",
        "secondary": "yellow",
        "teritary": "#b53c18",

        "secondary-bright": "white",

        "success": "green",
        "dangerous": "#ea574d",
        "warning": "#eeb231",

        "rank-gold": "#eeb231",
        "rank-silver": "#FFF0E4",
        "rank-bronze": "#FF8845",
        "rank-other": "#8A7261",

        "api-retrieve": "#79c501",
        "api-remove": "#ea574d",
        "api-push": "#eeb231",
      },
      lighthouse: {
        "background": "#ededf0",
        "header-background": "#121212",
        "form-background": "#dddddd",
        "backdrop": "#FFFFFF",

        "divider": "#d4d4d5",

        "header-foreground": "#F7F7F7",
        "foreground": "#15141A",
        "gentle": "#5B5B66",

        "primary": "#0e91f5",
        "secondary": "#d4d4d5",
        "teritary": "#FFFFFF",

        "secondary-bright": "#FFFFFF",

        "success": "#23c841",
        "dangerous": "#f12c2c",
        "warning": "#fdc202",

        "rank-gold": "#fdc202",
        "rank-silver": "#F2F2F2",
        "rank-bronze": "#FF8845",
        "rank-other": "#d4d4d5",

        "api-retrieve": "#23c841",
        "api-remove": "#f12c2c",
        "api-push": "#0e91f5"
      },
      vgui: {
        "background": "#2d3328",
        "header-background": "#2d3328",
        "form-background": "#4d5845",
        "backdrop": "#3f4638",

        "divider": "#c3b350",

        "header-foreground": "#c3b350",
        "foreground": "#c3b350",
        "gentle": "#968a3f",

        "primary": "#637059",
        "secondary": "#3c4535",
        "teritary": "#23291f",

        "secondary-bright": "#637059",

        "success": "#318831",
        "dangerous": "#963f3f",
        "warning": "#7a7032",

        "rank-gold": "#c3b350",
        "rank-silver": "#c2c2c0",
        "rank-bronze": "#c38450",
        "rank-other": "#968a3f",

        "api-retrieve": "#318831",
        "api-remove": "#963f3f",
        "api-push": "#3da9b1",

        // Taken from VGUI colors on steam deck
        // "BackgroundAlternate": "#3f4638",
        // "BackgroundNormal": "#4d5845",
        // "DecorationFocus": "#318831",
        // "DecorationHover": "#318831",
        // "ForegroundActive": "#4cb07b",
        // "ForegroundInactive": "#3da9b1",
        // "ForegroundLink": "#4cb07b",
        // "ForegroundNegative": "#37ffff",
        // "ForegroundNeutral": "#37ffff",
        // "ForegroundNormal": "#34fcfc",
        // "ForegroundPositive": "#37ffff",
        // "ForegroundVisited": "#4cb07b",
      },
      chicago: {
        "background": "#008081",
        "header-background": "#000080",
        "form-background": "#FFFFFF",
        "backdrop": "#c0c0c0",

        "divider": "#c0c0c0",

        "header-foreground": "#FFFFFF",
        "foreground": "#000000",
        "gentle": "#777777",

        "primary": "#54fcfc",
        "secondary": "#808080",
        "teritary": "#808080",

        "secondary-bright": "#FFFFFF",

        "success": "#54fc54",
        "dangerous": "#fc5454",
        "warning": "#fcfc54",

        "rank-gold": "#fcfc54",
        "rank-silver": "#FFFFFF",
        "rank-bronze": "#a85400",
        "rank-other": "#ABABAB",

        "api-retrieve": "#54fc54",
        "api-remove": "#fc5454",
        "api-push": "#54fcfc"
      }
    })
  ],
  safelist: [ // force generation for notifications
    "bg-success",
    "bg-warning",
    "bg-dangerous"
  ]
}
