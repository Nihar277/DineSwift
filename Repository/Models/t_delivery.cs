
using Repository;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;


namespace Repository.Model;

    public class t_delivery
    {
        [Key]
        [Column("c_d_id")]
        public int c_d_id { get; set; }
        

        // --------- Foreign Key to Customer ----------
        
        [Required(ErrorMessage = "Customer ID is required")]       
        public int c_customerid { get; set; }


        // --------- Bank Details ----------
        [Required(ErrorMessage = "Account number is required")]
        [Column("c_d_ac")]
        [RegularExpression(@"^\d{9,18}$", ErrorMessage = "Account number must be 9 to 18 digits")]
        public long c_d_ac { get; set; }

        [Required(ErrorMessage = "IFSC code is required")]
        [Column("c_d_ifsc")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "IFSC must be 11 characters")]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC format")]
        public string c_d_ifsc { get; set; }

        // --------- Image paths (stored in DB) ----------
        [Column("c_d_licence_img")]
        public string? c_d_licence { get; set; }

        [Column("c_d_vehicle_img")]
        public string? c_d_vehicle { get; set; }

        [Column("c_d_aadhar_img")]
        public string? c_d_aadhar { get; set; }

        // --------- Image files (NOT stored in DB) ----------
        [NotMapped]
        [Required(ErrorMessage = "Licence image is required")]
        public IFormFile? LicenceImageFile { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Vehicle image is required")]
        public IFormFile? VehicleImageFile { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Aadhar image is required")]
        public IFormFile? AadharImageFile { get; set; }

        // --------- Status ----------
        [Column("c_d_status")]
        public string c_d_status { get; set; } = "PENDING";

        [Column("c_d_isavailable")]
        public bool c_d_isavailable { get; set; } = false;

        [Column("c_created_at")]
        public DateTime c_created_at { get; set; } = DateTime.Now;
    }


