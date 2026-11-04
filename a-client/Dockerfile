FROM node:20-alpine AS build
WORKDIR /app
COPY a-client/package*.json ./
RUN npm ci
COPY a-client/ ./
RUN npm run build

FROM node:20-alpine AS final
WORKDIR /app
COPY --from=build /app/dist/a-client ./dist/a-client
ENV PORT=4200
EXPOSE 4200
CMD ["node", "dist/a-client/server/server.mjs"]
