using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudentManager
{
    // Hỗ trợ INotifyPropertyChanged để binding 2 chiều:
    // đổi dữ liệu trong code -> giao diện tự cập nhật.
    public class Student : INotifyPropertyChanged
    {
        private string _studentId = "";
        private string _fullName = "";
        private int _age;
        private string _gender = "Nam";
        private string _avatarPath = "";

        public string StudentId { get => _studentId; set => Set(ref _studentId, value); }
        public string FullName { get => _fullName; set => Set(ref _fullName, value); }
        public int Age { get => _age; set => Set(ref _age, value); }
        public string Gender { get => _gender; set => Set(ref _gender, value); }
        public string AvatarPath { get => _avatarPath; set => Set(ref _avatarPath, value); }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
