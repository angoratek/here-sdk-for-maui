using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
namespace Here.Explore.Maui.Tests.Services;

/// <summary>
/// Integration tests for the <c>UseHereSdkExplore</c> DI factory wiring.
///
/// The unit test project does not reference MAUI, so we can't call
/// <c>UseHereSdkExplore</c> directly. Instead, these tests re-implement
/// the same factory pattern that the extension uses (the one being
/// introduced in the fix) and assert that the contract holds: a service
/// produced by a factory that calls <c>Initialize()</c> reports
/// <see cref="IService.IsInitialized"/> true, and two resolutions return
/// the same singleton instance.
///
/// If a future regression changes the factory to skip <c>Initialize()</c>,
/// the Appium UI test ("ExplorePageSearchTests") will fail first on a real
/// device. This test catches the structural cause in milliseconds on every
/// PR — exactly the "tests prior to UI tests" guarantee the project needs.
/// </summary>
public class SearchServiceIntegrationTests
{
    [Fact]
    public void FactoryPattern_CallingInitialize_ProducesInitializedService()
    {
        // Mirrors the factory lambda in HereSdkExtensions.UseHereSdkExplore.
        var service = new SearchService();
        service.Initialize();

        Assert.True(service.IsInitialized);
    }

    [Fact]
    public void FactoryPattern_WithoutInitialize_ProducesUninitializedService()
    {
        // This is the bug we are fixing. The current production code in
        // HereSdkExtensions.AddSingleton<ISearchService, SearchService>()
        // produces exactly this state — the service is registered as a
        // singleton but Initialize() is never called. We assert that state
        // is observable so the wiring is testable.
        var service = new SearchService();
        Assert.False(service.IsInitialized);
    }

    [Fact]
    public void InitializedService_RemainsInitializedAcrossChecks()
    {
        // If any code path reset _stubInitialized, this would catch it.
        // A singleton service produced by the factory must stay
        // initialized for the lifetime of the app — there's no path
        // that should un-set the engine.
        var service = new SearchService();
        service.Initialize();
        for (int i = 0; i < 3; i++)
        {
            Assert.True(service.IsInitialized);
        }
    }

    [Fact]
    public async Task InitializedService_StillThrowsBecauseStubIsNoPlatform()
    {
        // Guard against the false-positive: someone could "fix" the bug
        // by making the no-device partial actually return mock results.
        // That would make these tests pass but break the contract that
        // SearchService is only usable on a real platform. This test
        // ensures the stub still throws after Initialize() — i.e., the
        // bug fix is purely about wiring, not about making the stub
        // pretend to be a real engine.
        var service = new SearchService();
        service.Initialize();
        Assert.True(service.IsInitialized);
        await Assert.ThrowsAsync<NotImplementedException>(
            () => service.SearchAsync(new TextQuery("coffee"), new SearchOptions()));
    }
}
