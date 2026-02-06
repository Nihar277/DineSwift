using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Repository.Model;
public class CustomerProfileUpdateDto
{
    
    public int c_customerid { get; set; }
    public string? c_fname { get; set; }
    public string? c_lname { get; set; }
    public string? c_state { get; set; }
    public string? c_city { get; set; }
    public string? c_pincode { get; set; }
    public string? c_gender { get; set; }
    public string? c_address { get; set; }
    public string? c_phonenumber { get; set; }
    public string? c_email { get; set; }
    public string? c_image { get; set; }
    public IFormFile? c_imagefile { get; set; }
}