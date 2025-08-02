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
   cp IzinTakipPaneli/appsettings.example.json IzinTakipPaneli/appsettings.json
   ```
   Edit `appsettings.json` and adjust the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=LeaveManagementDB;Trusted_Connection=True;"
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
   
7. **(Optional)Populate with example data:**
Run the following script in SQL Server (e.g., via SSMS) to insert sample departments, users, settings, and leave types:
  ```
   -- 1. Add department
   INSERT INTO Departments (DepartmentName, IsActive, CreatedAt)
   VALUES 
   ('Software', 1, GETDATE()),
   ('Human Resources', 1, GETDATE()),
   ('Accounting', 1, GETDATE());
   
   -- 2. Add employee
   INSERT INTO Employees (FullName, DepartmentID, UserRole, IsActive, Username, Password, CreatedAt)
   VALUES ('admin', 1, 'admin', 1, 'admin', '1', GETDATE());
   
   INSERT INTO Employees (FullName, DepartmentID, UserRole, IsActive, Username, Password, CreatedAt)
   VALUES ('user', 1, 'user', 1, 'user', '1', GETDATE());
   
   INSERT INTO Employees (FullName, DepartmentID, UserRole, IsActive, Username, Password, CreatedAt)
   VALUES ('Merve Arslan', 1, 'user', 1, 'merve.arslan', '1', GETDATE());
   
   -- 3. Add annual leave count to the settings table
   INSERT INTO Settings (MaxAnnualLeaveDays, IsActive, CreatedAt)
   VALUES (20, 1, GETDATE());
   
   -- 4. Add leave types
   INSERT INTO LeaveTypes (LeaveTypeName, IsActive, CreatedAt)
   VALUES 
   ('Anual', 1, GETDATE()),
   ('Unpaid', 1, GETDATE()),
   ('Illness', 1, GETDATE());
  ```

### Test Backup (.bak)

#### Restore Instructions
Using SQL Server Management Studio or another SQL client:
```sql
RESTORE DATABASE LeaveManagementDB_Test
FROM DISK = 'C:\path\to\LeaveManagementDB.bak'
WITH MOVE 'LeaveManagementDB_Data' TO 'C:\SQLData\LeaveManagementDB_Test.mdf',
     MOVE 'LeaveManagementDB_Log' TO 'C:\SQLData\LeaveManagementDB_Test_log.ldf',
     REPLACE;
```
Then update the connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=LeaveManagementDB_Test;Trusted_Connection=True;"
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
   cp IzinTakipPaneli/appsettings.example.json IzinTakipPaneli/appsettings.json
   ```
   `appsettings.json` dosyasını düzenleyin:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=LeaveManagementDB;Trusted_Connection=True;"
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
   
7. **(İsteğe bağlı)Örnek verilerle doldurma:**
Aşağıdaki script’i SQL Server’da çalıştırarak temel departmanları, kullanıcıları, ayarları ve izin türlerini ekleyebilirsiniz:
```
   -- 1. Departman ekle
   INSERT INTO Departments (DepartmentName, IsActive, CreatedAt)
   VALUES 
   ('Yazılım', 1, GETDATE()),
   ('İnsan Kaynakları', 1, GETDATE()),
   ('Muhasebe', 1, GETDATE());
   
   -- 2. Kullanıcı ekle
   INSERT INTO Employees (FullName, DepartmentID, UserRole, IsActive, Username, Password, CreatedAt)
   VALUES ('admin', 1, 'admin', 1, 'admin', '1', GETDATE());
   
   INSERT INTO Employees (FullName, DepartmentID, UserRole, IsActive, Username, Password, CreatedAt)
   VALUES ('user', 1, 'user', 1, 'user', '1', GETDATE());
   
   INSERT INTO Employees (FullName, DepartmentID, UserRole, IsActive, Username, Password, CreatedAt)
   VALUES ('Merve Arslan', 1, 'user', 1, 'merve.arslan', '1', GETDATE());
   
   -- 3. Ayarlar tablosuna yıllık izin sayısını ekle
   INSERT INTO Settings (MaxAnnualLeaveDays, IsActive, CreatedAt)
   VALUES (20, 1, GETDATE());
   
   -- 4. İzin türlerini ekle
   INSERT INTO LeaveTypes (LeaveTypeName, IsActive, CreatedAt)
   VALUES 
   ('Yıllık', 1, GETDATE()),
   ('Ücretsiz', 1, GETDATE()),
   ('Hastalık', 1, GETDATE());
```

### Test Yedeği (.bak)

#### Geri Yükleme
```sql
RESTORE DATABASE LeaveManagementDB_Test
FROM DISK = 'C:\path\to\LeaveManagementDB.bak'
WITH MOVE 'LeaveManagementDB_Data' TO 'C:\SQLData\LeaveManagementDB_Test.mdf',
     MOVE 'LeaveManagementDB_Log' TO 'C:\SQLData\LeaveManagementDB_Test_log.ldf',
     REPLACE;
```
Ardından bağlantı dizesini güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=LeaveManagementDB_Test;Trusted_Connection=True;"
  }
}
```

