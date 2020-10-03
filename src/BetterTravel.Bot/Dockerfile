ARG DotnetVersion=3.1
FROM mcr.microsoft.com/dotnet/core/sdk:$DotnetVersion-alpine AS build
WORKDIR /app

# Copy csproj files
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do \
        echo $file \
        && mkdir -p src/${file%.*}/ \
        && mv $file src/${file%.*}/; \
    done

# Restore api csproj
RUN dotnet restore ./src/BetterTravel.Bot/BetterTravel.Bot.csproj

# Copy everything else and build
COPY . ./

# Publish to out folder
RUN dotnet publish ./src/BetterTravel.Bot/BetterTravel.Bot.csproj -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:$DotnetVersion-alpine AS base

WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80/tcp
ENTRYPOINT ["dotnet", "BetterTravel.Bot.dll"]