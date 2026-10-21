using System;

namespace StudentManager
{
    // "Hợp đồng" giao tiếp giữa MDI Parent và MDI Child.
    public interface IChildForm
    {
        // Con -> Cha: con bắn sự kiện, cha nhận để cập nhật StatusStrip
        event Action<string> StatusMessage;

        // Cha -> Con: cha truyền dữ liệu (tên người đăng nhập) xuống con
        string CurrentUser { get; set; }

        // Cha -> Con: cha gọi hàm của con (VD: nút "Làm mới" trên ToolStrip)
        void ReloadData();
    }
}
