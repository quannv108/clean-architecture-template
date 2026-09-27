namespace SharedKernel.PhoneNumbers;

public static class PhoneNumberErrors
{
    public static Error CallingCodeRequired =>
        Error.Validation("PhoneNumber.CallingCodeRequired", "Calling code cannot be null or empty");

    public static Error PhoneNumberRequired =>
        Error.Validation("PhoneNumber.PhoneNumberRequired", "Phone number cannot be null or empty");

    public static Error InvalidFormat =>
        Error.Validation("PhoneNumber.InvalidFormat", "The phone number format is invalid. Use E.164 format (e.g. +447911123456)");

    public static Error InvalidNumber =>
        Error.Validation("PhoneNumber.InvalidNumber", "The phone number is not a recognised valid number");
}
