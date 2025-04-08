# Build vite app

FROM node:latest as vite-build

WORKDIR /vite

# Copy project dependency files

COPY Frontend/package.json Frontend/package-lock.json ./

# Install dependencies
RUN npm install

# Build vite 

COPY Frontend ./

RUN npm run build