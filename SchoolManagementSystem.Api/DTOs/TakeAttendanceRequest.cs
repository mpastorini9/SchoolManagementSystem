using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;  
using Microsoft.AspNetCore.Http;
using SchoolManagementSystem.Api.Models;                                        


namespace SchoolManagementSystem.Api.DTOs
{
    public class TakeAttendanceRequest
    {
        public int CourseId { get; set; }
        public DateTime Date { get; set; }
        public List<int> AbsentStudentsIds { get; set; } = new();
        
    }

    
}

