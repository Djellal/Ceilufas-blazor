using System;
using System.Linq;
using Ceilufas.Data;
using Ceilufas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ceilufas.Services
{
    public class Globals
    {
        // Role constants
        // These roles should be seeded in ApplicationDbContext
        public const string Admin = "Admin";
        public const string Student = "Student";
        public const string Teacher = "Teacher";
        
        // Array of roles for seeding
        public static readonly string[] Roles = new string[]
        {
            Admin,
            Student,
            Teacher
        };
        
        // Add properties and methods that you want to be globally accessible
        public string ApplicationName { get; set; } = "Ceilufas";
        
        // You can add more global properties or methods here
        
        private readonly IServiceProvider _serviceProvider;
        public Ceilufas.Models.AppSetting AppSettings { get; private set; }

        public Globals(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            LoadAppSettings();
        }

        private void LoadAppSettings()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            AppSettings = dbContext.AppSettings.FirstOrDefault();
            
            if (AppSettings == null)
            {
                // Create default settings if none exist
                AppSettings = new Ceilufas.Models.AppSetting
                {
                    OrganizationName = "Ceilufas",
                    Address = "Welcome to Ceilufas!",
                    Email = "Ceilufas@univ-setif.dz",
                    Tel = "Default"
                };
                
                dbContext.AppSettings.Add(AppSettings);
                dbContext.SaveChanges();
            }
        }

        public void UpdateAppSettings(AppSetting newSettings)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            if (newSettings.Id == 0)
            {
                // New settings
                dbContext.AppSettings.Add(newSettings);
            }
            else
            {
                // Update existing settings
                dbContext.AppSettings.Update(newSettings);
            }
            
            dbContext.SaveChanges();
            AppSettings = newSettings;
        }
    }


}