# === Step 1: Build ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy hanya file proyek dulu untuk caching restore
COPY *.csproj ./
RUN dotnet restore

# Copy seluruh kode
COPY . ./
RUN dotnet publish -c Release -o /app

# === Step 2: Runtime ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app ./
EXPOSE 80

ENTRYPOINT ["dotnet", "mitraacd.dll"]
