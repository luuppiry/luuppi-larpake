###########################
# Create vite build image #
###########################

# Build vite app

FROM node:latest AS vite-build

WORKDIR /source

# Copy project dependency files

COPY Frontend/package.json Frontend/package-lock.json /source/

# Install dependencies
RUN npm install

# Build vite 

COPY Frontend/. /source/

RUN npm run build

##########################
# Create API build image #
##########################

# Create 'build' image
FROM mcr.microsoft.com/dotnet/sdk:latest AS build

WORKDIR /source

# Copy source files 
COPY LarpakeServer/. ./

# Copy built frontend into wwwroot
COPY --from=vite-build /LarpakeServer/wwwroot ./wwwroot/

# Change workdir
RUN dotnet publish -c Release -o /app 

########################
# Create runtime image #
########################

# Create final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:latest
WORKDIR /app

# Copy build image result to runtime image
COPY --from=build /app .

# Run "dotnet LarpakeServer.dll" to run platform independent (before JIT) dll file 
ENTRYPOINT [ "dotnet", "LarpakeServer.dll" ]
