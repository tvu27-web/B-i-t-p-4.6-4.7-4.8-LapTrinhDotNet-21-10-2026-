using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StudentManager
{
    // Giả lập tầng truy cập dữ liệu (CSDL / File) tốn nhiều thời gian
    public static class DataService
    {
        private static readonly string[] Ho = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi" };
        private static readonly string[] Dem = { "Văn", "Thị", "Minh", "Hoàng", "Quốc", "Thu", "Gia", "Ngọc" };
        private static readonly string[] Ten = { "An", "Bình", "Cường", "Dũng", "Hà", "Hùng", "Lan", "Linh", "Nam", "Phúc", "Quang", "Trang" };
        private static readonly string[] SanPham = { "Bàn phím", "Chuột", "Màn hình", "Tai nghe", "Webcam", "Loa", "USB", "Ổ cứng SSD", "Router", "Laptop" };

        // async Task<List<Student>>: chạy việc nặng ở luồng nền bằng Task.Run
        public static async Task<List<Student>> LoadStudentsAsync(
            IProgress<int>? progress, bool simulateError, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                const int total = 100;
                var rnd = new Random(1);
                var list = new List<Student>();

                for (int i = 1; i <= total; i++)
                {
                    token.ThrowIfCancellationRequested();   // cho phép hủy khi đóng form
                    Thread.Sleep(40);                       // giả lập đọc CSDL chậm (đang ở luồng nền)

                    if (simulateError && i == 60)
                        throw new InvalidOperationException("Mất kết nối tới cơ sở dữ liệu (giả lập).");

                    list.Add(new Student
                    {
                        StudentId = "SV" + i.ToString("D3"),
                        FullName = Ho[rnd.Next(Ho.Length)] + " " + Dem[rnd.Next(Dem.Length)] + " " + Ten[rnd.Next(Ten.Length)],
                        Age = rnd.Next(18, 26),
                        Gender = rnd.Next(2) == 0 ? "Nam" : "Nữ"
                    });

                    progress?.Report(i * 100 / total);      // báo % về UI Thread
                }
                return list;
            }, token);
        }

        public static async Task<List<Product>> LoadProductsAsync(
            IProgress<int>? progress, bool simulateError, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                const int total = 80;
                var rnd = new Random(2);
                var list = new List<Product>();

                for (int i = 1; i <= total; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(50);

                    if (simulateError && i == 50)
                        throw new InvalidOperationException("Không đọc được file dữ liệu sản phẩm (giả lập).");

                    list.Add(new Product
                    {
                        ProductId = "SP" + i.ToString("D3"),
                        ProductName = SanPham[rnd.Next(SanPham.Length)] + " mẫu " + i,
                        Price = rnd.Next(5, 500) * 10000m,
                        Quantity = rnd.Next(1, 100)
                    });

                    progress?.Report(i * 100 / total);
                }
                return list;
            }, token);
        }
    }
}
