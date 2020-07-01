FROM node:14.4.0-buster-slim AS base
WORKDIR /app
EXPOSE 443

FROM node:14.4.0-buster AS build
WORKDIR /build
COPY *.json ./
COPY *.js ./
COPY src ./src/
RUN npm install
COPY public ./public/
RUN npm run build # see https://create-react-app.dev/docs/production-build/

FROM base AS final
WORKDIR /app
COPY --from=build /src/build .
ENTRYPOINT ["npm", "start"]