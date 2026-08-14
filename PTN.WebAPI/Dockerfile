FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PTN.WebAPI.csproj", "./"]
RUN dotnet restore "PTN.WebAPI.csproj"
COPY . .
RUN dotnet publish "PTN.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5160
ENV ASPNETCORE_URLS=http://+:5160
ENTRYPOINT ["dotnet", "PTN.WebAPI.dll"]