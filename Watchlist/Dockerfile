# Étape 1  build
FROM mcr.microsoft.comdotnetsdk8.0 AS build
WORKDIR app

COPY .csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o out

# Étape 2  runtime
FROM mcr.microsoft.comdotnetaspnet8.0 AS runtime
WORKDIR app

COPY --from=build out .

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

ENTRYPOINT [dotnet, Watchlist.dll]
