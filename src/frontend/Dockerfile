FROM nginx:1.19.0 AS base
WORKDIR /app
EXPOSE 3000
COPY nginx.conf /etc/nginx/nginx.conf

FROM node:14.4.0-buster AS build
WORKDIR /tls
COPY tls-cert ./boardz.pfx
RUN openssl pkcs12 -in ./boardz.pfx -clcerts -nokeys -out boardz-out.crt -passin 'pass:'
RUN openssl pkcs12 -in ./boardz.pfx -nocerts -nodes -out boardz-out.rsa -passin 'pass:'
WORKDIR /build
COPY *.json ./
COPY *.js ./
COPY src ./src/
RUN npm install
COPY public ./public/
RUN npm run build # see https://create-react-app.dev/docs/production-build/

FROM base AS final
WORKDIR /app
COPY --from=build /build/build /app/
COPY --from=build /tls/boardz-out.* /etc/nginx/ssl/
ENTRYPOINT ["nginx", "-g", "daemon off;"]