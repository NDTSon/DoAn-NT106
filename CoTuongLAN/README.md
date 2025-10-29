# CoTuongLAN

Một dự án C# Windows Forms gồm 2 phần: Server và Client cho trò chơi Cờ Tướng chạy trên mạng LAN.

## Tổng quan
- Thư mục chính: `CoTuongLAN/`
- Chứa 2 project chính: `Server/` và `Client/` cùng file solution `CoTuongLAN.sln`.
- Dự án được phát triển cho Windows (Visual Studio). Các file `.sln` và `.csproj` có trong repo.

## Yêu cầu
- Windows 10/11
- Visual Studio 2017/2019/2022 hoặc MSBuild
- .NET Framework (phiên bản tương thích với project; mở `.csproj` để kiểm tra chính xác)

## Cấu trúc chính
- `CoTuongLAN.sln` — solution chứa các project
- `Server/` — source cho Server
- `Client/` — source cho Client

## Hướng dẫn build & chạy (Visual Studio)
1. Mở `CoTuongLAN.sln` trong Visual Studio.
2. Chọn cấu hình `Debug` hoặc `Release` và build solution (Build -> Build Solution).
3. Chạy trước `Server` (F5 hoặc Start without Debugging) — sau đó chạy `Client`.

## Hướng dẫn build & chạy (PowerShell / dòng lệnh)
- Build bằng MSBuild (nếu có):

```powershell
cd 'C:\Users\User\OneDrive\Máy tính\da-NT106\Nhom7-NT106.Q12\CoTuongLAN'
msbuild .\CoTuongLAN.sln /p:Configuration=Debug
```

- Hoặc mở trực tiếp file exe (nếu đã build):

```powershell
Start-Process -FilePath ".\Server\bin\Debug\Server.exe"
Start-Process -FilePath ".\Client\bin\Debug\Client.exe"
```

Lưu ý: đường dẫn exe có thể khác nếu bạn build ở cấu hình `Release` hoặc thay đổi output path.

## Cấu hình mạng / App.config
- Nếu `Client` cần kết nối đến IP/port của `Server`, kiểm tra file `App.config` trong `Client/` và `Server/` để cấu hình.
- Trên môi trường LAN, đảm bảo firewall cho phép ứng dụng (hoặc mở port tương ứng).

## Ghi chú về Git
- Hiện repo có chứa một số file build/output (ví dụ `bin/`, `obj/`, `.exe`) — để giữ repo sạch, nên thêm `.gitignore` (loại trừ `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, v.v.).
- Nếu bạn muốn, mình có thể: tạo `.gitignore`, loại bỏ file nhị phân khỏi lịch sử (bằng cách chỉnh lịch sử và force-push). Thao tác này có thể ảnh hưởng đến lịch sử chung — chỉ làm khi bạn đồng ý.

## Góp ý & đóng góp
- Nếu muốn mình chỉnh README này (thêm chi tiết về cổng, cách cấu hình, hình ảnh, hoặc license), cho mình biết những nội dung cụ thể.

## License
- Chưa có file LICENSE trong repo. Nếu bạn muốn, mình có thể thêm `LICENSE` (ví dụ MIT) — xác nhận nếu muốn.

---
Cần mình thêm chi tiết kỹ thuật (cổng mặc định, format message, config) không? Nếu có, mình sẽ đọc `Client/Service.cs` và `Server/Service.cs` để ghi vào README chi tiết hơn.