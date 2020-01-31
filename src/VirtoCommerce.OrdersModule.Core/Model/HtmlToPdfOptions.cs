namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class HtmlToPdfOptions
    {
        public string ViewportSize { get; set; } = "1920x1080";
        public int? DPI { get; set; } = 300;
        public string DefaultEncoding { get; set; } = "utf-8";
        public int? MinimumFontSize { get; set; } = 10;
        public string PaperSize { get; set; } = "A4";
    }
}
