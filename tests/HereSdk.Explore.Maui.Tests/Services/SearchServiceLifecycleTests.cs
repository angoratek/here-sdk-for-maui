using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Tests.Services;

/// <summary>
/// Lifecycle contract for the real <see cref="SearchService"/> partial class.
///
/// These tests target the no-device partial (the stub). They lock down the
/// "Initialize() flips IsInitialized" contract that the
/// <c>UseHereSdkExplore</c> DI factory in
/// <c>HereSdk.Explore.Maui/HereSdkExtensions.cs</c> depends on. If a
/// regression breaks that wiring, the device + Appium tests will catch the
/// user-visible symptom ("SearchService not initialized") but these unit
/// tests will catch the structural cause first, in milliseconds, on every PR.
///
/// The platform-specific partials (SearchService.Android.cs and
/// SearchService.iOS.cs) implement <c>Initialize()</c> by constructing the
/// native engine. The shared partial in SearchService.cs implements
/// <c>IsInitialized</c> as <c>_engine is not null</c> on Android/iOS and
/// <c>_stubInitialized</c> in the no-device stub. Both forms flip on
/// <c>Initialize()</c>, so the contract is the same across platforms.
/// </summary>
public class SearchServiceLifecycleTests
{
    [Fact]
    public void SearchService_NotInitialized_HasIsInitializedFalse()
    {
        var service = new SearchService();
        Assert.False(service.IsInitialized);
    }

    [Fact]
    public void SearchService_AfterInitialize_HasIsInitializedTrue()
    {
        var service = new SearchService();
        service.Initialize();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void SearchService_Initialize_IsIdempotent()
    {
        var service = new SearchService();
        service.Initialize();
        service.Initialize(); // second call must not throw
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void SearchService_Dispose_DoesNotChangeIsInitialized()
    {
        // IsInitialized reflects engine creation, not lifetime. Disposal is
        // a separate concern; we don't want a disposed service to suddenly
        // report "not initialized" if a test resolves it after teardown.
        var service = new SearchService();
        service.Initialize();
        service.Dispose();
        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void SearchService_Dispose_IsIdempotent()
    {
        var service = new SearchService();
        service.Dispose();
        service.Dispose(); // must not throw
    }

    [Fact]
    public async Task SearchService_NotInitialized_SearchAsync_ThrowsNotImplemented()
    {
        // In the no-device stub, every method throws NotImplementedException
        // ("Platform-specific implementation required."). The platform
        // partials throw InvalidOperationException ("SearchService not
        // initialized.") instead. Both flavors are checked in their
        // respective device-test projects. The important invariant here is
        // that the method is not silently returning a fake result — a
        // regression that swallows the bug would do that.
        var service = new SearchService();
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.SearchAsync(new TextQuery("coffee"), new SearchOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.SuggestAsync(new TextQuery("co"), new SearchOptions()));
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.GetPlaceByIdAsync("p1"));
    }
}
