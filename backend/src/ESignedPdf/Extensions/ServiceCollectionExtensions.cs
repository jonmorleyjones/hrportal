using ESignedPdf.Interfaces;
using ESignedPdf.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ESignedPdf.Extensions;

/// <summary>
/// Extension methods for registering ESignedPdf services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ESignedPdf services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddESignedPdf(this IServiceCollection services)
    {
        services.AddSingleton<ITimestampService, TimestampService>();
        services.AddSingleton<IPdfDocumentService, PdfDocumentService>();
        services.AddSingleton<IPdfSignatureService, PdfSignatureService>();
        services.AddSingleton<ISignatureVerificationService, SignatureVerificationService>();
        services.AddSingleton<ESignedPdfClient>();

        return services;
    }

    /// <summary>
    /// Adds ESignedPdf services with scoped lifetime (for per-request scenarios).
    /// </summary>
    public static IServiceCollection AddESignedPdfScoped(this IServiceCollection services)
    {
        services.AddScoped<ITimestampService, TimestampService>();
        services.AddScoped<IPdfDocumentService, PdfDocumentService>();
        services.AddScoped<IPdfSignatureService, PdfSignatureService>();
        services.AddScoped<ISignatureVerificationService, SignatureVerificationService>();
        services.AddScoped<ESignedPdfClient>();

        return services;
    }
}
