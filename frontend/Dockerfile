# syntax=docker/dockerfile:1

FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.30-alpine AS final

COPY --from=build /app/dist /usr/share/nginx/html

# nginx's own docker-entrypoint.d hook envsubst's *.template files into
# conf.d on startup, substituting BACKEND_BASE_URL from the container env
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

EXPOSE 80
