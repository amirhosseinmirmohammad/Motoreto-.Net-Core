# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Shop.Presentation/Shop.Presentation.csproj", "Shop.Presentation/"]
COPY ["src/Shop.Infrastructure/Shop.Infrastructure.csproj", "Shop.Infrastructure/"]
COPY ["src/Shop.Application/Shop.Application.csproj", "Shop.Application/"]
COPY ["src/Shop.Domain/Shop.Domain.csproj", "Shop.Domain/"]
COPY ["src/Shop.Shared/Shop.Shared.csproj", "Shop.Shared/"]
RUN dotnet restore "Shop.Presentation/Shop.Presentation.csproj"
COPY src/ .
WORKDIR /src/Shop.Presentation
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- final ----------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Shop.Presentation.dll"]
