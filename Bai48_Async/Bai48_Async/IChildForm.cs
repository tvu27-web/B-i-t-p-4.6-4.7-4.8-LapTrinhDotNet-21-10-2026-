using System;

namespace StudentManager
{
    // "Hợp đồng" giao tiếp giữa MDI Parent và MDI Child.
    public interface IChildForm
    {
        // Con -> Cha: thông điệp hiển thị trên StatusStrip
        event Action<string> StatusMessage;

        // Con -> Cha: phần trăm tiến trình (0-100). Giá trị -1 = ẩn thanh tiến trình
        event Action<int> ProgressChanged;

        // Cha -> Con: truyền tên người đăng nhập
        string CurrentUser { get; set; }

        // Cha -> Con: gọi hàm làm mới dữ liệu
        void ReloadData();
    }
}
