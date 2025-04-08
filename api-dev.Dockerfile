########################
# Create build image   #
########################

# Create 'build' image
FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /source

# Copy source files 
COPY LarpakeServer/. ./

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

COPY --from=vite-build /LarpakeServer/wwwroot ./wwwroot

# Run "dotnet LarpakeServer.dll" to run platform independent (before JIT) dll file 
ENTRYPOINT [ "dotnet", "LarpakeServer.dll" ]
