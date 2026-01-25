import {enableProdMode} from '@angular/core';
import {platformBrowserDynamic} from '@angular/platform-browser-dynamic';
import {environment} from 'src/environments/environment';

import {AppModule} from './app/app.module';

import * as dayjs from 'dayjs';
import * as relativeTime from 'dayjs/plugin/relativeTime'

dayjs.extend(relativeTime)

if (environment.production) {
    enableProdMode();
}

document.addEventListener("DOMContentLoaded", () => {
    platformBrowserDynamic()
        .bootstrapModule(AppModule)
        .catch(err => console.log(err));
});
