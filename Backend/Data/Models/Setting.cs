namespace Data.Models
{
    public class Setting
    {
        public int Id { get; set; }
        public string? SettingName { get; set; } 
        public string? Value { get; set; }
        public bool Applied { get; set; }
    }
}
