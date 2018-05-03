namespace SoftwareMonitoringSystem
{
    using SQLite.CodeFirst;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Globalization;

    public class SMSDBContext : DbContext
    {
        // Your context has been configured to use a 'SMSDBContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'SoftwareMonitoringSystem.SMSDBContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'SMSDBContext' 
        // connection string in the application configuration file.
        public SMSDBContext()
            : base("SMSDBContext")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<SMSDBContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);

            //base.OnModelCreating(modelBuilder);
        }
        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public DbSet<ScanAndDevice> ScansAndDevices { get; set; }
        public DbSet<Scan> Scans { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Setting> Settings { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
    [Table("ScansAndDevices")]
    public class ScanAndDevice
    {
        public ScanAndDevice()
        {
            IsSuccessful = 0;
        }

        [Key, ForeignKey("Scan"), Column(Order = 0), Required]
        public int ScanID { get; set; }
        [Key, ForeignKey("Device"), Column(Order = 1), Required]
        public int DeviceID { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public int IsSuccessful { get; set; }
        public virtual Scan Scan { get; set; }
        public virtual Device Device { get; set; }
    }
    [Table("Scans")]
    public class Scan
    {
        public Scan()
        {
            ScansAndDevices = new HashSet<ScanAndDevice>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Autoincrement]
        public int ScanID { get; set; }
        [Required]
        public DateTime ScanDateTime { get; set; }
        public virtual ICollection<ScanAndDevice> ScansAndDevices { get; set; }
    }
    [Table("Devices")]
    public class Device
    {
        public Device()
        {
            CreationDate = DateTime.Now;
            IsActive = 0;
            ScansAndDevices = new HashSet<ScanAndDevice>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Autoincrement]
        public int DeviceID { get; set; }
        [MinLength(17), MaxLength(17), Required]
        public string MACAddress { get; set; }
        [MaxLength(64)]
        public string Manufacturer { get; set; }
        [MaxLength(45)]
        public string IPAddress { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        public Nullable<DateTime> LastEditDate { get; set; }
        [Required]
        public int IsActive { get; set; }
        public virtual ICollection<ScanAndDevice> ScansAndDevices { get; set; }
    }
    [Table("Admins")]
    public class Admin
    {
        public Admin()
        {
            LogInAttemptCounter = 0;
            LastEditDate = DateTime.MinValue;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Autoincrement]
        public int AdminID { get; set; }
        [MinLength(5), MaxLength(5), Required]
        public string Username { get; set; }
        [MaxLength(128), Required]
        public string Password { get; set; }
        [Required, Range(0, 3)]
        public int LogInAttemptCounter { get; set; }
        public Nullable<DateTime> LastLogInAttemptDate { get; set; }
        public DateTime LastEditDate { get; set; }
    }
    [Table("Settings")]
    public class Setting
    {
        public Setting()
        {
            this.Password = "3RFbUcJvdIox+NK6bKofC7NVxjgbx8PkMBrRAkjhduQ=";
        }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Autoincrement]
        public int SettingID { get; set; }
        [MaxLength(44), Required]
        public string Password { get; set; }

    }
}