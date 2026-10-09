# ---- Build ----
FROM node:22-alpine AS build
WORKDIR /app

# Configuration Angular à utiliser (docker, preprod, production).
ARG BUILD_CONFIGURATION=docker

# Installation à part pour profiter du cache Docker ; npm ci installe exactement les versions de package-lock.json.
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
RUN npx ng build --configuration ${BUILD_CONFIGURATION}

# ---- Runtime ----
FROM nginxinc/nginx-unprivileged:alpine AS runtime

# URL du backend vers laquelle nginx redirige /api (substituée au démarrage du conteneur).
ENV API_UPSTREAM=http://api:8080

COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist/fizzbuzz-frontend/browser /usr/share/nginx/html

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/ || exit 1
