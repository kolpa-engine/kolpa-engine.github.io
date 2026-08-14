using System.Reflection;
using SixLabors.ImageSharp;

namespace Kolpa.Generator.Services;

/// <summary>
/// Invokes the AVIF encoder via reflection so AVIF support works when the linked
/// ImageSharp build exposes it, while remaining compatible with builds that do not.
/// </summary>
internal static class AvifEncoderHelper
{
    private static readonly Type? _encoderType = TryFindEncoder();

    public static async Task SaveAsync(
        Image image,
        Stream stream,
        int quality,
        CancellationToken cancellationToken
    )
    {
        if (_encoderType == null)
        {
            throw new NotSupportedException(
                "AVIF encoder is not available in the linked ImageSharp build."
            );
        }

        var encoder = Activator.CreateInstance(_encoderType)!;
        _encoderType.GetProperty("Quality")?.SetValue(encoder, quality);

        var extensionType = typeof(Image).Assembly.GetType("SixLabors.ImageSharp.ImageExtensions");
        if (extensionType == null)
        {
            throw new NotSupportedException("ImageSharp ImageExtensions type was not found.");
        }

        var saveMethod = extensionType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == "SaveAsync"
                && m.GetParameters().Length == 4
                && m.GetParameters()[2].ParameterType == _encoderType
            );

        if (saveMethod == null)
        {
            throw new NotSupportedException("AVIF SaveAsync overload was not found.");
        }

        var task = (Task)
            saveMethod.Invoke(null, [image, stream, encoder, cancellationToken])!;
        await task.ConfigureAwait(false);
    }

    private static Type? TryFindEncoder()
    {
        return typeof(Image).Assembly.GetType("SixLabors.ImageSharp.Formats.Avif.AvifEncoder");
    }
}
