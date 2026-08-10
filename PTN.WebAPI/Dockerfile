# 1. Aşama: C# SDK ile Derleme (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PTN.WebAPI.csproj", "./"]
RUN dotnet restore "PTN.WebAPI.csproj"
COPY . .
RUN dotnet build "PTN.WebAPI.csproj" -c Release -o /app/build

# 2. Aşama: Yayınlama (Publish Stage)
FROM build AS publish
RUN dotnet publish "PTN.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Aşama: Canlı Çalıştırma (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5160
ENV ASPNETCORE_URLS=http://+:5160
ENTRYPOINT ["dotnet", "PTN.WebAPI.dll"]