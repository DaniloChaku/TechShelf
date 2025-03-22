namespace TechShelf.Domain.Orders;

public static class OrderConstants
{
    public const int EmailMaxLength = 256;
    public const int PhoneNumberMaxLength = 20;
    public const int FullNameMaxLength = 100;
    public const int CustomerIdMaxLength = 50;
    public const int PaymentIntentIdMaxLength = 50;

    public static class Address
    {
        public const int Line1MaxLength = 100;
        public const int Line2MaxLength = 100;
        public const int CityMaxLength = 50;
        public const int StateMaxLength = 50;
        public const int PostalCodeMaxLength = 10;
    }

    public static class OrderHistory
    {
        public const int NotesMaxLength = 500;
    }

    public static class OrderItem
    {
        public const int ProductNameMaxLength = 200;
        public const int ProductImageUrlMaxLength = 250;
    }
}
