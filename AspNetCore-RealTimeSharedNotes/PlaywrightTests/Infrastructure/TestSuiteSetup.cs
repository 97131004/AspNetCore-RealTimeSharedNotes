using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCore_RealTimeSharedNotes.PlaywrightTests;

//starts the main project on a free port via kestrel so playwright connects like a real browser.
public class AppFixture : WebApplicationFactory<Program>
{
    private IHost? _kestrelHost;

    public string BaseUrl { get; private set; } = string.Empty;

    public string GetConfig(string key)
    {
        return _kestrelHost!.Services.GetRequiredService<IConfiguration>()[key] ?? string.Empty;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var dummyHost = builder.Build();

        //build and start a second host backed by a real kestrel socket on a free port
        builder.ConfigureWebHost(web =>
            web.UseKestrel(opts =>
                opts.Listen(IPAddress.Loopback, 0, o => o.Protocols = HttpProtocols.Http1)));

        _kestrelHost = builder.Build();
        _kestrelHost.Start();

        var addresses = _kestrelHost.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!
            .Addresses;

        BaseUrl = addresses.First();

        return dummyHost;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _kestrelHost != null)
        {
            _kestrelHost.StopAsync().GetAwaiter().GetResult();
            _kestrelHost.Dispose();
            _kestrelHost = null;
        }
        base.Dispose(disposing);
    }
}

//runs only once per test suite
[SetUpFixture]
public class TestSuiteSetup
{
    private static AppFixture? _fixture;

    [OneTimeSetUp]
    public void StartApp()
    {
        _fixture = new AppFixture();
        _fixture.CreateClient(); //host creation and kestrel starts

        //test config setup
        Config.TestConfig.BaseUrl = _fixture.BaseUrl;
        Config.TestConfig.SuperAdminEmail = _fixture.GetConfig("SuperAdmin:Email");
        Config.TestConfig.SuperAdminPassword = _fixture.GetConfig("SuperAdmin:Password");
    }

    [OneTimeTearDown]
    public void StopApp()
    {
        _fixture?.Dispose();
    }
}
