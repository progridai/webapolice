FROM node:24.18.0-alpine AS build
WORKDIR /app

# Copiar package.json e package-lock.json
COPY package*.json ./
RUN npm ci

# Copiar o resto dos arquivos
COPY . .

# Variáveis de ambiente para o build do Vite
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

ARG VITE_KEYCLOAK_URL
ENV VITE_KEYCLOAK_URL=$VITE_KEYCLOAK_URL

ARG VITE_KEYCLOAK_REALM
ENV VITE_KEYCLOAK_REALM=$VITE_KEYCLOAK_REALM

ARG VITE_KEYCLOAK_CLIENT_ID
ENV VITE_KEYCLOAK_CLIENT_ID=$VITE_KEYCLOAK_CLIENT_ID

ARG VITE_ENABLE_DESIGN_SYSTEM
ENV VITE_ENABLE_DESIGN_SYSTEM=$VITE_ENABLE_DESIGN_SYSTEM

ARG VITE_APP_VERSION
ENV VITE_APP_VERSION=$VITE_APP_VERSION

# Build da aplicação
RUN npm run build

# Imagem final de produção
FROM nginx:1.30.6-alpine3.24
COPY --from=build /app/dist /usr/share/nginx/html

COPY nginx.conf /etc/nginx/nginx.conf

USER nginx
EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]
