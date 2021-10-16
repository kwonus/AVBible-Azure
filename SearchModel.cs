using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

public class SearchModel
{
    [Required]
    [StringLength(4096, ErrorMessage = "Search specification is too long.")]
    public string SearchSpec { get; set; }
}
