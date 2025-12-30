namespace RestoBook_MiniProjet.ViewModels.Menu
{
    public class PublicMenuViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<MenuItemViewModel> Items { get; set; } = new();
    }
}