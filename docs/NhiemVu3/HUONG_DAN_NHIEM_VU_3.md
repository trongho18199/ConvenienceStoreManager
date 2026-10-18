# Hướng dẫn chạy và ghép phần nhiệm vụ 3

Đây là ghi chú kỹ thuật để học, chạy thử và bàn giao code. Nội dung không phải báo cáo học thuật nộp cuối kỳ.

## 1. Bản này dựa trên đâu?

- Repository: https://github.com/trongho18199/ConvenienceStoreManager
- Bản gốc: commit `b5046bd1fd4c330dd8319c942d25477d5bad707f`, trùng ZIP nhóm trưởng đã gửi.
- Giữ nguyên ba project UI, Business, Data; .NET Framework 4.8; Entity Framework 6.5.2; namespace và GUID project gốc.
- Không đổi Program.cs, Form1, module đăng nhập, PasswordHasher, migration Init và seed tài khoản gốc.
- Thêm solution `.sln` để mở thuận tiện trong Visual Studio 2022. File `.slnx` gốc vẫn giữ nguyên; khi chạy phần này hãy mở `.sln`.

## 2. Bốn Form đã làm

| Form trong ConvenienceStore.UI/Forms | Chức năng |
|---|---|
| FrmPurchaseReceipt | Chọn nhà cung cấp, thêm/gộp/xóa dòng hàng, nhập đơn giá, ghi chú; lưu phiếu và tăng tồn kho. |
| FrmSales | Giỏ hàng, khách lẻ/thành viên, VIP giảm 10%, tiền khách đưa và tiền thừa, kiểm tra tồn, thanh toán. |
| FrmStockMovement | Lọc lịch sử theo ngày, sản phẩm, nhập/bán; xem tồn đầu, nhập, bán, tồn cuối kỳ và tồn hiện tại. |
| FrmRevenueReport | Lọc doanh thu theo khoảng ngày; xem tổng hợp theo ngày và danh sách hóa đơn; xuất CSV UTF-8. |

Mỗi Form có đủ `.cs`, `.Designer.cs`, `.resx`, đã được đăng ký trong `.csproj`. Muốn xem giao diện: chọn Form, nhấn Shift+F7. Constructor không tham số dành cho Designer; khi chạy thật cần truyền ID người dùng đã đăng nhập.

Mẫu in RDLC/Crystal, in hóa đơn và biểu đồ dashboard thuộc nhiệm vụ 4. Bản này cung cấp dữ liệu báo cáo và CSV; CSV không thay thế yêu cầu mẫu in của cả nhóm.

## 3. Chạy trên Visual Studio 2022

1. Giải nén toàn bộ ZIP; mở `ConvenienceStoreManager.sln` ở thư mục gốc. Không mở rời một file `.cs`.
2. Máy cần workload **.NET desktop development**, .NET Framework **4.8 Developer/Targeting Pack**, SQL Server Express LocalDB hoặc SQL Server mà nhóm đang dùng.
3. Nhấp phải solution > **Restore NuGet Packages**. Giữ package EntityFramework **6.5.2** đang có của nhóm.
4. Mở `ConvenienceStore.UI/App.config` và `tools/ConvenienceStore.Task3Runner/App.config`. Hai file phải dùng cùng server/database. Nên thử trên database riêng, ví dụ đổi `Initial Catalog=ConvenienceStoreDB` thành `Initial Catalog=ConvenienceStore_NV3_Dev` ở CẢ HAI file. Nếu dùng SQL Server khác LocalDB, sửa Data Source đúng máy/instance của em.
5. Vào Tools > NuGet Package Manager > Package Manager Console; chạy:

   ```powershell
   Update-Database -ProjectName ConvenienceStore.Data -StartUpProjectName ConvenienceStore.UI
   ```

   Đã có migration `Task3Operations` cùng metadata/snapshot. Không cần Add-Migration lại và không xóa migration Init. Nếu dùng database đã ghép thêm code thành viên khác, nhóm trưởng cần hợp nhất model và migration trước khi chạy trên database chung.
