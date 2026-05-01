# syntax=docker/dockerfile:1
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG GOOGLE_WEB_CLIENT_ID=
RUN GOOGLE_WEB_CLIENT_ID="${GOOGLE_WEB_CLIENT_ID}" node scripts/write-docker-environment.mjs

RUN npm run build

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
EXPOSE 80
