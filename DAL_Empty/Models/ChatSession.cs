using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class ChatSession
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public ChatStatusEnum Status { get; set; }

        public DateTime? CreateAt { get; set; }

        [ForeignKey("Customer")]
        public Guid? CustomerId { get; set; }

        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

        public virtual Customer? Customer { get; set; }
    }
    public enum ChatStatusEnum
    {
        Active = 1,     // Đang hoạt động
        Closed = 2,     // Đã đóng
        Waiting = 3,    // Đang chờ
        Timeout = 4     // Hết thời gian
    }
}
