using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Java.Security;
using Java.Security.Cert;
using Javax.Net.Ssl;
using Xamarin.Android.Net;

namespace HikePOS.Handlers;

public class AdyenHttpHandler : AndroidMessageHandler
{
    private static SSLSocketFactory _cachedSocketFactory;

    protected override SSLSocketFactory ConfigureCustomSSLSocketFactory(HttpsURLConnection connection)
    {
        if (_cachedSocketFactory == null)
        {
            _cachedSocketFactory = CreatePinnedSslContext().SocketFactory;
        }
        return _cachedSocketFactory;

    }

    protected override IHostnameVerifier GetSSLHostnameVerifier(HttpsURLConnection connection)
    {
        return new AllowAnyHostnameVerifier();
    }
    public class AllowAnyHostnameVerifier : Java.Lang.Object, IHostnameVerifier
    {
        public bool Verify(string hostname, ISSLSession session)
        {
            // Accept all hostnames — required for local IPs with Adyen terminals
            return true;
        }
    }
    private SSLContext CreatePinnedSslContext()
    {
        // 1. Load the Certificate from Assets
        // Ensure adyen_test.crt Build Action is "AndroidAsset"
        using var asset = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.Assets.Open(HikePOS.Helpers.Settings.AppEnvironment == (int)Models.Enum.AppEnvironment.Live ? "adyen-terminalfleet-live.crt" : "adyen-terminalfleet-test.crt");
        var cf = CertificateFactory.GetInstance("X.509");
        var adyenRootCertificate = cf.GenerateCertificate(asset);

        // 2. Create and initialize the KeyStore
        KeyStore keyStore = KeyStore.GetInstance(KeyStore.DefaultType);
        keyStore.Load(null, null);
        keyStore.SetCertificateEntry("adyenRootCertificate", adyenRootCertificate);

        // 3. Create TrustManagerFactory
        string tmfAlgorithm = TrustManagerFactory.DefaultAlgorithm;
        TrustManagerFactory trustManagerFactory = TrustManagerFactory.GetInstance(tmfAlgorithm);
        trustManagerFactory.Init(keyStore);

        // 4. Create SSLContext (Use "TLS" instead of "SSL" for modern Android)
        SSLContext sslContext = SSLContext.GetInstance("TLSv1.2");
        sslContext.Init(null, trustManagerFactory.GetTrustManagers(), new SecureRandom());

        // 5. Create the Native Handler
        // This is the CRITICAL bridge that makes HttpClient use the Java SSL settings
        return sslContext;
    }
}
