########################
# Create build image   #
########################

# Create 'build' image
FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /source

# Copy source files 
COPY MigrationsService/. ./

# Change workdir
RUN dotnet publish -c Release -o /app 

########################
# Create runtime image #
########################

# Create final runtime image
FROM mcr.microsoft.com/dotnet/runtime:latest
WORKDIR /app

# Copy build image result to runtime image
COPY --from=build /app .

# Run "dotnet LarpakeServer.dll" to run platform independent (before JIT) dll file 
ENTRYPOINT [ "dotnet", "MigrationsService.dll" ]
