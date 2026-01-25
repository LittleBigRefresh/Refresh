export {AppServerModule} from './app/app.server.module';

import {enableProdMode} from '@angular/core';
import * as dayjs from 'dayjs';
import * as relativeTime from 'dayjs/plugin/relativeTime'
import {environment} from 'src/environments/environment';

dayjs.extend(relativeTime)

if (environment.production) {
    enableProdMode();
}
