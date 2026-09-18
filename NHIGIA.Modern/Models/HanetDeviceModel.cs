using System.ComponentModel.DataAnnotations;
namespace NHIGIA.Modern.Models;
public sealed class HanetDeviceModel
{
 public int Id { get; set; }
 [Required, StringLength(100)] public string DeviceId { get; set; }
 [Required, StringLength(150)] public string Name { get; set; }
 [Required, StringLength(100)] public string PlaceId { get; set; }
 [StringLength(250)] public string Location { get; set; }
 [StringLength(1000)] public string Notes { get; set; }
 public bool IsActive { get; set; } = true;
}
