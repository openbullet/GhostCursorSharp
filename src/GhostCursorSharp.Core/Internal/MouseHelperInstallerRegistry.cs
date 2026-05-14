namespace GhostCursorSharp.Internal;

internal static class MouseHelperInstallerRegistry
{
    private static readonly Lock RegistrationLock = new();
    private static readonly List<Registration> Registrations = [];

    public static void Register<TPage>(Func<TPage, Task<MouseHelperInstallation>> installer)
        where TPage : class
    {
        lock (RegistrationLock)
        {
            Registrations.RemoveAll(static r => r.PageType == typeof(TPage));
            Registrations.Add(new Registration(typeof(TPage), page => installer((TPage)page)));
        }
    }

    public static Task<MouseHelperInstallation> InstallAsync(object page)
    {
        Registration? registration;

        lock (RegistrationLock)
        {
            registration = Registrations.LastOrDefault(r => r.PageType.IsInstanceOfType(page));
        }

        if (registration is null)
        {
            throw new NotSupportedException(
                $"No mouse helper installer has been registered for page type '{page.GetType().FullName}'.");
        }

        return registration.Installer(page);
    }

    private sealed record Registration(
        Type PageType,
        Func<object, Task<MouseHelperInstallation>> Installer);
}
