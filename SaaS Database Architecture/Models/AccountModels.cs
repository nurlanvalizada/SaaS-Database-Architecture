using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace SaaS_Database_Architecture.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext() : base("Administration") { }

        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<TenantProfile> TenantProfiles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        public UserProfile() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "İstifadəçi Adı")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Ünvanı")]
        public string EmailAdress { get; set; }

         [Display(Name = "İş Telefonu")]
        public string WorkPhone { get; set; }

        public virtual TenantProfile UserTenantProfile { get; set; }
    }

    [Table("TenantProfile")]
    public class TenantProfile
    {
        public TenantProfile()
        {
            UserProfiles = new List<UserProfile>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TenantId { get; set; }

        [Required]
        [Display(Name = "Müştəri Adı")]
        public string TenantName { get; set; }

        [Required]
        [Display(Name = "İstifadəçi Adı")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Ünvanı")]
        public string EmailAdress { get; set; }

         [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; }

         [Display(Name = "İşçi Sayı")]
        public long EmployeeCount { get; set; }

        public string TenantDatabaseName { get; set; }

        public string TenantDatabaseUsername { get; set; }

        public string TenantDatabasePassword { get; set; }

        public virtual ICollection<UserProfile> UserProfiles { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "İstifadəçi Adı")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; }

        [Display(Name = "Məni xatırla?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "İstifadəçi Adı")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email Ünvanı")]
        public string Email { get; set; }

         [Display(Name = "İş Telefonu")]
        public string WorkPhone { get; set; }
    }
}
