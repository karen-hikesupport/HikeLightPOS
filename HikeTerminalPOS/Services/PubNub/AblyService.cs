using System;
using System.Diagnostics;
using HikePOS.Helpers;
using IO.Ably;
using IO.Ably.Rest;
using IO.Ably.Realtime;
using PubnubApi;
using System.Threading.Tasks;

namespace HikePOS.Services.PubNub
{
    public class AblyService
    {
        AblyRealtime _ablyRealtime;
        #region Channels Name

        string ProductChannelName = "productChannel";
        string ProductDeleteChannelName = "productDeleteChannel";
        string CategoryChannelName = "categoryChannel";
        string OfferChannelName = "offerChannel";
        string StockUpdateChannelName = "stockUpdateChannel";
        string SaleChannelName = "saleChannel";
        string CustomerChannelName = "customerChannel";
        string ShopChannelName = "shopChannel";
        string UserChannelName = "userChannel";
        string PaymentTypeChannelName = "paymentTypeChannel";
        string RegisterActivityChannelName = "registerActivityChannel";
        string ProductWithVariantsChannelName = "productWithVariantsChannel";

        #endregion

        #region Ably Realtime Channel
        IRealtimeChannel ProductChannel;
        IRealtimeChannel ProductDeleteChannel;
        IRealtimeChannel CategoryChannel;
        IRealtimeChannel OfferChannel;
        IRealtimeChannel StockUpdateChannel;
        IRealtimeChannel SaleChannel;
        IRealtimeChannel CustomerChannel;
        IRealtimeChannel ShopChannel;
        IRealtimeChannel UserChannel;
        IRealtimeChannel PaymentTypeChannel;
        IRealtimeChannel RegisterActivityChannel;
        IRealtimeChannel ProductWithVariantsChannel;
        #endregion

        #region Events
        public event EventHandler<string> CustomerReceived;
        public event EventHandler<string> CustomerDeleted;

        public event EventHandler<string> CategoryReceived;
        public event EventHandler<string> CategoryDeleted;
        public event EventHandler<string> CategoryActiveDeActive;

        public event EventHandler<string> ProductReceived;
        public event EventHandler<string> ProductDeleted;
        public event EventHandler<string> ProductActiveDeActive;
        public event EventHandler<string> ProductStockReceived;

        public event EventHandler<string> OfferReceived;
        public event EventHandler<string> OfferDeleted;
        public event EventHandler<string> OfferActiveDeActive;
        public event EventHandler<string> OfferAssociateToProducts;

        public event EventHandler<string> InvoiceReceived;

        public event EventHandler<string> ShopSettingsReceived;

        public event EventHandler<string> RegisterReceived;

        public event EventHandler<string> UserReceived;

        public event EventHandler<string> PaymentTypesReceived;

        public event EventHandler<string> ReceiptTemplateReceived;

        public event EventHandler<string> ProductWithVariantsReceived;
        #endregion

        #region Configuration Keys

        //const string ABLY_KEY = "d7BMjQ.BawwLw:r3SKRBB4resZyZPk";// Old key
        
        
        // LIVE KEY
        const string ABLY_KEY = "d7BMjQ.mHVkNg:Doa4VUsE4cVQ3NcH";
        // ASY KEY
        //const string ABLY_KEY = "3ZsW-g.DAOTIg:DP3dHRkb5ZHdXuxb";

        //if (Settings.AppEnvironment != (int) Models.Enum.AppEnvironment.Live)
        //{
        //    env = "sandbox";

        //}

        #endregion

        public AblyService()
        {
            InitAsync();
            Publish();
        }
        void InitAsync()
        {

            if (_ablyRealtime == null)
            {
                var options = new ClientOptions(ABLY_KEY); //{ UseBinaryProtocol = true, Tls = true, AutoConnect = false, ClientId = clientId };
                _ablyRealtime = new AblyRealtime(options);
                _ablyRealtime.Connect();
            }

        }

        void Publish()
        {

            #region Ably Subscribe Channels
            ProductChannel = _ablyRealtime.Channels.Get(ProductChannelName + Settings.TenantId);//("administration");//this.channel.StateChanged += channel_ChannelStateChanged;
            ProductChannel.Subscribe((Message message) =>
            {
                ProductReceived?.Invoke(this, message.Data.ToString());
            });



            ProductDeleteChannel = _ablyRealtime.Channels.Get(ProductDeleteChannelName + Settings.TenantId);
            ProductDeleteChannel.Subscribe((Message message) =>
            {
                ProductDeleted?.Invoke(this, message.Data.ToString());
            });

            CategoryChannel = _ablyRealtime.Channels.Get(CategoryChannelName + Settings.TenantId);
            CategoryChannel.Subscribe((Message message) =>
            {
                CategoryReceived?.Invoke(this, message.Data.ToString());
            });

           

         

            OfferChannel = _ablyRealtime.Channels.Get(OfferChannelName + Settings.TenantId);
            OfferChannel.Subscribe((Message message) =>
            {
                OfferReceived?.Invoke(this, message.Data.ToString());
            });




            StockUpdateChannel = _ablyRealtime.Channels.Get(StockUpdateChannelName + Settings.TenantId);
            StockUpdateChannel.Subscribe((Message message) =>
            {
                ProductStockReceived?.Invoke(this, message.Data.ToString());
            });

            SaleChannel = _ablyRealtime.Channels.Get(SaleChannelName + Settings.TenantId);
            SaleChannel.Subscribe((Message message) =>
            {
                InvoiceReceived?.Invoke(this, message.Data.ToString());
            });

            CustomerChannel = _ablyRealtime.Channels.Get(CustomerChannelName + Settings.TenantId);
            CustomerChannel.Subscribe((Message message) =>
            {
                CustomerReceived?.Invoke(this, message.Data.ToString());
            });


            ShopChannel = _ablyRealtime.Channels.Get(ShopChannelName + Settings.TenantId);
            ShopChannel.Subscribe((Message message) =>
            {
                ShopSettingsReceived?.Invoke(this, message.Data.ToString());
            });

            UserChannel = _ablyRealtime.Channels.Get(UserChannelName + Settings.TenantId);
            UserChannel.Subscribe((Message message) =>
            {
                UserReceived?.Invoke(this, message.Data.ToString());
            });

            PaymentTypeChannel = _ablyRealtime.Channels.Get(PaymentTypeChannelName + Settings.TenantId);
            PaymentTypeChannel.Subscribe((Message message) =>
            {
                PaymentTypesReceived?.Invoke(this, message.Data.ToString());
            });

            RegisterActivityChannel = _ablyRealtime.Channels.Get(RegisterActivityChannelName + Settings.TenantId);
            RegisterActivityChannel.Subscribe((Message message) =>
            {
                RegisterReceived?.Invoke(this, message.Data.ToString());
            });

            ProductWithVariantsChannel = _ablyRealtime.Channels.Get(ProductWithVariantsChannelName + Settings.TenantId);
            ProductWithVariantsChannel.Subscribe((Message message) =>
            {
                ProductWithVariantsReceived?.Invoke(this, message.Data.ToString());
            });


            #endregion
        }
        public void releaseAblyChannels()
        {
            _ablyRealtime.Channels.ReleaseAll();
        }
    }
}
