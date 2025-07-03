using System.ComponentModel.DataAnnotations;

namespace SchedulerV4.Models
{
    public class UserEntity
    {
        
        public int ID { get; set; }

        [Required(ErrorMessage = "Имя пользователя обязательно для заполнения.")]
        public string NAME { get; set; }

        [Required(ErrorMessage = "Email пользователя обязателен для заполнения.")]
        [EmailAddress(ErrorMessage = "Некорректный формат email.")]
        public string EMAIL { get; set; }

        public int? GROUP_ID { get; set; } // Разрешаем GROUP_ID быть null

    }

    public class GroupEntity
    {
        public int ID { get; set; }
        public string NANE { get; set; }
    }


    public class UserWithGroups
    {

        public int UserID {  get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public int? GroupID { get; set; }

         public string? GroupName { get; set; }

    }
}
