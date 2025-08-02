# Leave Tracking Mini Panel / İzin Takip Mini Paneli

## 🇺🇸 English

### Project Overview
A lightweight panel for employees to request leaves, monitor annual leave balances, and filter records by department. Administrators can manage departments, leave types, and annual leave policies. Built with Entity Framework Core using safe migrations. An optional test database backup (.bak) is provided for evaluation or development.

### Features
- **Leave Request:** Select employee, department, leave type; specify start/end dates; automatic working-day computation; enforcement of annual leave limits.  
- **Listing:** View leave history with department and date filters; alerts for overuse.  
- **Admin:** Configure annual leave limits, add departments, define leave types.  
- **Configurable:** Editable parameters such as limits, leave types, and departments.  
- **Validation & Robustness:** Handles invalid dates, limit violations, and input errors gracefully.  
- **Clean Architecture:** EF Core, separated ViewModels, and migration-safe schema evolution.

### Requirements
- .NET 8+ SDK  
- SQL Server (LocalDB / SQLExpress / full)  
- Git  

### Quick Start

1. **Clone the repository:**
   ```bash
   git clone https://github.com/zekiyeoztrrk/leave-tracking-mini-panel.git
   cd leave-tracking-mini-panel/IzinTakipPaneli
   ```

2. **Prepare configuration:**  
   ```powershell
   copy-item appsettings.example.json appsettings.json
   ```
   Edit `appsettings.json` and adjust the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\SQLEXPRESS;Database=LeaveManagementDB;Trusted_Connection=True;"
     }
   }
   ```

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

4. **Apply migrations / create or update database:**
   ```bash
   dotnet ef database update
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. **(Optional) Use the provided test backup:**  
   Restore the `.bak` file and update the connection string to point to the test database.

### Test Backup (.bak)

#### Restore Instructions
Using SQL Server Management Studio or another SQL client:
```sql
RESTORE DATABASE LeaveManagementDB_Test
FROM DISK = 'C:\path\to\IzinTakipTest.bak'
WITH MOVE 'LeaveManagementDB_Data' TO 'C:\SQLData\LeaveManagementDB_Test.mdf',
     MOVE 'LeaveManagementDB_Log' TO 'C:\SQLData\LeaveManagementDB_Test_log.ldf',
     REPLACE;
```
Then update the connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\SQLEXPRESS;Database=LeaveManagementDB_Test;Trusted_Connection=True;"
  }
}
```
---

## 🇹🇷 Türkçe

### Proje Özeti
Çalışanların izin taleplerini girebildiği, yıllık izin bakiyelerini izleyebildiği ve departman bazında filtreleme yapabildiği hafif bir panel. Yöneticiler departmanları, izin türlerini ve yıllık izin politikalarını yönetebilir. Entity Framework Core ile güvenli migration’lar kullanılarak inşa edilmiştir. Değerlendirme veya geliştirme için isteğe bağlı bir test veritabanı yedeği (.bak) sağlanır.

### Özellikler
- **İzin Girişi:** Çalışan, departman, izin türü seçimi; başlangıç/bitiş tarihleri; otomatik iş günü hesaplaması; yıllık izin limitlerinin uygulanması.  
- **Listeleme:** Departman ve tarih filtreli izin geçmişi görüntüleme; aşırı kullanım uyarıları.  
- **Admin:** Yıllık izin limitlerini yapılandırma, departman ekleme, izin türleri tanımlama.  
- **Parametrik:** Limitler, izin türleri ve departmanlar düzenlenebilir.  
- **Doğrulama & Dayanıklılık:** Geçersiz tarihler, limit ihlalleri ve giriş hataları düzgün şekilde ele alınır.  
- **Temiz Mimari:** EF Core, ayrılmış ViewModel’ler ve migration’larla güvenli şema evrimi.

### Gereksinimler
- .NET 8+ SDK  
- SQL Server (LocalDB / SQLExpress / tam sürüm)  
- Git  

### Hızlı Başlangıç

1. **Repoyu klonlayın:**
   ```bash
   git clone https://github.com/zekiyeoztrrk/leave-tracking-mini-panel.git
   cd leave-tracking-mini-panel/IzinTakipPaneli
   ```

2. **Konfigürasyonu hazırla:**  
   ```powershell
   copy-item appsettings.example.json appsettings.json
   ```
   `appsettings.json` dosyasını düzenleyin:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\SQLEXPRESS;Database=LeaveManagementDB;Trusted_Connection=True;"
     }
   }
   ```

3. **Bağımlılıkları yükleyin:**
   ```bash
   dotnet restore
   ```

4. **Migration’ları uygula / veritabanını oluştur veya güncelle:**
   ```bash
   dotnet ef database update
   ```

5. **Uygulamayı çalıştır:**
   ```bash
   dotnet run
   ```

6. **(İsteğe bağlı) Sağlanan test yedeğini kullan:**  
   `.bak` dosyasını geri yükleyin ve bağlantı dizesini test veritabanına göre güncelleyin.

### Test Yedeği (.bak)

#### Geri Yükleme
```sql
RESTORE DATABASE LeaveManagementDB_Test
FROM DISK = 'C:\path\to\IzinTakipTest.bak'
WITH MOVE 'LeaveManagementDB_Data' TO 'C:\SQLData\LeaveManagementDB_Test.mdf',
     MOVE 'LeaveManagementDB_Log' TO 'C:\SQLData\LeaveManagementDB_Test_log.ldf',
     REPLACE;
```
Ardından bağlantı dizesini güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\SQLEXPRESS;Database=LeaveManagementDB_Test;Trusted_Connection=True;"
  }
}
```