6. Nhấp phải project **ConvenienceStore.Task3Runner** > **Set as Startup Project**. Ctrl+Shift+B, sau đó F5.
7. Đăng nhập tài khoản trong database. Seed gốc của nhóm tạo `admin` / `Admin@123`; tài khoản này chỉ dùng cho chạy thử theo cấu hình ban đầu. Nếu nhóm đã đổi mật khẩu, dùng mật khẩu hiện tại. Seed gốc ghi lại tài khoản admin khi Update-Database; nhóm trưởng nên điều chỉnh phần seed khi triển khai tài khoản thật.
8. Trong công cụ chạy thử, dùng nút **Tạo danh mục mẫu** trên database thử nghiệm. Chỉ Admin được dùng nút này. Tạo hai sản phẩm có tồn 0, một nhà cung cấp và một khách VIP; chạy lại không nhân đôi theo tên/mã mẫu.
9. Mở **Lập phiếu nhập**, chọn nhà cung cấp và `NV3 - Sữa hộp`, nhập 5 hộp với giá nhập 12.000, lưu. Tồn tăng từ 0 lên 5, tổng nhập 60.000.
10. Mở **Bán hàng**, chọn khách VIP mẫu, chọn 2 hộp sữa (giá bán 20.000), nhập tiền khách đưa 50.000 rồi thanh toán. Tiền hàng 40.000, giảm 4.000, phải trả 36.000, thừa 14.000; tồn còn 3.
11. Mở **Lịch sử kho**: có một biến động +5 và một biến động -2; tồn đầu 0 + nhập 5 - bán 2 = tồn cuối 3. Mở **Báo cáo doanh thu** cùng ngày: 1 hóa đơn, doanh thu 36.000. Phiếu nhập không tính vào doanh thu bán hàng.

## 4. Luồng xử lý và lý do chia file

- **UI/Forms**: đọc lựa chọn, giỏ hàng, thông báo; gọi service. Không viết câu SELECT/INSERT hoặc SqlCommand trong sự kiện nút bấm.
- **Business/Services/Task3Service.cs**: kiểm tra quyền, dữ liệu, tính tiền, giảm giá, điều phối lưu phiếu và kiểm tra tồn.
- **Business/Models/Task3Models.cs**: DTO cho giỏ hàng, yêu cầu lưu phiếu và kết quả báo cáo.
- **Data/Repositories**: session EF, LINQ và transaction; một DbContext mới cho mỗi thao tác.
- **Data/Entities**: bảng và quan hệ; giữ nguyên Product, Category, Role, User của nhóm. Product.Stock được thêm ConcurrencyCheck.

Thanh toán thực hiện theo thứ tự:

1. Mở transaction Serializable; đọc lại tài khoản, trạng thái hoạt động và nhóm quyền từ database.
2. Kiểm tra mã thao tác OperationId. Nếu thao tác đã lưu thì trả lại số hóa đơn cũ, không trừ kho lần nữa.
3. Đọc lại sản phẩm, GIÁ BÁN và tồn kho từ database; kiểm tra số lượng từng mặt hàng.
4. Lấy hạng VIP từ Customer.IsVip. Tính tiền bằng decimal, làm tròn 2 chữ số; giảm VIP 10%. Không tin đơn giá hoặc phần trăm giảm do Form gửi lên. Nếu tổng mới khác tổng em vừa thấy, từ chối để em tải lại và xác nhận.
5. Kiểm tra đủ tiền, tính tiền thừa; thêm hóa đơn, các dòng hàng, giảm Product.Stock và thêm StockMovement.
6. SaveChanges rồi Commit. Nếu bất kỳ bước nào lỗi, transaction chưa commit sẽ rollback. Bấm thử lại cùng phiếu giữ nguyên OperationId để tránh lưu trùng nếu mạng mất đúng lúc commit.

Nhập hàng cũng dùng transaction để phiếu, chi tiết, tăng tồn và lịch sử cùng thành công hoặc cùng hủy. Phiếu đã lưu là chứng từ; bản này không xóa/sửa trực tiếp chứng từ đã chốt. Giới hạn mỗi phiếu 500 sản phẩm khác nhau, số lượng mỗi dòng từ 1 đến 1.000.000; tiền tối đa 999.999.999.999,99. Đơn vị tiền VND; chưa bổ sung VAT, hoàn trả, công nợ hay thanh toán ngân hàng.

