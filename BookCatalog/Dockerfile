FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Server/BookCatalog.Server.csproj", "Server/"]
COPY ["Client/BookCatalog.Client.csproj", "Client/"]
COPY ["Shared/BookCatalog.Shared.csproj", "Shared/"]
RUN dotnet restore "Server/BookCatalog.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "BookCatalog.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookCatalog.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookCatalog.Server.dll"] 