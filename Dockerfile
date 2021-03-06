FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["pdfcut.csproj", "./"]
RUN dotnet restore "pdfcut.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "pdfcut.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pdfcut.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pdfcut.dll"]
# Use the following instead for Heroku
#CMD ASPNETCORE_URLS=http://*:$PORT dotnet pdfcut.dll

