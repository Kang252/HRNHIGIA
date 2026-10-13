# Primary Render image: serve the NHIGIA React client and Nest API from one
# same-origin service. This replaces the previous ASP.NET runtime.
FROM node:22-alpine AS api-dependencies
WORKDIR /src/NHIGIA.Api
COPY NHIGIA.Api/package*.json ./
RUN npm ci

FROM node:22-alpine AS client-build
WORKDIR /src/NHIGIA.Modern/ClientApp
COPY NHIGIA.Modern/ClientApp/package*.json ./
RUN npm ci
COPY NHIGIA.Modern/ClientApp/ ./
ENV VITE_APP_BASE_PATH=/modern-ui/
ENV VITE_API_BASE_PATH=/api/v1
RUN npm run build

FROM node:22-alpine AS api-build
WORKDIR /src/NHIGIA.Api
COPY --from=api-dependencies /src/NHIGIA.Api/node_modules ./node_modules
COPY NHIGIA.Api/ ./
RUN npm run build && npm prune --omit=dev

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production \
    PORT=10000 \
    HOST=0.0.0.0 \
    CLIENT_APP_DIR=/app/client-app
COPY --from=api-build --chown=node:node /src/NHIGIA.Api/node_modules ./node_modules
COPY --from=api-build --chown=node:node /src/NHIGIA.Api/dist ./dist
COPY --from=client-build --chown=node:node /src/NHIGIA.Modern/ClientApp/dist ./client-app
USER node
EXPOSE 10000
CMD ["node", "dist/main.js"]
