FROM node:24-alpine AS build
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
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html

# Configuração simples do nginx para Single Page Applications (SPA)
RUN echo 'server { \
    listen 80; \
    server_name _; \
    location / { \
        root /usr/share/nginx/html; \
        index index.html index.htm; \
        try_files $uri $uri/ /index.html; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
