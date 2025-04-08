

# Create 'build' image
FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /source

# Copy source files 
COPY LarpakeServer/. ./

# build with dotnet
RUN dotnet publish -c Release -o /app 

# Create new image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:latest
WORKDIR /app

# Copy build image result to runtime image
COPY --from=build /app .

# Run "dotnet LarpakeServer.dll" to run platform independent (before JIT) dll file 
ENTRYPOINT [ "dotnet", "LarpakeServer.dll" ]
