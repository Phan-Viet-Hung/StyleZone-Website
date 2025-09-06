using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Người gửi không được để trống.")]
        [MaxLength(50, ErrorMessage = "Tên người gửi không được vượt quá 50 ký tự.")]
        public string? Sender { get; set; }
        [Required(ErrorMessage = "Nội dung tin nhắn không được để trống.")]
        [MaxLength(1000, ErrorMessage = "Tin nhắn không được vượt quá 1000 ký tự.")]
        public string? Message { get; set; }

        public DateTime? SendAt { get; set; }
        [Required(ErrorMessage = "ChatSessionId không được để trống.")]
        [ForeignKey("ChatSession")]
        public Guid? ChatSessionId { get; set; }

        public virtual ChatSession? ChatSession { get; set; }
    }
}