Lọc ngày sử dụng `>= ngày bắt đầu` và `< ngày sau ngày kết thúc`, nên không bỏ sót giao dịch lúc 23:59. Dữ liệu thời gian dùng giờ máy tính chạy ứng dụng; các máy trong nhóm nên thống nhất múi giờ Việt Nam. Tồn kỳ được suy ra từ tồn hiện tại và các biến động; điều kiện đối chiếu đúng là mọi nhập/xuất sau khi triển khai đều đi qua service này, không sửa tay cột Stock trong Form sản phẩm hoặc SQL.

## 5. Ghép vào menu và quyền của nhóm

Ví dụ code trong FrmMain sau khi module đăng nhập đã trả về User hợp lệ:

```csharp
using ConvenienceStore.UI.Forms;
using ConvenienceStore.Business.Services;
using ConvenienceStore.Business.Models;

// currentUser là tài khoản đã đăng nhập của module nhiệm vụ 1.
btnPurchase.Enabled = Task3Service.Can(currentUser.Role.Name, Task3Action.Purchase);
btnSales.Enabled = Task3Service.Can(currentUser.Role.Name, Task3Action.Sale);
btnStock.Enabled = Task3Service.Can(currentUser.Role.Name, Task3Action.Stock);
btnRevenue.Enabled = Task3Service.Can(currentUser.Role.Name, Task3Action.Revenue);

// Ví dụ trong sự kiện Click tương ứng:
using (var form = new FrmPurchaseReceipt(currentUser.Id)) form.ShowDialog(this);
using (var form = new FrmSales(currentUser.Id)) form.ShowDialog(this);
using (var form = new FrmStockMovement(currentUser.Id)) form.ShowDialog(this);
using (var form = new FrmRevenueReport(currentUser.Id)) form.ShowDialog(this);
```

Bốn dòng mở Form phía trên là bốn ví dụ cho BỐN sự kiện riêng, không dán cả bốn vào cùng một nút. Nếu dùng MDI thì tạo Form, đặt `MdiParent = this`, gọi `Show()` và không đặt trong using vì Form còn chạy sau sự kiện Click.

| Nhóm quyền (đúng tên đang dùng trong DB) | Nhập hàng | Bán hàng | Lịch sử kho | Doanh thu |
|---|---|---|---|---|
| Admin, QuanLy | Có | Có | Có | Có |
| ThuKho | Có | Không | Có | Không |
| ThuNgan | Không | Có | Không | Không |
| Khác / tài khoản khóa | Không | Không | Không | Không |

Can() dùng để bật/tắt menu; service vẫn đọc lại quyền từ DB ở mỗi thao tác. QuanLy là tên được hỗ trợ nếu nhóm thêm vai trò này; seed gốc mới có Admin/ThuKho/ThuNgan. Công cụ Task3Runner chỉ phục vụ thử module, không thay thế Login/Main của thành viên nhiệm vụ 1.

### Dữ liệu dùng chung cần thống nhất

Có 7 entity mới: Supplier, Customer, PurchaseReceipt, PurchaseReceiptLine, SaleInvoice, SaleInvoiceLine, StockMovement; enum StockMovementKind (Purchase=1, Sale=2). DbSet nằm trong CSDbContext gốc, không tạo database/context thứ hai.

- Bạn nhiệm vụ 2: dùng chung Supplier (Id, Name, Phone, IsActive), Customer (Id, Name, Phone, IsVip, IsActive). Đừng tạo thêm class cùng tên ở namespace khác. Nếu đã có model mới hơn, nhóm trưởng hợp nhất thuộc tính và migration trước khi merge. Product.Stock nên chỉ hiển thị đọc, tăng/giảm qua chứng từ.
- Bạn làm FrmInvoice: dữ liệu nằm ở CSDbContext.SaleInvoices và SaleInvoiceLines, quan hệ Lines. Tên, giá, giảm giá được lưu snapshot để đổi giá sản phẩm không làm đổi hóa đơn cũ.
- Bạn nhiệm vụ 4: Task3Service.Revenue(userId, from, to) trả RevenueData gồm Invoices/Days/Subtotal/Discount/Revenue để bind RDLC; CSV không phải mẫu in RDLC. Mã giảm giá/chồng chương trình khuyến mãi chưa nối vì model Promotion chưa có ở bản gốc; cần thống nhất quy tắc với bạn phụ trách trước khi mở rộng. VIP 10% hiện đã hoạt động.
- Nhóm trưởng: review CSDbContext, Product, 3 csproj và migration bổ sung. Program.cs gốc không thay đổi. Công cụ test có thể giữ dưới tools hoặc bỏ khỏi solution khi bàn giao cuối kỳ.

