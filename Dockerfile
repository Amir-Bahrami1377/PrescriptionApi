FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props Prescription.slnx ./
COPY src/Api/Prescription.Api.csproj src/Api/
COPY src/SharedKernel/Prescription.SharedKernel.csproj src/SharedKernel/
COPY src/Modules/Identity/Prescription.Modules.Identity.csproj src/Modules/Identity/
COPY src/Modules/Catalog/Prescription.Modules.Catalog.csproj src/Modules/Catalog/
COPY src/Modules/Orders/Prescription.Modules.Orders.csproj src/Modules/Orders/
COPY src/Modules/Ticketing/Prescription.Modules.Ticketing.csproj src/Modules/Ticketing/
COPY src/Modules/Notifications/Prescription.Modules.Notifications.csproj src/Modules/Notifications/
COPY src/Modules/Payments/Prescription.Modules.Payments.csproj src/Modules/Payments/
COPY src/Modules/FileStorage/Prescription.Modules.FileStorage.csproj src/Modules/FileStorage/
RUN dotnet restore src/Api/Prescription.Api.csproj

COPY src/ src/
RUN dotnet publish src/Api/Prescription.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Prescription.Api.dll"]
