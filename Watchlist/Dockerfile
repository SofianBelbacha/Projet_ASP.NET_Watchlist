# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copier et restaurer les projets
COPY *.csproj ./
RUN dotnet restore

# Copier le reste du projet et publier
COPY . ./
RUN dotnet publish -c Release -o /out

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime
WORKDIR /app

# Installer locales et ICU pour supporter la culture en-US
RUN apt-get update && apt-get install -y locales && rm -rf /var/lib/apt/lists/*
RUN locale-gen en_US.UTF-8

# Copier les fichiers publiés depuis l'étape build
COPY --from=build /out ./

# Variables pour globalization
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LANG=en_US.UTF-8
ENV LC_ALL=en_US.UTF-8

# Port à écouter
ENV PORT=5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "Watchlist.dll"]