## 6. Bàn giao bằng Git

Thư mục ZIP là bản đầy đủ để chạy. File NhiemVu3.patch bên ngoài ZIP là thay đổi so với đúng commit gốc. Khi clone repo và chắc chắn đang ở đúng bản gốc, có thể dùng:

```bash
git switch -c feature/nhiem-vu-3
git apply --check /duong-dan/NhiemVu3.patch
git apply /duong-dan/NhiemVu3.patch
```

Đường dẫn phía trên phải thay bằng đường dẫn file thật trên máy em. Nếu --check báo lỗi thì repo đã có thay đổi trùng vùng; đối chiếu thủ công hoặc nhờ nhóm trưởng merge, không ghi đè toàn bộ bản mới bằng bản ZIP cũ. Review `git diff`, build, kiểm thử SQL trên máy rồi commit/push nhánh của em theo quyền nhóm cấp.

## 7. Kiểm tra đã làm và phần cần chạy trên Windows

Đã biên dịch Data, Business, UI và công cụ runner bằng Mono 6.8/C# 7.2, dùng đúng EntityFramework 6.5.2, coi cảnh báo là lỗi. Migration được sinh từ chênh lệch model EF của Init với model mới, có đủ metadata snapshot; không chỉnh sửa migration Init.

35 kiểm tra đã qua gồm nghiệp vụ với memory session có transaction mô phỏng, lỗi lưu/lỗi commit, lỗi dòng nhập thứ hai, không đủ tiền/hàng, không nhận đơn giá giả, quyền truy cập, chống thao tác lặp, lọc ngày, cân tồn và mở/đóng 4 Form; hai nút Thêm hàng được bấm thật trong kiểm tra giao diện. Ảnh xem trước là ứng dụng WinForms chạy trên Mono/Linux với dữ liệu test, không phải ảnh chạy SQL Server/Visual Studio.

CHƯA chạy Visual Studio Designer hoặc SQL Server/LocalDB thật trong môi trường tạo bản này. Không coi kiểm tra transaction mô phỏng là bằng chứng transaction SQL Server đã chạy.

Để tự kiểm tra SQL thật:

1. Build project `ConvenienceStore.Task3Tests` trong solution. File App.config của nó trỏ riêng đến `ConvenienceStore_Task3_Test`, không dùng DB chính.
2. Mở Terminal ở thư mục solution, chạy:

   ```powershell
   .\tools\ConvenienceStore.Task3Tests\bin\Debug\ConvenienceStore.Task3Tests.exe --sql
   ```

3. Chương trình chỉ cho chạy trên database có đúng tên `ConvenienceStore_Task3_Test`; tự áp dụng migration và tạo dữ liệu test. Nó kiểm tra nhập/bán, chống lưu trùng, cố ý gây lỗi SAU SaveChanges nhưng TRƯỚC Commit để kiểm tra rollback DB thật, và hai lần thanh toán đồng thời không bán vượt tồn.
4. Chạy không có `--sql` để thực hiện 29 kiểm tra nghiệp vụ memory session. Các test UI bổ sung được dùng khi truyền thư mục ảnh; không cần chạy để kiểm tra SQL.
5. Ngoài test tự động, thực hiện ví dụ 5 hộp / bán 2 hộp ở mục 3, thử tài khoản ThuKho/ThuNgan, mở Designer và thử in khi bạn nhiệm vụ 4 ghép mẫu in.

Tài liệu API tham khảo: [EF6 Transactions](https://learn.microsoft.com/en-us/ef/ef6/saving/transactions), [EF6 Code First Migrations](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/).
