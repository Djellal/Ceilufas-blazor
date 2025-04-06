using System;

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
    }
}