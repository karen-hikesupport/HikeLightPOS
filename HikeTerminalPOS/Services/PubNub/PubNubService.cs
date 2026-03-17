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
    public class PubNubService
    {
        Pubnub _pubnub;
       
        #region Channels Name

        string ProductChannelName = "productChannel";
        string ProductDeleteChannelName = "productDeleteChannel";
        string CategoryChannelName = "categoryChannel";
        string OfferChannelName = "offerChannel";
        string OfferAssociateToProductsChannelName = "offerAssociateToProductsChannel";
        string StockUpdateChannelName = "stockUpdateChannel";
        string SaleChannelName = "saleChannel";
        string CustomerChannelName = "customerChannel";
        string ShopChannelName = "shopChannel";
        string UserChannelName = "userChannel";
        string PaymentTypeChannelName = "paymentTypeChannel";
        string RegisterActivityChannelName = "registerActivityChannel";
        string ReceiptTemplateChannelName = "receiptTemplateChannel";
        string ProductWithVariantsChannelName = "productWithVariantsChannel";

        #endregion

        #region Ably Realtime Channel
        //IRealtimeChannel ProductChannel;
        //IRealtimeChannel ProductActiveChannel;
        //IRealtimeChannel ProductDeleteChannel;
        //IRealtimeChannel CategoryChannel;
        //IRealtimeChannel CategoryActiveChannel;
        //IRealtimeChannel CategoryDeleteChannel;
        //IRealtimeChannel OfferChannel;
        //IRealtimeChannel OfferActiveChannel;
        //IRealtimeChannel OfferDeleteChannel;
        //IRealtimeChannel OfferAssociateProductChannel;
        //IRealtimeChannel OfferAssociateToProductsChannel;
        //IRealtimeChannel StockUpdateChannel;
        //IRealtimeChannel SaleChannel;
        //IRealtimeChannel CustomerChannel;
        //IRealtimeChannel CustomerDeleteChannel;
        //IRealtimeChannel ShopChannel;
        //IRealtimeChannel UserChannel;
        //IRealtimeChannel PaymentTypeChannel;
        //IRealtimeChannel RegisterActivityChannel;
        //IRealtimeChannel ReceiptTemplateChannel;
        //IRealtimeChannel ProductWithVariantsChannel;
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
        //const string ABLY_KEY = "d7BMjQ.BawwLw:r3SKRBB4resZyZPk";

        const string PUBLISH_KEY = "pub-c-42444038-c5aa-420e-a9ab-1b7642f183e3";
        const string SUBSCRIBE_KEY = "sub-c-4a279052-fde5-11e4-bb05-02ee2ddab7fe";
        const string SECRET_KEY = "sec-c-YzFhYjE0YWEtYmI1ZS00MzhlLTkzYTAtZTk1OGJjMWI4ZDA4";
        const string CIPHER_KEY = "";
        const bool SSLON = true;
        #endregion


        public PubNubService()
        {
            InitAsync();
            Publish();
        }

        void InitAsync()
        {
            if (_pubnub == null)
            {
                UserId user = new UserId($"pn-{Guid.NewGuid().ToString()}");
                
                _pubnub = new Pubnub(new PNConfiguration(user)
                {
                    PublishKey = PUBLISH_KEY,
                    SubscribeKey = SUBSCRIBE_KEY,
                    SecretKey = SECRET_KEY,
                    CipherKey = CIPHER_KEY,
                    Secure = SSLON,
                    ReconnectionPolicy = PNReconnectionPolicy.LINEAR
                });
            }
        }

        void Publish()
        {
            #region pubnub Subscribe Channel
            _pubnub.AddListener(new SubscribeCallbackExt((pubnubObj, message) =>
                {
                    if (message != null)
                    {
                        if (message.Channel == ProductChannelName + Settings.TenantId)
                        {
                            ProductReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == ProductDeleteChannelName + Settings.TenantId)
                        {
                            ProductDeleted?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == StockUpdateChannelName + Settings.TenantId)
                        {
                            ProductStockReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == CategoryChannelName + Settings.TenantId)
                        {
                            CategoryReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == OfferChannelName + Settings.TenantId)
                        {
                            OfferReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == OfferAssociateToProductsChannelName + Settings.TenantId)
                        {
                            OfferAssociateToProducts?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == SaleChannelName + Settings.TenantId)
                        {
                            InvoiceReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == CustomerChannelName + Settings.TenantId)
                        {
                            CustomerReceived?.Invoke(this, message.Message.ToString());
                        }
                        //if (message.Channel == CustomerDeleteChannelName + Settings.TenantId)
                        //{
                        //    CustomerDeleted?.Invoke(this, message.Message.ToString());
                        //}
                        if (message.Channel == ShopChannelName + Settings.TenantId)
                        {
                            ShopSettingsReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == UserChannelName + Settings.TenantId)
                        {
                            UserReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == PaymentTypeChannelName + Settings.TenantId)
                        {
                            PaymentTypesReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == RegisterActivityChannelName + Settings.TenantId)
                        {
                            RegisterReceived?.Invoke(this, message.Message.ToString());
                        }
                        if (message.Channel == ProductWithVariantsChannelName + Settings.TenantId)
                        {
                            ProductWithVariantsReceived?.Invoke(this, message.Message.ToString());
                        }
                    }
                },
            (pubnubObj, presence) => { },
            (pubnubObj, status) =>
            {
                if (status.Category == PNStatusCategory.PNUnexpectedDisconnectCategory)
                {
                    // This event happens when radio / connectivity is lost
                }
                else if (status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    // Connect event. You can do stuff like publish, and know you'll get it.
                    // Or just use the connected event to confirm you are subscribed for
                    // UI / internal notifications, etc
                }
                else if (status.Category == PNStatusCategory.PNReconnectedCategory)
                {
                    // Happens as part of our regular operation. This event happens when
                    // radio / connectivity is lost, then regained.
                }
                else if (status.Category == PNStatusCategory.PNDecryptionErrorCategory)
                {
                    // Handle messsage decryption error. Probably client configured to
                    // encrypt messages and on live data feed it received plain text.
                }
            }
            ));
            _pubnub.Subscribe<string>()
               .Channels(new string[]
            {
                ProductChannelName + Settings.TenantId,
                ProductDeleteChannelName + Settings.TenantId,
                CategoryChannelName + Settings.TenantId,
                OfferChannelName + Settings.TenantId,
                StockUpdateChannelName + Settings.TenantId,
                SaleChannelName + Settings.TenantId,
                CustomerChannelName + Settings.TenantId,
                ShopChannelName + Settings.TenantId,
                UserChannelName + Settings.TenantId,
                PaymentTypeChannelName + Settings.TenantId,
                RegisterActivityChannelName + Settings.TenantId,
                ReceiptTemplateChannelName + Settings.TenantId,
                ProductWithVariantsChannelName + Settings.TenantId
            }).Execute();
            #endregion
        }

    }
}
