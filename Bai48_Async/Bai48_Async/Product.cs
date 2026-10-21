using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudentManager
{
    public class Product : INotifyPropertyChanged
    {
        private string _id = "";
        private string _name = "";
        private decimal _price;
        private int _quantity;

        public string ProductId { get => _id; set => Set(ref _id, value); }
        public string ProductName { get => _name; set => Set(ref _name, value); }
        public decimal Price { get => _price; set => Set(ref _price, value); }
        public int Quantity { get => _quantity; set => Set(ref _quantity, value); }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Kho dữ liệu dùng chung: đóng/mở lại form con vẫn giữ nguyên dữ liệu
    public static class DataStore
    {
        public static readonly BindingList<Student> Students = new BindingList<Student>
        {
            new Student { StudentId = "SV001", FullName = "Nguyễn Văn An",  Age = 20, Gender = "Nam" },
            new Student { StudentId = "SV002", FullName = "Trần Thị Bình",  Age = 19, Gender = "Nữ"  },
            new Student { StudentId = "SV003", FullName = "Lê Hoàng Cường", Age = 21, Gender = "Nam" }
        };

        public static readonly BindingList<Product> Products = new BindingList<Product>
        {
            new Product { ProductId = "SP001", ProductName = "Bàn phím cơ",  Price = 850000,  Quantity = 15 },
            new Product { ProductId = "SP002", ProductName = "Chuột không dây", Price = 320000, Quantity = 40 },
            new Product { ProductId = "SP003", ProductName = "Màn hình 24 inch", Price = 3200000, Quantity = 8 }
        };
    }
}
