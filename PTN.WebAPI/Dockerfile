FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY ./publish .
EXPOSE 5160
ENV ASPNETCORE_URLS=http://+:5160
ENTRYPOINT ["dotnet", "PTN.WebAPI.dll"]