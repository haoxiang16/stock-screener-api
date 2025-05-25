# 基礎執行階段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# 建置階段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 複製 csproj 並還原 NuGet 套件
COPY ["StockAPI.csproj", "."]
RUN dotnet restore "StockAPI.csproj"

# 複製所有原始碼
COPY . .

# 建置應用
RUN dotnet build "StockAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 發佈階段
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StockAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 最終執行階段
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockAPI.dll"]
