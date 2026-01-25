FROM node:22-alpine3.20 AS build

RUN mkdir /refresh-web
WORKDIR /refresh-web

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build:ssr

FROM node:22-alpine3.20 AS run
EXPOSE 4000/tcp

RUN mkdir -p /dist
COPY --from=build /refresh-web/dist /dist

ENTRYPOINT [ "node", "/dist/refresh-web/server/main.js" ]
