## Nội dung chính
- Thư mục chính của dự án: `CoTuongLAN/` (chứa `Client/`, `Server/` và các file solution/project)

## Yêu cầu
- Windows 10/11
- Visual Studio 2017/2019/2022 hoặc MSBuild
- .NET Framework tương ứng (mở `.csproj` để biết phiên bản chính xác)

## Cách build & chạy (Visual Studio)
1. Mở `CoTuongLAN\CoTuongLAN.sln` bằng Visual Studio.
2. Chọn cấu hình `Debug` hoặc `Release`, rồi Build Solution.
3. Chạy `Server` trước, sau đó chạy `Client`.

## Cách build & chạy (PowerShell / dòng lệnh)
- Dùng MSBuild nếu đã cài:

```powershell
cd 'C:\Users\User\OneDrive\Máy tính\da-NT106\Nhom7-NT106.Q12\CoTuongLAN'
msbuild .\CoTuongLAN.sln /p:Configuration=Debug
```

- Hoặc chạy trực tiếp exe sau khi build:

```powershell
Start-Process -FilePath ".\CoTuongLAN\Server\bin\Debug\Server.exe"
Start-Process -FilePath ".\CoTuongLAN\Client\bin\Debug\Client.exe"
```

## Cấu hình mạng
- Kiểm tra `App.config` trong `Client/` và `Server/` để biết IP/port mặc định.
- Trên mạng LAN, đảm bảo mở port tương ứng và cho phép firewall nếu cần.
