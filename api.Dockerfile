###########################
# Build vite front-end    #
###########################

# Create npm build container 
FROM node:23-slim AS vite-build


# Define vite build env variables as usable args
ARG VITE_API_BASE_URL
ARG VITE_ENTRA_CLIENT_ID
ARG VITE_ENTRA_TEDANT_ID
ARG VITE_ENTRA_SERVER
ARG VITE_ENTRA_SCOPE
ARG VITE_ENTRA_REDIRECT_URL

# Use the argument as an environment variable
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
ENV VITE_ENTRA_CLIENT_ID=${VITE_ENTRA_CLIENT_ID}
ENV VITE_ENTRA_TEDANT_ID=${VITE_ENTRA_TEDANT_ID}
ENV VITE_ENTRA_SERVER=${VITE_ENTRA_SERVER}
ENV VITE_ENTRA_SCOPE=${VITE_ENTRA_SCOPE}
ENV VITE_ENTRA_REDIRECT_URL=${VITE_ENTRA_REDIRECT_URL}

# Example usage in a command
RUN echo "The value of API_BASE_URL is ${VITE_API_BASE_URL}"
RUN echo "The value of VITE_ENTRA_CLIENT_ID is ${VITE_ENTRA_CLIENT_ID}"
RUN echo "The value of VITE_ENTRA_TEDANT_ID is ${VITE_ENTRA_TEDANT_ID}"
RUN echo "The value of VITE_ENTRA_SERVER is ${VITE_ENTRA_SERVER}"
RUN echo "The value of VITE_ENTRA_SCOPE is ${VITE_ENTRA_SCOPE}"
RUN echo "The value of VITE_ENTRA_REDIRECT_URL is ${VITE_ENTRA_REDIRECT_URL}"





WORKDIR /source

# Copy project files
COPY Frontend/. ./

# Install dependencies
RUN npm install

# Build vite 
RUN npm run build

# Vite container result file system 
#
# root/
# ├─ source/
# │  ├─ package.json
# ├─ LarpakeServer/
# │  ├─ wwwroot/
# │  │  ├─ public/
# │  │  ├─ src/
# │  │  ├─ styles/





##########################
# Build web API project  #
##########################

# Create 'build' image
FROM mcr.microsoft.com/dotnet/sdk:latest AS build

WORKDIR /source

# Copy source files 
COPY LarpakeServer/. ./

# Change workdir
RUN dotnet publish -c Release -o /app 

# Dotnet build container output
#
# root/
# ├─ source/
# │  ├─ LarpakeServer.csproj
# ├─ app/
# │  ├─ LarpakeServer.exe
# │  ├─ LarpakeServer.dll
# │  ├─ many_libraries.dll






########################
# Create runtime image #
########################

# Create final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:latest
WORKDIR /app

# Copy build image result to runtime image
COPY --from=build /app .

# Copy built frontend into wwwroot
COPY --from=vite-build /LarpakeServer/wwwroot/. ./wwwroot/.

# Run "dotnet LarpakeServer.dll" to run platform independent (before JIT) dll file 
ENTRYPOINT [ "dotnet", "LarpakeServer.dll" ]

# Final application container file structure
#
# root/
# ├─ app/
# │  ├─ LarpakeServer.dll
# │  ├─ LarpakeServer.exe
# │  ├─ wwwroot/
# │  │  ├─ src/
# │  │  ├─ node_modules/
# │  │  ├─ styles/
# │  │  ├─ public/

