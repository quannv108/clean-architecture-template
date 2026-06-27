using System.Net;
using SharedKernel;

namespace Domain.Emails;

public static class EmailErrors
{
    public static Error ToAddressRequired() => Error.Validation(
        "Email.ToAddressRequired",
        "The 'To' address is required.");

    public static Error SubjectRequired() => Error.Validation(
        "Email.SubjectRequired",
        "The email subject is required.");

    public static Error BodyRequired() => Error.Validation(
        "Email.BodyRequired",
        "The email body is required.");

    public static Error FromAddressEmpty() => Error.Validation(
        "Email.FromAddressEmpty",
        "The 'From' address was provided but is empty or whitespace.");

    public static Error CcAddressEmpty() => Error.Validation(
        "Email.CcAddressEmpty",
        "A CC address was provided but is empty or whitespace.");

    public static Error BccAddressEmpty() => Error.Validation(
        "Email.BccAddressEmpty",
        "A BCC address was provided but is empty or whitespace.");

    public static Error MissingFromAddress() => Error.Failure(
        "Email.MissingFromAddress",
        "No From address provided and no default FromAddress is configured.");

    public static Error SendFailed(string provider, HttpStatusCode statusCode) => Error.Failure(
        "Email.SendFailed",
        $"{provider} returned non-success status code {statusCode}.");

    public static Error Rejected(string provider, string message) => Error.Failure(
        "Email.Rejected",
        $"{provider} rejected the message: {message}");

    public static Error UnexpectedException(string provider, string message) => Error.Failure(
        "Email.UnexpectedException",
        $"{provider} threw an unexpected exception: {message}");
}
