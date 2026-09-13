FROM node:24-alpine AS build
WORKDIR /app

# Copiar package.json e package-lock.json
COPY package*.json ./
RUN npm ci

# Copiar o resto dos arquivos
COPY . .

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
