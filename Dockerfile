# Sử dụng SDK 8.0 để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copy file project và restore thư viện
COPY ["TranCongDuc_21231110517/TranCongDuc_21231110517.csproj", "TranCongDuc_21231110517/"]
RUN dotnet restore "TranCongDuc_21231110517/TranCongDuc_21231110517.csproj"

# 2. Copy toàn bộ code và build Release (PHẢI NẰM TRÊN 1 DÒNG)
COPY . .
WORKDIR "/src/TranCongDuc_21231110517"
RUN dotnet publish "TranCongDuc_21231110517.csproj" -c Release -o /app/publish

# 3. Stage chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# Lệnh khởi chạy file dll của bạn
ENTRYPOINT ["dotnet", "TranCongDuc_21231110517.dll"]