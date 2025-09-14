# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# فقط csproj ها برای کش
COPY ["src/Presentation/Presentation.csproj", "src/Presentation/"]
COPY ["src/Application/Application/Application.csproj", "src/Application/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/Presentation/Presentation.csproj"

# بقیه سورس + wwwroot
COPY . .

RUN dotnet publish "src/Presentation/Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# چک: wwwroot باید وجود داشته باشد
RUN test -d /app/publish/wwwroot && echo "wwwroot OK" && find /app/publish/wwwroot -maxdepth 2 -type f | head -n 30

# final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Presentation.dll"]
